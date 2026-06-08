using Microsoft.AspNetCore.SignalR;

namespace ScaleAtacado.Api.Hubs;

public class PrintHub : Hub
{
    private readonly IConfiguration _configuration;

    public PrintHub(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public override async Task OnConnectedAsync()
    {
        var agentKey = Context.GetHttpContext()?.Request.Query["agentKey"].ToString();
        var expectedKey = _configuration["PrintAgent:AgentKey"];

        if (agentKey == expectedKey)
            await Groups.AddToGroupAsync(Context.ConnectionId, "PrintAgents");
        else
            await Groups.AddToGroupAsync(Context.ConnectionId, "Clients");

        await base.OnConnectedAsync();
    }
}
