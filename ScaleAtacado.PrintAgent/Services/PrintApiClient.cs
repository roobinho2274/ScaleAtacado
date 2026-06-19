using System.Net.Http.Json;
using ScaleAtacado.Application.DTOs;
using ScaleAtacado.Domain.Enums;
using ScaleAtacado.Shared.Common;

namespace ScaleAtacado.PrintAgent.Services;

public class PrintApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PrintApiClient> _logger;

    public PrintApiClient(HttpClient httpClient, ILogger<PrintApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IEnumerable<PendingPrintJobDto>> GetPendingJobsAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<IEnumerable<PendingPrintJobDto>>>(
                "api/printjob/pending");
            return response?.Data ?? Enumerable.Empty<PendingPrintJobDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar jobs pendentes.");
            return Enumerable.Empty<PendingPrintJobDto>();
        }
    }

    public async Task<OrderResponseDto?> GetOrderAsync(Guid orderId, string jwtToken)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/pedido/{orderId}");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<OrderResponseDto>>();
            return result?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar pedido {OrderId}.", orderId);
            return null;
        }
    }

    public async Task<OrderResponseDto?> GetOrderForAgentAsync(Guid printJobId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<OrderResponseDto>>(
                $"api/printjob/{printJobId}/order");
            return response?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar pedido para job {JobId}.", printJobId);
            return null;
        }
    }

    public async Task UpdateStatusAsync(Guid printJobId, PrintStatus status, string? errorMessage = null)
    {
        try
        {
            var dto = new UpdatePrintJobStatusDto(status, errorMessage);
            await _httpClient.PatchAsJsonAsync($"api/printjob/{printJobId}/status", dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar status do job {JobId}.", printJobId);
        }
    }
}
