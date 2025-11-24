using System.Globalization;
using ZebraLabelPrinter.Models;
using ZebraLabelPrinter.Repositories;

namespace ZebraLabelPrinter.Forms;

/// <summary>
/// Production form - refactored from the original MainForm
/// Maintains all existing functionality:
/// - Enter production name, price per kg, weight
/// - Calculate total price automatically
/// - Print labels using ZPL templates via LPT1
/// - Store production entries in SQLite database (replaces CSV)
/// </summary>
public class ProductionForm : Form
{
    private TextBox txtPrecoPorKg = null!;
    private TextBox txtPeso = null!;
    private Label lblTotal = null!;
    private Button btnCalcular = null!;
    private Button btnImprimir = null!;
    private Label lblPrecoPorKgLabel = null!;
    private Label lblPesoLabel = null!;
    private TextBox txtZplTemplate = null!;
    private Label lblZplTemplateLabel = null!;
    private TextBox txtNomeProducao = null!;
    private Label lblNomeProducaoLabel = null!;
    private Button btnNovaProducao = null!;
    private DataGridView dgvProductions = null!;
    private Label lblHistoricoLabel = null!;
    private ComboBox cmbProduto = null!;
    private Label lblProdutoLabel = null!;

    private decimal totalCalculado = 0;
    private decimal precoPorKg = 0;
    private string currentProductionName = string.Empty;

    // Repositories for database access
    private readonly ProductionRepository productionRepo = new();
    private readonly ProductRepository productRepo = new();

    // Template ZPL padrão - usado se o campo customizado estiver vazio
    private const string ZPL_TEMPLATE = @"^XA
^FO50,50^ADN,36,20^FDPreco Total: ##TOTAL##^FS
^FO50,100^ADN,36,20^FDPeso: ##PESO## kg^FS
^XZ";

    public ProductionForm()
    {
        InitializeComponents();
        ConfigureTabOrder();

        // Load data when form is shown, not in constructor
        this.Load += ProductionForm_Load;
    }

    private void ProductionForm_Load(object? sender, EventArgs e)
    {
        try
        {
            LoadProducts();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao carregar produtos:\n\n{ex.Message}",
                "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        try
        {
            LoadProductionHistory();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao carregar historico:\n\n{ex.Message}",
                "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void InitializeComponents()
    {
        // Configurações do Form
        this.Text = "Producao - Impressao de Etiquetas";
        this.Size = new Size(1000, 700);
        this.FormBorderStyle = FormBorderStyle.None;
        this.Font = new Font("Segoe UI", 10F);
        this.BackColor = Color.White;
        this.Padding = new Padding(20);

        // Label Nome da Produção
        lblNomeProducaoLabel = new Label
        {
            Text = "Nome da Producao:",
            Location = new Point(30, 20),
            Size = new Size(180, 25),
            Font = new Font("Segoe UI", 11F, FontStyle.Bold)
        };

        // TextBox Nome da Produção
        txtNomeProducao = new TextBox
        {
            Location = new Point(30, 50),
            Size = new Size(400, 35),
            Font = new Font("Segoe UI", 14F),
            TabIndex = 0,
            PlaceholderText = "Ex: Lote 001, Producao Manha, etc."
        };
        txtNomeProducao.KeyDown += TxtNomeProducao_KeyDown;

        // Label Produto
        lblProdutoLabel = new Label
        {
            Text = "Produto (opcional):",
            Location = new Point(30, 100),
            Size = new Size(180, 25),
            Font = new Font("Segoe UI", 11F, FontStyle.Bold)
        };

        // ComboBox Produto
        cmbProduto = new ComboBox
        {
            Location = new Point(30, 130),
            Size = new Size(400, 35),
            Font = new Font("Segoe UI", 12F),
            DropDownStyle = ComboBoxStyle.DropDownList,
            TabIndex = 1
        };
        cmbProduto.SelectedIndexChanged += CmbProduto_SelectedIndexChanged;

        // Label Preço por KG
        lblPrecoPorKgLabel = new Label
        {
            Text = "Preco por KG (R$):",
            Location = new Point(30, 180),
            Size = new Size(180, 25),
            Font = new Font("Segoe UI", 11F, FontStyle.Bold)
        };

        // TextBox Preço por KG
        txtPrecoPorKg = new TextBox
        {
            Location = new Point(30, 210),
            Size = new Size(400, 35),
            Font = new Font("Segoe UI", 14F),
            TabIndex = 2
        };
        txtPrecoPorKg.KeyPress += NumericTextBox_KeyPress;
        txtPrecoPorKg.KeyDown += TxtPrecoPorKg_KeyDown;

        // Label Peso
        lblPesoLabel = new Label
        {
            Text = "Peso (kg):",
            Location = new Point(30, 260),
            Size = new Size(180, 25),
            Font = new Font("Segoe UI", 11F, FontStyle.Bold)
        };

        // TextBox Peso
        txtPeso = new TextBox
        {
            Location = new Point(30, 290),
            Size = new Size(400, 35),
            Font = new Font("Segoe UI", 14F),
            TabIndex = 3
        };
        txtPeso.KeyPress += NumericTextBox_KeyPress;
        txtPeso.KeyDown += TxtPeso_KeyDown;

        // Label Total
        lblTotal = new Label
        {
            Text = "Total: R$ 0,00",
            Location = new Point(30, 340),
            Size = new Size(400, 40),
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            ForeColor = Color.DarkGreen,
            TextAlign = ContentAlignment.MiddleCenter,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.LightYellow
        };

        // Label ZPL Template
        lblZplTemplateLabel = new Label
        {
            Text = "Template ZPL Customizado (opcional):",
            Location = new Point(30, 395),
            Size = new Size(400, 25),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };

        // TextBox ZPL Template
        txtZplTemplate = new TextBox
        {
            Location = new Point(30, 425),
            Size = new Size(400, 80),
            Font = new Font("Consolas", 9F),
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            TabIndex = 4,
            Text = "Cole aqui o codigo ZPL customizado\r\nUse ##TOTAL## para o valor total e ##PESO## para o peso\r\nDeixe vazio para usar o template padrao",
            ForeColor = Color.Gray
        };
        txtZplTemplate.GotFocus += TxtZplTemplate_GotFocus;
        txtZplTemplate.LostFocus += TxtZplTemplate_LostFocus;

        // Botão Calcular
        btnCalcular = new Button
        {
            Text = "Calcular (Enter)",
            Location = new Point(30, 520),
            Size = new Size(190, 45),
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            TabIndex = 5,
            BackColor = Color.LightBlue
        };
        btnCalcular.Click += BtnCalcular_Click;

        // Botão Imprimir
        btnImprimir = new Button
        {
            Text = "Imprimir (Enter)",
            Location = new Point(240, 520),
            Size = new Size(190, 45),
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            TabIndex = 6,
            BackColor = Color.LightGreen,
            Enabled = false
        };
        btnImprimir.Click += BtnImprimir_Click;
        btnImprimir.KeyDown += BtnImprimir_KeyDown;

        // Botão Nova Produção
        btnNovaProducao = new Button
        {
            Text = "Nova Producao",
            Location = new Point(30, 580),
            Size = new Size(400, 45),
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            TabIndex = 7,
            BackColor = Color.LightSalmon
        };
        btnNovaProducao.Click += BtnNovaProducao_Click;

        // Label Histórico
        lblHistoricoLabel = new Label
        {
            Text = "Historico de Producao:",
            Location = new Point(460, 20),
            Size = new Size(500, 25),
            Font = new Font("Segoe UI", 11F, FontStyle.Bold)
        };

        // DataGridView para histórico
        dgvProductions = new DataGridView
        {
            Location = new Point(460, 50),
            Size = new Size(500, 575),
            Font = new Font("Segoe UI", 9F),
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowHeadersVisible = false
        };

        // Adicionar controles ao Form
        this.Controls.Add(lblNomeProducaoLabel);
        this.Controls.Add(txtNomeProducao);
        this.Controls.Add(lblProdutoLabel);
        this.Controls.Add(cmbProduto);
        this.Controls.Add(lblPrecoPorKgLabel);
        this.Controls.Add(txtPrecoPorKg);
        this.Controls.Add(lblPesoLabel);
        this.Controls.Add(txtPeso);
        this.Controls.Add(lblTotal);
        this.Controls.Add(lblZplTemplateLabel);
        this.Controls.Add(txtZplTemplate);
        this.Controls.Add(btnCalcular);
        this.Controls.Add(btnImprimir);
        this.Controls.Add(btnNovaProducao);
        this.Controls.Add(lblHistoricoLabel);
        this.Controls.Add(dgvProductions);
    }

    private void ConfigureTabOrder()
    {
        txtNomeProducao.TabIndex = 0;
        cmbProduto.TabIndex = 1;
        txtPrecoPorKg.TabIndex = 2;
        txtPeso.TabIndex = 3;
        txtZplTemplate.TabIndex = 4;
        btnCalcular.TabIndex = 5;
        btnImprimir.TabIndex = 6;
        btnNovaProducao.TabIndex = 7;
    }

    /// <summary>
    /// Load products into the ComboBox
    /// </summary>
    private void LoadProducts()
    {
        try
        {
            var products = productRepo.GetAll();

            cmbProduto.Items.Clear();
            cmbProduto.Items.Add(new ComboBoxItem { Text = "-- Sem Produto --", Value = null });

            foreach (var product in products)
            {
                cmbProduto.Items.Add(new ComboBoxItem
                {
                    Text = $"{product.Name} ({product.Type})",
                    Value = product
                });
            }

            cmbProduto.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao carregar produtos: {ex.Message}",
                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Load production history from database
    /// </summary>
    private void LoadProductionHistory()
    {
        var productions = productionRepo.GetAll();

        dgvProductions.DataSource = null;
        dgvProductions.Columns.Clear();

        if (productions.Count == 0)
        {
            // No productions yet - that's OK
            return;
        }

        var displayData = productions.Select(p => new
        {
            Data = p.Date.ToString("dd/MM/yyyy HH:mm"),
            Producao = p.Name ?? "",
            Produto = p.Product?.Name ?? "Sem Produto",
            Peso = $"{p.Weight:F3} kg",
            PrecoKg = $"R$ {p.PricePerKg:F2}",
            Total = $"R$ {p.Total:F2}"
        }).ToList();

        dgvProductions.DataSource = displayData;

        // Adjust column widths
        if (dgvProductions.Columns.Count > 0)
        {
            dgvProductions.Columns[0].Width = 90;  // Data
            dgvProductions.Columns[1].Width = 100; // Producao
            dgvProductions.Columns[2].Width = 100; // Produto
            dgvProductions.Columns[3].Width = 70;  // Peso
            dgvProductions.Columns[4].Width = 70;  // PrecoKg
            dgvProductions.Columns[5].Width = 70;  // Total
        }
    }

    private void CmbProduto_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (cmbProduto.SelectedItem is ComboBoxItem item && item.Value is Product product)
        {
            // Auto-fill price from selected product
            txtPrecoPorKg.Text = product.DefaultPricePerKg.ToString("F2", CultureInfo.GetCultureInfo("pt-BR"));
        }
    }

    private void TxtNomeProducao_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            cmbProduto.Focus();
        }
    }

    private void TxtZplTemplate_GotFocus(object? sender, EventArgs e)
    {
        if (txtZplTemplate.ForeColor == Color.Gray)
        {
            txtZplTemplate.Text = "";
            txtZplTemplate.ForeColor = Color.Black;
        }
    }

    private void TxtZplTemplate_LostFocus(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtZplTemplate.Text))
        {
            txtZplTemplate.Text = "Cole aqui o codigo ZPL customizado\r\nUse ##TOTAL## para o valor total e ##PESO## para o peso\r\nDeixe vazio para usar o template padrao";
            txtZplTemplate.ForeColor = Color.Gray;
        }
    }

    private void NumericTextBox_KeyPress(object? sender, KeyPressEventArgs e)
    {
        // Permitir apenas números, vírgula, ponto e backspace
        if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
            e.KeyChar != ',' && e.KeyChar != '.')
        {
            e.Handled = true;
        }

        // Converter ponto em vírgula
        if (e.KeyChar == '.')
        {
            e.KeyChar = ',';
        }

        // Permitir apenas uma vírgula
        if (e.KeyChar == ',' && (sender as TextBox)!.Text.Contains(","))
        {
            e.Handled = true;
        }
    }

    private void TxtPrecoPorKg_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            txtPeso.Focus();
        }
    }

    private void TxtPeso_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            BtnCalcular_Click(sender, e);
        }
    }

    private void BtnImprimir_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            BtnImprimir_Click(sender, e);
        }
    }

    private void BtnCalcular_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtPrecoPorKg.Text) ||
            string.IsNullOrWhiteSpace(txtPeso.Text))
        {
            MessageBox.Show("Por favor, preencha o preco e o peso.",
                "Dados incompletos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            precoPorKg = decimal.Parse(txtPrecoPorKg.Text,
                CultureInfo.GetCultureInfo("pt-BR"));
            decimal peso = decimal.Parse(txtPeso.Text,
                CultureInfo.GetCultureInfo("pt-BR"));

            totalCalculado = precoPorKg * peso;
            totalCalculado = Math.Round(totalCalculado, 2);

            lblTotal.Text = $"Total: R$ {totalCalculado:N2}";
            btnImprimir.Enabled = true;

            // Mover foco para o botão de imprimir
            btnImprimir.Focus();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao calcular: {ex.Message}",
                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnImprimir_Click(object? sender, EventArgs e)
    {
        if (totalCalculado <= 0)
        {
            MessageBox.Show("Calcule o total antes de imprimir.",
                "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            // Determinar qual template usar
            string templateZpl = ZPL_TEMPLATE;

            // Verificar se há template customizado (não é placeholder)
            if (!string.IsNullOrWhiteSpace(txtZplTemplate.Text) &&
                txtZplTemplate.ForeColor != Color.Gray)
            {
                templateZpl = txtZplTemplate.Text;
            }

            // Verificar se o template contém os marcadores
            if (!templateZpl.Contains("##TOTAL##") && !templateZpl.Contains("##PESO##"))
            {
                var result = MessageBox.Show(
                    "O template ZPL nao contem os marcadores ##TOTAL## ou ##PESO##.\n\n" +
                    "Deseja imprimir mesmo assim?",
                    "Aviso",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.No)
                    return;
            }

            // Obter o peso digitado (com 3 casas decimais)
            decimal peso = decimal.Parse(txtPeso.Text,
                CultureInfo.GetCultureInfo("pt-BR"));

            // Substituir os marcadores pelos valores calculados
            string totalFormatado = totalCalculado.ToString("F2",
                CultureInfo.GetCultureInfo("pt-BR"));
            string pesoFormatado = peso.ToString("F3",
                CultureInfo.GetCultureInfo("pt-BR"));

            string zplFinal = templateZpl
                .Replace("##TOTAL##", totalFormatado)
                .Replace("##PESO##", pesoFormatado);

            // Enviar para LPT1 usando o método existente (type file.zpl > LPT1)
            EnviarParaImpressora(zplFinal);

            MessageBox.Show("Impressao enviada com sucesso!",
                "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Salvar registro no banco de dados SQLite
            SalvarProducao(peso);

            // Limpar campos para próxima etiqueta
            LimparCampos();

            // Recarregar histórico
            LoadProductionHistory();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao enviar para impressora: {ex.Message}\n\n" +
                "Verifique se a impressora esta mapeada corretamente (net use LPT1).",
                "Erro de Impressao", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Save production entry to SQLite database (replaces CSV storage)
    /// </summary>
    private void SalvarProducao(decimal peso)
    {
        try
        {
            var production = new Production
            {
                Name = string.IsNullOrWhiteSpace(txtNomeProducao.Text)
                    ? "Sem Nome"
                    : txtNomeProducao.Text,
                ProductId = (cmbProduto.SelectedItem as ComboBoxItem)?.Value is Product product
                    ? product.Id
                    : null,
                Weight = peso,
                PricePerKg = precoPorKg,
                Total = totalCalculado,
                Date = DateTime.Now,
                ZplTemplatePath = txtZplTemplate.ForeColor != Color.Gray && !string.IsNullOrWhiteSpace(txtZplTemplate.Text)
                    ? "Custom Template"
                    : "Default Template"
            };

            productionRepo.Add(production);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao salvar producao no banco de dados: {ex.Message}",
                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void LimparCampos()
    {
        // Limpar apenas peso e total
        txtPeso.Clear();
        lblTotal.Text = "Total: R$ 0,00";
        totalCalculado = 0;
        btnImprimir.Enabled = false;

        // Voltar foco para o campo de peso (ciclo peso -> imprimir)
        txtPeso.Focus();
    }

    private void BtnNovaProducao_Click(object? sender, EventArgs e)
    {
        // Limpar todos os campos
        txtNomeProducao.Clear();
        cmbProduto.SelectedIndex = 0;
        txtPrecoPorKg.Clear();
        txtPeso.Clear();
        lblTotal.Text = "Total: R$ 0,00";
        totalCalculado = 0;
        precoPorKg = 0;
        btnImprimir.Enabled = false;

        // Focar no campo de nome da produção
        txtNomeProducao.Focus();
    }

    /// <summary>
    /// Send ZPL content to printer using the exact same command: type file.zpl > LPT1
    /// This preserves the existing printing functionality
    /// </summary>
    private void EnviarParaImpressora(string zplContent)
    {
        string tempFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp_label.zpl");

        try
        {
            // Escrever ZPL no arquivo temporário
            File.WriteAllText(tempFile, zplContent);

            // Usar exatamente o comando que funciona: type temp_label.zpl > LPT1
            var processInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c type temp_label.zpl > LPT1",
                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            using (var process = System.Diagnostics.Process.Start(processInfo))
            {
                process!.WaitForExit(5000); // Timeout de 5 segundos

                string erro = process.StandardError.ReadToEnd();

                if (process.ExitCode != 0)
                {
                    throw new Exception($"Erro no comando (Exit Code {process.ExitCode}): {erro}");
                }

                if (!string.IsNullOrEmpty(erro))
                {
                    throw new Exception($"Erro: {erro}");
                }
            }
        }
        finally
        {
            // Aguardar um pouco antes de deletar (garantir que o comando terminou)
            System.Threading.Thread.Sleep(200);

            // Limpar arquivo temporário
            if (File.Exists(tempFile))
            {
                try { File.Delete(tempFile); } catch { }
            }
        }
    }

    /// <summary>
    /// Helper class for ComboBox items
    /// </summary>
    private class ComboBoxItem
    {
        public string Text { get; set; } = string.Empty;
        public object? Value { get; set; }

        public override string ToString() => Text;
    }
}
