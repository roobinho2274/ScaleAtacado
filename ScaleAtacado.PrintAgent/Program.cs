using ScaleAtacado.PrintAgent;
using ScaleAtacado.PrintAgent.Services;
using WinForms = System.Windows.Forms;

namespace ScaleAtacado.PrintAgent;

internal static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        WinForms.Application.EnableVisualStyles();
        WinForms.Application.SetCompatibleTextRenderingDefault(false);

        var builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddHttpClient<PrintApiClient>(client =>
        {
            client.BaseAddress = new Uri(builder.Configuration["PrintAgent:ApiUrl"]!);
            client.DefaultRequestHeaders.Add("X-Agent-Key", builder.Configuration["PrintAgent:AgentKey"]);
        });

        builder.Services.AddSingleton<AgentStatus>();
        builder.Services.AddSingleton<PrintService>();
        builder.Services.AddSingleton<ReceiptFormatter>();
        builder.Services.AddHostedService<Worker>();

        var host = builder.Build();
        host.Start();

        using var trayContext = new TrayApplicationContext(host.Services);
        WinForms.Application.Run(trayContext);

        host.StopAsync().GetAwaiter().GetResult();
    }
}
