namespace ScaleAtacado.PrintAgent;

public class PrintAgentSettings
{
    public const string Section = "PrintAgent";
    public string ApiUrl { get; set; } = string.Empty;
    public string AgentKey { get; set; } = string.Empty;
    public string PrinterName { get; set; } = string.Empty;
    public int FallbackIntervalMinutes { get; set; } = 5;
}
