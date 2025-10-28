using System.Globalization;
using ZebraLabelPrinter.Models;
using ZebraLabelPrinter.Repositories;

namespace ZebraLabelPrinter.Forms;

/// <summary>
/// Costs management form
/// Allows manual entry of production costs
/// Links costs to production batches and displays cost per kg
/// </summary>
public class CostsForm : Form
{
    private DataGridView dgvCosts = null!;
    private Button btnAdd = null!;
    private Button btnEdit = null!;
    private Button btnDelete = null!;
    private Button btnRefresh = null!;
    private DateTimePicker dtpStartDate = null!;
    private DateTimePicker dtpEndDate = null!;
    private Button btnFilter = null!;
    private Label lblDateRange = null!;
    private GroupBox grpCostDetails = null!;
    private TextBox txtProductionName = null!;
    private Label lblProductionName = null!;
    private TextBox txtTotalCost = null!;
    private Label lblTotalCost = null!;
    private TextBox txtNotes = null!;
    private Label lblNotes = null!;
    private DateTimePicker dtpCostDate = null!;
    private Label lblCostDate = null!;
    private Button btnSave = null!;
    private Button btnCancel = null!;
    private Label lblTotalCosts = null!;

    private readonly CostRepository costRepo = new();
    private readonly ProductionRepository productionRepo = new();
    private Cost? currentCost = null;
    private bool isEditMode = false;

    public CostsForm()
    {
        InitializeComponents();
        this.Load += CostsForm_Load;
    }

    private void CostsForm_Load(object? sender, EventArgs e)
    {
        try
        {
            LoadCosts();
            UpdateTotalCosts();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao carregar dados:\n\n{ex.Message}",
                "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void InitializeComponents()
    {
        // Form configuration
        this.Text = "Gerenciamento de Custos";
        this.Size = new Size(1000, 700);
        this.FormBorderStyle = FormBorderStyle.None;
        this.BackColor = Color.White;
        this.Padding = new Padding(20);

        // Date range filter
        lblDateRange = new Label
        {
            Text = "Periodo:",
            Location = new Point(30, 20),
            Size = new Size(60, 25),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };

        dtpStartDate = new DateTimePicker
        {
            Location = new Point(100, 18),
            Size = new Size(150, 25),
            Font = new Font("Segoe UI", 9F),
            Format = DateTimePickerFormat.Short,
            Value = DateTime.Now.AddMonths(-1)
        };

        var lblAte = new Label
        {
            Text = "ate",
            Location = new Point(260, 20),
            Size = new Size(30, 25),
            Font = new Font("Segoe UI", 9F)
        };

        dtpEndDate = new DateTimePicker
        {
            Location = new Point(295, 18),
            Size = new Size(150, 25),
            Font = new Font("Segoe UI", 9F),
            Format = DateTimePickerFormat.Short,
            Value = DateTime.Now
        };

        btnFilter = new Button
        {
            Text = "Filtrar",
            Location = new Point(455, 16),
            Size = new Size(80, 28),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            BackColor = Color.LightBlue
        };
        btnFilter.Click += (s, e) => LoadCosts();

        // Action buttons
        btnAdd = new Button
        {
            Text = "Novo Custo",
            Location = new Point(700, 15),
            Size = new Size(110, 30),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            BackColor = Color.LightGreen
        };
        btnAdd.Click += BtnAdd_Click;

        btnEdit = new Button
        {
            Text = "Editar",
            Location = new Point(820, 15),
            Size = new Size(80, 30),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            BackColor = Color.LightBlue
        };
        btnEdit.Click += BtnEdit_Click;

        btnDelete = new Button
        {
            Text = "Excluir",
            Location = new Point(910, 15),
            Size = new Size(80, 30),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            BackColor = Color.LightCoral
        };
        btnDelete.Click += BtnDelete_Click;

        // Total costs label
        lblTotalCosts = new Label
        {
            Text = "Total de Custos: R$ 0,00",
            Location = new Point(30, 55),
            Size = new Size(400, 25),
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = Color.DarkRed
        };

        // DataGridView
        dgvCosts = new DataGridView
        {
            Location = new Point(30, 90),
            Size = new Size(960, 250),
            Font = new Font("Segoe UI", 9F),
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };

        // Cost details GroupBox
        grpCostDetails = new GroupBox
        {
            Text = "Detalhes do Custo",
            Location = new Point(30, 360),
            Size = new Size(960, 290),
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            Visible = false
        };

        // Production name label and textbox
        lblProductionName = new Label
        {
            Text = "Nome da Producao:",
            Location = new Point(20, 40),
            Size = new Size(150, 25),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };

        txtProductionName = new TextBox
        {
            Location = new Point(180, 38),
            Size = new Size(400, 25),
            Font = new Font("Segoe UI", 10F),
            PlaceholderText = "Ex: Lote 001, Producao Manha"
        };

        // Cost date label and DateTimePicker
        lblCostDate = new Label
        {
            Text = "Data do Custo:",
            Location = new Point(600, 40),
            Size = new Size(120, 25),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };

        dtpCostDate = new DateTimePicker
        {
            Location = new Point(730, 38),
            Size = new Size(200, 25),
            Font = new Font("Segoe UI", 10F),
            Format = DateTimePickerFormat.Short
        };

        // Total cost label and textbox
        lblTotalCost = new Label
        {
            Text = "Custo Total (R$):",
            Location = new Point(20, 85),
            Size = new Size(150, 25),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };

        txtTotalCost = new TextBox
        {
            Location = new Point(180, 83),
            Size = new Size(200, 25),
            Font = new Font("Segoe UI", 10F)
        };
        txtTotalCost.KeyPress += NumericTextBox_KeyPress;

        // Notes label and textbox
        lblNotes = new Label
        {
            Text = "Observacoes:",
            Location = new Point(20, 130),
            Size = new Size(150, 25),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };

        txtNotes = new TextBox
        {
            Location = new Point(180, 128),
            Size = new Size(750, 80),
            Font = new Font("Segoe UI", 10F),
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            PlaceholderText = "Informacoes adicionais sobre este custo..."
        };

        // Save button
        btnSave = new Button
        {
            Text = "Salvar",
            Location = new Point(180, 225),
            Size = new Size(120, 40),
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            BackColor = Color.LightGreen
        };
        btnSave.Click += BtnSave_Click;

        // Cancel button
        btnCancel = new Button
        {
            Text = "Cancelar",
            Location = new Point(310, 225),
            Size = new Size(120, 40),
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            BackColor = Color.LightGray
        };
        btnCancel.Click += BtnCancel_Click;

        // Add controls to GroupBox
        grpCostDetails.Controls.Add(lblProductionName);
        grpCostDetails.Controls.Add(txtProductionName);
        grpCostDetails.Controls.Add(lblCostDate);
        grpCostDetails.Controls.Add(dtpCostDate);
        grpCostDetails.Controls.Add(lblTotalCost);
        grpCostDetails.Controls.Add(txtTotalCost);
        grpCostDetails.Controls.Add(lblNotes);
        grpCostDetails.Controls.Add(txtNotes);
        grpCostDetails.Controls.Add(btnSave);
        grpCostDetails.Controls.Add(btnCancel);

        // Add controls to form
        this.Controls.Add(lblDateRange);
        this.Controls.Add(dtpStartDate);
        this.Controls.Add(lblAte);
        this.Controls.Add(dtpEndDate);
        this.Controls.Add(btnFilter);
        this.Controls.Add(btnAdd);
        this.Controls.Add(btnEdit);
        this.Controls.Add(btnDelete);
        this.Controls.Add(lblTotalCosts);
        this.Controls.Add(dgvCosts);
        this.Controls.Add(grpCostDetails);
    }

    private void LoadCosts()
    {
        try
        {
            var costs = costRepo.GetByDateRange(dtpStartDate.Value.Date, dtpEndDate.Value.Date.AddDays(1).AddSeconds(-1));

            dgvCosts.DataSource = null;
            dgvCosts.Columns.Clear();

            var displayData = costs.Select(c => new
            {
                c.Id,
                Data = c.Date.ToString("dd/MM/yyyy"),
                Producao = c.ProductionName,
                CustoTotal = $"R$ {c.TotalCost:F2}",
                Observacoes = string.IsNullOrEmpty(c.Notes) ? "" : c.Notes.Length > 50 ? c.Notes.Substring(0, 47) + "..." : c.Notes
            }).ToList();

            dgvCosts.DataSource = displayData;

            // Hide ID column
            if (dgvCosts.Columns["Id"] != null)
                dgvCosts.Columns["Id"]!.Visible = false;

            UpdateTotalCosts();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao carregar custos: {ex.Message}",
                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void UpdateTotalCosts()
    {
        try
        {
            var total = costRepo.GetTotalCosts(dtpStartDate.Value.Date, dtpEndDate.Value.Date.AddDays(1).AddSeconds(-1));
            lblTotalCosts.Text = $"Total de Custos: R$ {total:F2}";
        }
        catch { }
    }

    private void BtnAdd_Click(object? sender, EventArgs e)
    {
        isEditMode = false;
        currentCost = null;

        txtProductionName.Clear();
        txtTotalCost.Clear();
        txtNotes.Clear();
        dtpCostDate.Value = DateTime.Now;

        grpCostDetails.Visible = true;
        txtProductionName.Focus();
    }

    private void BtnEdit_Click(object? sender, EventArgs e)
    {
        if (dgvCosts.SelectedRows.Count == 0)
        {
            MessageBox.Show("Selecione um custo para editar.",
                "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            var selectedRow = dgvCosts.SelectedRows[0];
            int costId = Convert.ToInt32(selectedRow.Cells["Id"].Value);

            currentCost = costRepo.GetById(costId);
            if (currentCost == null)
            {
                MessageBox.Show("Custo nao encontrado.",
                    "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            isEditMode = true;

            txtProductionName.Text = currentCost.ProductionName;
            txtTotalCost.Text = currentCost.TotalCost.ToString("F2", CultureInfo.GetCultureInfo("pt-BR"));
            txtNotes.Text = currentCost.Notes ?? "";
            dtpCostDate.Value = currentCost.Date;

            grpCostDetails.Visible = true;
            txtProductionName.Focus();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao carregar custo: {ex.Message}",
                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (dgvCosts.SelectedRows.Count == 0)
        {
            MessageBox.Show("Selecione um custo para excluir.",
                "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            var selectedRow = dgvCosts.SelectedRows[0];
            string productionName = selectedRow.Cells["Producao"].Value?.ToString() ?? "";
            int costId = Convert.ToInt32(selectedRow.Cells["Id"].Value);

            var result = MessageBox.Show(
                $"Deseja realmente excluir o custo da producao '{productionName}'?\n\n" +
                "Esta acao nao pode ser desfeita.",
                "Confirmar Exclusao",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                if (costRepo.Delete(costId))
                {
                    MessageBox.Show("Custo excluido com sucesso!",
                        "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadCosts();
                }
                else
                {
                    MessageBox.Show("Erro ao excluir custo.",
                        "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao excluir custo: {ex.Message}",
                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtProductionName.Text))
        {
            MessageBox.Show("Por favor, informe o nome da producao.",
                "Validacao", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtProductionName.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(txtTotalCost.Text))
        {
            MessageBox.Show("Por favor, informe o custo total.",
                "Validacao", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtTotalCost.Focus();
            return;
        }

        try
        {
            decimal totalCost = decimal.Parse(txtTotalCost.Text, CultureInfo.GetCultureInfo("pt-BR"));

            if (totalCost < 0)
            {
                MessageBox.Show("O custo deve ser maior ou igual a zero.",
                    "Validacao", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (isEditMode && currentCost != null)
            {
                // Update existing cost
                currentCost.ProductionName = txtProductionName.Text;
                currentCost.TotalCost = totalCost;
                currentCost.Notes = txtNotes.Text;
                currentCost.Date = dtpCostDate.Value;

                costRepo.Update(currentCost);

                MessageBox.Show("Custo atualizado com sucesso!",
                    "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                // Add new cost
                var newCost = new Cost
                {
                    ProductionName = txtProductionName.Text,
                    TotalCost = totalCost,
                    Notes = txtNotes.Text,
                    Date = dtpCostDate.Value
                };

                costRepo.Add(newCost);

                MessageBox.Show("Custo adicionado com sucesso!",
                    "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            grpCostDetails.Visible = false;
            LoadCosts();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao salvar custo: {ex.Message}",
                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnCancel_Click(object? sender, EventArgs e)
    {
        grpCostDetails.Visible = false;
        currentCost = null;
    }

    private void NumericTextBox_KeyPress(object? sender, KeyPressEventArgs e)
    {
        if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
            e.KeyChar != ',' && e.KeyChar != '.')
        {
            e.Handled = true;
        }

        if (e.KeyChar == '.')
        {
            e.KeyChar = ',';
        }

        if (e.KeyChar == ',' && (sender as TextBox)!.Text.Contains(","))
        {
            e.Handled = true;
        }
    }
}
