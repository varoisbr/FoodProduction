using ZebraLabelPrinter.Repositories;

namespace ZebraLabelPrinter.Forms;

/// <summary>
/// Reports form - displays production summaries and profit calculations
///
/// FUTURE API INTEGRATION:
/// - Currently uses production values as proxy for sales
/// - When Flutter app is connected via API, this form should:
///   1. Fetch actual sales data from a Sales table (to be created)
///   2. Compare production (cost basis) vs actual sales (revenue)
///   3. Calculate real profit = Sales Revenue - Production Costs
/// - For now, uses production totals as placeholder values
/// </summary>
public class ReportsForm : Form
{
    private DateTimePicker dtpStartDate = null!;
    private DateTimePicker dtpEndDate = null!;
    private Button btnGenerate = null!;
    private Label lblDateRange = null!;
    private GroupBox grpSummary = null!;
    private Label lblTotalProduction = null!;
    private Label lblTotalCosts = null!;
    private Label lblTotalSales = null!;
    private Label lblProfit = null!;
    private Label lblProfitMargin = null!;
    private DataGridView dgvProductionSummary = null!;
    private Label lblProductionSummary = null!;
    private DataGridView dgvCostSummary = null!;
    private Label lblCostSummary = null!;
    private Label lblNote = null!;

    private readonly ProductionRepository productionRepo = new();
    private readonly CostRepository costRepo = new();

    public ReportsForm()
    {
        InitializeComponents();
        this.Load += ReportsForm_Load;
    }

    private void ReportsForm_Load(object? sender, EventArgs e)
    {
        try
        {
            GenerateReport();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao gerar relatorio:\n\n{ex.Message}",
                "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void InitializeComponents()
    {
        // Form configuration
        this.Text = "Relatorios e Lucros";
        this.Size = new Size(1000, 700);
        this.FormBorderStyle = FormBorderStyle.None;
        this.BackColor = Color.White;
        this.Padding = new Padding(20);

        // Date range filter
        lblDateRange = new Label
        {
            Text = "Periodo do Relatorio:",
            Location = new Point(30, 20),
            Size = new Size(150, 25),
            Font = new Font("Segoe UI", 11F, FontStyle.Bold)
        };

        dtpStartDate = new DateTimePicker
        {
            Location = new Point(190, 18),
            Size = new Size(150, 25),
            Font = new Font("Segoe UI", 10F),
            Format = DateTimePickerFormat.Short,
            Value = DateTime.Now.AddMonths(-1)
        };

        var lblAte = new Label
        {
            Text = "ate",
            Location = new Point(350, 20),
            Size = new Size(30, 25),
            Font = new Font("Segoe UI", 10F)
        };

        dtpEndDate = new DateTimePicker
        {
            Location = new Point(385, 18),
            Size = new Size(150, 25),
            Font = new Font("Segoe UI", 10F),
            Format = DateTimePickerFormat.Short,
            Value = DateTime.Now
        };

        btnGenerate = new Button
        {
            Text = "Gerar Relatorio",
            Location = new Point(550, 15),
            Size = new Size(140, 30),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            BackColor = Color.LightGreen
        };
        btnGenerate.Click += (s, e) => GenerateReport();

        // Summary GroupBox
        grpSummary = new GroupBox
        {
            Text = "Resumo Financeiro",
            Location = new Point(30, 60),
            Size = new Size(960, 180),
            Font = new Font("Segoe UI", 12F, FontStyle.Bold)
        };

        lblTotalProduction = new Label
        {
            Text = "Total Producao: R$ 0,00",
            Location = new Point(30, 40),
            Size = new Size(400, 25),
            Font = new Font("Segoe UI", 11F, FontStyle.Regular)
        };

        lblTotalCosts = new Label
        {
            Text = "Total Custos: R$ 0,00",
            Location = new Point(30, 70),
            Size = new Size(400, 25),
            Font = new Font("Segoe UI", 11F, FontStyle.Regular),
            ForeColor = Color.DarkRed
        };

        lblTotalSales = new Label
        {
            Text = "Total Vendas: R$ 0,00 (usando dados de producao)",
            Location = new Point(30, 100),
            Size = new Size(500, 25),
            Font = new Font("Segoe UI", 11F, FontStyle.Regular),
            ForeColor = Color.DarkBlue
        };

        lblProfit = new Label
        {
            Text = "Lucro: R$ 0,00",
            Location = new Point(30, 130),
            Size = new Size(400, 30),
            Font = new Font("Segoe UI", 14F, FontStyle.Bold),
            ForeColor = Color.DarkGreen
        };

        lblProfitMargin = new Label
        {
            Text = "Margem de Lucro: 0,00%",
            Location = new Point(450, 130),
            Size = new Size(400, 30),
            Font = new Font("Segoe UI", 14F, FontStyle.Bold),
            ForeColor = Color.DarkGreen
        };

        grpSummary.Controls.Add(lblTotalProduction);
        grpSummary.Controls.Add(lblTotalCosts);
        grpSummary.Controls.Add(lblTotalSales);
        grpSummary.Controls.Add(lblProfit);
        grpSummary.Controls.Add(lblProfitMargin);

        // Note label
        lblNote = new Label
        {
            Text = "NOTA: Os valores de vendas atualmente usam dados de producao como placeholder.\n" +
                   "Quando o aplicativo Flutter for integrado, os dados reais de vendas serao sincronizados via API.",
            Location = new Point(30, 250),
            Size = new Size(960, 40),
            Font = new Font("Segoe UI", 9F, FontStyle.Italic),
            ForeColor = Color.Gray,
            BackColor = Color.FromArgb(245, 245, 245),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(10)
        };

        // Production summary label
        lblProductionSummary = new Label
        {
            Text = "Resumo de Producao por Produto:",
            Location = new Point(30, 305),
            Size = new Size(300, 25),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };

        // Production summary DataGridView
        dgvProductionSummary = new DataGridView
        {
            Location = new Point(30, 335),
            Size = new Size(460, 300),
            Font = new Font("Segoe UI", 9F),
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };

        // Cost summary label
        lblCostSummary = new Label
        {
            Text = "Resumo de Custos por Producao:",
            Location = new Point(510, 305),
            Size = new Size(300, 25),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };

        // Cost summary DataGridView
        dgvCostSummary = new DataGridView
        {
            Location = new Point(510, 335),
            Size = new Size(480, 300),
            Font = new Font("Segoe UI", 9F),
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };

        // Add controls to form
        this.Controls.Add(lblDateRange);
        this.Controls.Add(dtpStartDate);
        this.Controls.Add(lblAte);
        this.Controls.Add(dtpEndDate);
        this.Controls.Add(btnGenerate);
        this.Controls.Add(grpSummary);
        this.Controls.Add(lblNote);
        this.Controls.Add(lblProductionSummary);
        this.Controls.Add(dgvProductionSummary);
        this.Controls.Add(lblCostSummary);
        this.Controls.Add(dgvCostSummary);
    }

    private void GenerateReport()
    {
        try
        {
            var startDate = dtpStartDate.Value.Date;
            var endDate = dtpEndDate.Value.Date.AddDays(1).AddSeconds(-1);

            // Get production data
            var totalProductionValue = productionRepo.GetTotalValue(startDate, endDate);
            var totalProductionWeight = productionRepo.GetTotalWeight(startDate, endDate);

            // Get costs data
            var totalCosts = costRepo.GetTotalCosts(startDate, endDate);

            // For now, use production value as sales (placeholder)
            // TODO: Replace with actual sales data from Flutter app via API
            var totalSales = totalProductionValue;

            // Calculate profit
            var profit = totalSales - totalCosts;
            var profitMargin = totalSales > 0 ? (profit / totalSales) * 100 : 0;

            // Update summary labels
            lblTotalProduction.Text = $"Total Producao: R$ {totalProductionValue:F2} ({totalProductionWeight:F3} kg)";
            lblTotalCosts.Text = $"Total Custos: R$ {totalCosts:F2}";
            lblTotalSales.Text = $"Total Vendas: R$ {totalSales:F2} (usando dados de producao)";
            lblProfit.Text = $"Lucro: R$ {profit:F2}";
            lblProfit.ForeColor = profit >= 0 ? Color.DarkGreen : Color.DarkRed;
            lblProfitMargin.Text = $"Margem de Lucro: {profitMargin:F2}%";
            lblProfitMargin.ForeColor = profit >= 0 ? Color.DarkGreen : Color.DarkRed;

            // Load production summary
            LoadProductionSummary(startDate, endDate);

            // Load cost summary
            LoadCostSummary(startDate, endDate);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao gerar relatorio: {ex.Message}",
                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LoadProductionSummary(DateTime startDate, DateTime endDate)
    {
        try
        {
            var summary = productionRepo.GetProductionSummary(startDate, endDate);

            dgvProductionSummary.DataSource = null;
            dgvProductionSummary.Columns.Clear();

            var displayData = summary.Select(s => new
            {
                Produto = s.ProductName,
                Quantidade = s.Count,
                PesoTotal = $"{s.TotalWeight:F3} kg",
                ValorTotal = $"R$ {s.TotalValue:F2}"
            }).ToList();

            dgvProductionSummary.DataSource = displayData;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao carregar resumo de producao: {ex.Message}",
                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LoadCostSummary(DateTime startDate, DateTime endDate)
    {
        try
        {
            var summary = costRepo.GetCostSummary(startDate, endDate);

            dgvCostSummary.DataSource = null;
            dgvCostSummary.Columns.Clear();

            var displayData = summary.Select(s => new
            {
                Producao = s.ProductionName,
                Quantidade = s.Count,
                CustoTotal = $"R$ {s.TotalCost:F2}"
            }).ToList();

            dgvCostSummary.DataSource = displayData;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao carregar resumo de custos: {ex.Message}",
                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
