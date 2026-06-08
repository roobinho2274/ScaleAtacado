using ScaleAtacado.PrintAgent;
using ScaleAtacado.PrintAgent.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHttpClient<PrintApiClient>(client =>
{
    var apiUrl = builder.Configuration["PrintAgent:ApiUrl"]!;
    client.BaseAddress = new Uri(apiUrl);
    client.DefaultRequestHeaders.Add("X-Agent-Key", builder.Configuration["PrintAgent:AgentKey"]);
});

builder.Services.AddSingleton<PrintService>();
builder.Services.AddSingleton<ReceiptFormatter>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
