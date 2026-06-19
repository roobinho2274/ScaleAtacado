using Microsoft.AspNetCore.SignalR.Client;
using ScaleAtacado.Domain.Enums;
using ScaleAtacado.PrintAgent.Services;

namespace ScaleAtacado.PrintAgent;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;
    private readonly PrintApiClient _apiClient;
    private readonly PrintService _printService;
    private readonly ReceiptFormatter _formatter;

    private HubConnection? _hubConnection;

    public Worker(
        ILogger<Worker> logger,
        IConfiguration configuration,
        PrintApiClient apiClient,
        PrintService printService,
        ReceiptFormatter formatter)
    {
        _logger = logger;
        _configuration = configuration;
        _apiClient = apiClient;
        _printService = printService;
        _formatter = formatter;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ConnectToHubAsync(stoppingToken);
        await RunFallbackAsync(stoppingToken);

        var fallbackInterval = TimeSpan.FromMinutes(
            _configuration.GetValue("PrintAgent:FallbackIntervalMinutes", 5));

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(fallbackInterval, stoppingToken);

            if (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Verificando jobs pendentes (fallback)...");
                await RunFallbackAsync(stoppingToken);
            }
        }
    }

    private async Task ConnectToHubAsync(CancellationToken stoppingToken)
    {
        var apiUrl = _configuration["PrintAgent:ApiUrl"]!;
        var agentKey = _configuration["PrintAgent:AgentKey"]!;
        var hubUrl = $"{apiUrl}/hubs/print?agentKey={agentKey}";

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<Guid>("NewPrintJob", async printJobId =>
        {
            _logger.LogInformation("Novo job de impressão recebido: {JobId}", printJobId);
            await ProcessJobAsync(printJobId);
        });

        _hubConnection.Reconnected += connectionId =>
        {
            _logger.LogInformation("Reconectado ao hub. ConnectionId: {Id}", connectionId);
            return Task.CompletedTask;
        };

        try
        {
            await _hubConnection.StartAsync(stoppingToken);
            _logger.LogInformation("Conectado ao PrintHub com sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Não foi possível conectar ao PrintHub. Operando apenas no modo fallback.");
        }
    }

    private async Task RunFallbackAsync(CancellationToken stoppingToken)
    {
        var jobs = await _apiClient.GetPendingJobsAsync();
        foreach (var job in jobs)
        {
            if (stoppingToken.IsCancellationRequested) break;
            await ProcessJobAsync(job.PrintJobId);
        }
    }

    private async Task ProcessJobAsync(Guid printJobId)
    {
        _logger.LogInformation("Processando job {JobId}...", printJobId);

        await _apiClient.UpdateStatusAsync(printJobId, PrintStatus.Printing);

        var order = await _apiClient.GetOrderForAgentAsync(printJobId);
        if (order == null)
        {
            _logger.LogError("Pedido não encontrado para o job {JobId}.", printJobId);
            await _apiClient.UpdateStatusAsync(printJobId, PrintStatus.Canceled, "Pedido não encontrado.");
            return;
        }

        var receipt = _formatter.Format(order);
        var printerName = _configuration["PrintAgent:PrinterName"];
        var success = _printService.Print(receipt, printerName);

        if (success)
        {
            await _apiClient.UpdateStatusAsync(printJobId, PrintStatus.Printed);
            _logger.LogInformation("Job {JobId} impresso com sucesso.", printJobId);
        }
        else
        {
            await _apiClient.UpdateStatusAsync(printJobId, PrintStatus.Canceled, "Falha ao enviar para impressora.");
            _logger.LogError("Falha ao imprimir job {JobId}.", printJobId);
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        if (_hubConnection != null)
            await _hubConnection.DisposeAsync();

        await base.StopAsync(stoppingToken);
    }
}
