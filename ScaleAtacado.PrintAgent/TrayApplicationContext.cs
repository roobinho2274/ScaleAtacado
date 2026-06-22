using ScaleAtacado.PrintAgent.Services;
using WinForms = System.Windows.Forms;

namespace ScaleAtacado.PrintAgent;

public class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _tray;
    private readonly IServiceProvider _services;

    public TrayApplicationContext(IServiceProvider services)
    {
        _services = services;

        _tray = new NotifyIcon
        {
            Icon = SystemIcons.Information,
            Text = "ScaleAtacado — Agente de Impressão",
            Visible = true,
            ContextMenuStrip = BuildMenu()
        };

        _tray.DoubleClick += (_, _) => OpenConfig();
    }

    private ContextMenuStrip BuildMenu()
    {
        var menu = new ContextMenuStrip();

        var header = new ToolStripLabel("ScaleAtacado PrintAgent")
        {
            Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold)
        };
        menu.Items.Add(header);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Configurar Impressora...", null, (_, _) => OpenConfig());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Fechar Agente", null, (_, _) => Exit());

        return menu;
    }

    private void OpenConfig()
    {
        var config = _services.GetRequiredService<IConfiguration>();
        var printService = _services.GetRequiredService<PrintService>();
        var agentStatus = _services.GetRequiredService<AgentStatus>();

        using var form = new ConfigForm(config, printService, agentStatus);
        form.ShowDialog();
    }

    private void Exit()
    {
        _tray.Visible = false;
        WinForms.Application.Exit();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _tray.Visible = false;
            _tray.Dispose();
        }
        base.Dispose(disposing);
    }
}
