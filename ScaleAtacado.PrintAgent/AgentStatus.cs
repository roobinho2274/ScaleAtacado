namespace ScaleAtacado.PrintAgent;

public class AgentStatus
{
    public bool IsHubConnected { get; set; }
    public DateTime? LastJobAt { get; set; }
    public bool LastJobSuccess { get; set; }
}
