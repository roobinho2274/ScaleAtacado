using ScaleAtacado.PrintAgent.Services;
using System.Drawing;
using System.Drawing.Printing;
using System.Text.Json;
using System.Windows.Forms;

namespace ScaleAtacado.PrintAgent;

public class ConfigForm : Form
{
    private readonly IConfiguration _configuration;
    private readonly PrintService _printService;
    private readonly AgentStatus _agentStatus;
    private readonly string _settingsFilePath;

    private ComboBox _cmbPrinter = null!;
    private TextBox _txtApiUrl = null!;
    private TextBox _txtAgentKey = null!;
    private NumericUpDown _nudInterval = null!;
    private Panel _statusDot = null!;
    private Label _lblStatus = null!;
    private System.Windows.Forms.Timer _statusTimer = null!;

    private const int LabelW = 148;
    private const int InputX = 16 + LabelW + 6;
    private const int InputW = 480 - InputX - 16;
    private const int RowH = 38;

    public ConfigForm(IConfiguration configuration, PrintService printService, AgentStatus agentStatus)
    {
        _configuration = configuration;
        _printService = printService;
        _agentStatus = agentStatus;
        _settingsFilePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");

        BuildUI();
        LoadCurrentValues();

        _statusTimer = new System.Windows.Forms.Timer { Interval = 2000 };
        _statusTimer.Tick += (_, _) => UpdateStatus();
        _statusTimer.Start();
        UpdateStatus();
    }

    private void BuildUI()
    {
        Text = "Configuração — ScaleAtacado PrintAgent";
        ClientSize = new Size(480, 316);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9f);
        BackColor = Color.White;

        int y = 16;

        // ── Título ──────────────────────────────────────────
        Add(new Label
        {
            Text = "Configuração do Agente de Impressão",
            Font = new Font("Segoe UI", 11f, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 37, 41),
            Location = new Point(16, y),
            AutoSize = true
        });
        y += 38;

        // ── Impressora ──────────────────────────────────────
        AddLabel("Impressora:", y);
        _cmbPrinter = new ComboBox
        {
            Location = new Point(InputX, y - 1),
            Width = InputW,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        foreach (string printer in PrinterSettings.InstalledPrinters)
            _cmbPrinter.Items.Add(printer);
        if (_cmbPrinter.Items.Count == 0)
            _cmbPrinter.Items.Add("(nenhuma impressora instalada)");
        Add(_cmbPrinter);
        y += RowH;

        // ── URL da API ──────────────────────────────────────
        AddLabel("URL da API:", y);
        _txtApiUrl = new TextBox
        {
            Location = new Point(InputX, y - 1),
            Width = InputW
        };
        Add(_txtApiUrl);
        y += RowH;

        // ── Chave do Agente ─────────────────────────────────
        AddLabel("Chave do Agente:", y);
        _txtAgentKey = new TextBox
        {
            Location = new Point(InputX, y - 1),
            Width = InputW - 78,
            PasswordChar = '●'
        };
        Add(_txtAgentKey);

        var chkShow = new CheckBox
        {
            Text = "Mostrar",
            Location = new Point(InputX + InputW - 72, y),
            Width = 72,
            AutoSize = false,
            Height = 20
        };
        chkShow.CheckedChanged += (_, _) =>
            _txtAgentKey.PasswordChar = chkShow.Checked ? '\0' : '●';
        Add(chkShow);
        y += RowH;

        // ── Intervalo de verificação ─────────────────────────
        AddLabel("Verificar a cada:", y);
        _nudInterval = new NumericUpDown
        {
            Location = new Point(InputX, y - 1),
            Width = 64,
            Minimum = 1,
            Maximum = 60,
            Value = 5
        };
        Add(_nudInterval);
        Add(new Label
        {
            Text = "minutos (fallback sem conexão)",
            Location = new Point(InputX + 70, y + 2),
            AutoSize = true,
            ForeColor = Color.Gray
        });
        y += RowH;

        // ── Separador ────────────────────────────────────────
        Add(new Label
        {
            Location = new Point(16, y),
            Size = new Size(448, 2),
            BorderStyle = BorderStyle.Fixed3D
        });
        y += 12;

        // ── Status de conexão ────────────────────────────────
        _statusDot = new Panel
        {
            Location = new Point(16, y + 3),
            Size = new Size(13, 13),
            BackColor = Color.Gray
        };
        Add(_statusDot);

        _lblStatus = new Label
        {
            Text = "Verificando conexão com o servidor...",
            Location = new Point(36, y),
            AutoSize = true,
            ForeColor = Color.FromArgb(80, 80, 80)
        };
        Add(_lblStatus);
        y += 28;

        // ── Nota sobre reinício ──────────────────────────────
        Add(new Label
        {
            Text = "Mudanças na URL ou Chave requerem reiniciar o agente.",
            Location = new Point(16, y),
            AutoSize = true,
            ForeColor = Color.FromArgb(140, 100, 0),
            Font = new Font("Segoe UI", 8f)
        });

        // ── Botões ───────────────────────────────────────────
        int btnY = ClientSize.Height - 48;

        var btnTest = new Button
        {
            Text = "Testar Impressão",
            Location = new Point(16, btnY),
            Size = new Size(140, 32),
            FlatStyle = FlatStyle.System
        };
        btnTest.Click += BtnTest_Click;
        Add(btnTest);

        var btnSave = new Button
        {
            Text = "Salvar",
            Location = new Point(ClientSize.Width - 16 - 164, btnY),
            Size = new Size(76, 32),
            FlatStyle = FlatStyle.System
        };
        btnSave.Click += BtnSave_Click;
        Add(btnSave);

        var btnClose = new Button
        {
            Text = "Fechar",
            Location = new Point(ClientSize.Width - 16 - 80, btnY),
            Size = new Size(76, 32),
            FlatStyle = FlatStyle.System
        };
        btnClose.Click += (_, _) => Close();
        Add(btnClose);
    }

    private void AddLabel(string text, int y) => Add(new Label
    {
        Text = text,
        Location = new Point(16, y + 3),
        Width = LabelW,
        TextAlign = ContentAlignment.MiddleRight
    });

    private void Add(Control c) => Controls.Add(c);

    private void LoadCurrentValues()
    {
        var printerName = _configuration["PrintAgent:PrinterName"] ?? string.Empty;
        if (!string.IsNullOrEmpty(printerName) && _cmbPrinter.Items.Contains(printerName))
            _cmbPrinter.SelectedItem = printerName;
        else if (_cmbPrinter.Items.Count > 0)
            _cmbPrinter.SelectedIndex = 0;

        _txtApiUrl.Text = _configuration["PrintAgent:ApiUrl"] ?? string.Empty;
        _txtAgentKey.Text = _configuration["PrintAgent:AgentKey"] ?? string.Empty;

        var intervalStr = _configuration["PrintAgent:FallbackIntervalMinutes"];
        _nudInterval.Value = int.TryParse(intervalStr, out var v)
            ? Math.Clamp(v, 1, 60)
            : 5;
    }

    private void UpdateStatus()
    {
        if (_agentStatus.IsHubConnected)
        {
            _statusDot.BackColor = Color.SeaGreen;
            if (_agentStatus.LastJobAt.HasValue)
            {
                var suffix = _agentStatus.LastJobSuccess ? "OK" : "Erro";
                _lblStatus.Text = $"Conectado — último job: {_agentStatus.LastJobAt:dd/MM HH:mm} ({suffix})";
            }
            else
            {
                _lblStatus.Text = "Conectado ao servidor. Aguardando jobs...";
            }
        }
        else
        {
            _statusDot.BackColor = Color.OrangeRed;
            _lblStatus.Text = "Sem conexão com o servidor (modo fallback ativo)";
        }
    }

    private void BtnTest_Click(object? sender, EventArgs e)
    {
        var printer = _cmbPrinter.SelectedItem?.ToString() ?? string.Empty;
        if (printer.StartsWith("("))
        {
            MessageBox.Show("Nenhuma impressora disponível para teste.", "Atenção",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var lines = string.Join("\n", new[]
        {
            "   SCALE ATACADO",
            "   Teste de Impressora",
            "================",
            $"Impressora:",
            $"  {printer}",
            $"Data: {DateTime.Now:dd/MM/yyyy HH:mm:ss}",
            "----------------",
            "   Impressao OK!",
            ""
        });

        var success = _printService.Print(lines, printer);

        MessageBox.Show(
            success
                ? $"Impressão de teste enviada com sucesso!\n\nImpressora: {printer}"
                : "Falha ao enviar para a impressora.\n\nVerifique se a impressora está ligada e conectada.",
            "Teste de Impressão",
            MessageBoxButtons.OK,
            success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        var printerName = _cmbPrinter.SelectedItem?.ToString() ?? string.Empty;
        if (printerName.StartsWith("("))
        {
            MessageBox.Show("Selecione uma impressora válida.", "Atenção",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var apiUrl = _txtApiUrl.Text.Trim().TrimEnd('/');
        if (string.IsNullOrEmpty(apiUrl))
        {
            MessageBox.Show("Informe a URL da API.", "Atenção",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var settings = new Dictionary<string, object>
        {
            ["Logging"] = new Dictionary<string, object>
            {
                ["LogLevel"] = new Dictionary<string, string>
                {
                    ["Default"] = "Information",
                    ["Microsoft.Hosting.Lifetime"] = "Information"
                }
            },
            ["PrintAgent"] = new Dictionary<string, object>
            {
                ["ApiUrl"] = apiUrl,
                ["AgentKey"] = _txtAgentKey.Text.Trim(),
                ["PrinterName"] = printerName,
                ["FallbackIntervalMinutes"] = (int)_nudInterval.Value
            }
        };

        try
        {
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsFilePath, json);
            ((IConfigurationRoot)_configuration).Reload();

            MessageBox.Show(
                $"Configuração salva com sucesso!\n\nImpressora ativa: {printerName}\n\n" +
                "Mudanças na impressora já estão em vigor.\n" +
                "Para aplicar nova URL ou Chave, feche e reabra o agente.",
                "Configuração Salva",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Erro ao salvar configuração:\n{ex.Message}",
                "Erro",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _statusTimer.Stop();
        _statusTimer.Dispose();
        base.OnFormClosed(e);
    }
}
