using System.Globalization;
using ZebraLabelPrinter.Models;
using ZebraLabelPrinter.Repositories;

namespace ZebraLabelPrinter.Forms;

/// <summary>
/// Formulations management form
/// Allows creating recipes with multiple ingredients
/// Each formulation can have multiple ingredients with quantities and percentages
/// </summary>
public class FormulationsForm : Form
{
    private DataGridView dgvFormulations = null!;
    private Button btnAdd = null!;
    private Button btnEdit = null!;
    private Button btnDelete = null!;
    private Button btnRefresh = null!;
    private GroupBox grpFormulationDetails = null!;
    private TextBox txtName = null!;
    private Label lblName = null!;
    private TextBox txtYield = null!;
    private Label lblYield = null!;
    private DataGridView dgvIngredients = null!;
    private Label lblIngredients = null!;
    private ComboBox cmbIngredientProduct = null!;
    private TextBox txtIngredientQuantity = null!;
    private TextBox txtIngredientPercentage = null!;
    private Button btnAddIngredient = null!;
    private Button btnRemoveIngredient = null!;
    private Button btnSave = null!;
    private Button btnCancel = null!;

    private readonly FormulationRepository formulationRepo = new();
    private readonly ProductRepository productRepo = new();
    private Formulation? currentFormulation = null;
    private bool isEditMode = false;
    private readonly List<FormulationIngredient> currentIngredients = new();

    public FormulationsForm()
    {
        InitializeComponents();
        this.Load += FormulationsForm_Load;
    }

    private void FormulationsForm_Load(object? sender, EventArgs e)
    {
        try
        {
            LoadFormulations();
            LoadProductsForIngredients();
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
        this.Text = "Gerenciamento de Formulacoes";
        this.Size = new Size(1000, 700);
        this.FormBorderStyle = FormBorderStyle.None;
        this.BackColor = Color.White;
        this.Padding = new Padding(20);

        // Title and buttons
        var lblTitle = new Label
        {
            Text = "Formulacoes / Receitas",
            Location = new Point(30, 20),
            Size = new Size(300, 30),
            Font = new Font("Segoe UI", 14F, FontStyle.Bold)
        };

        btnAdd = new Button
        {
            Text = "Nova Formulacao",
            Location = new Point(700, 15),
            Size = new Size(140, 30),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            BackColor = Color.LightGreen
        };
        btnAdd.Click += BtnAdd_Click;

        btnEdit = new Button
        {
            Text = "Editar",
            Location = new Point(850, 15),
            Size = new Size(80, 30),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            BackColor = Color.LightBlue
        };
        btnEdit.Click += BtnEdit_Click;

        btnDelete = new Button
        {
            Text = "Excluir",
            Location = new Point(940, 15),
            Size = new Size(80, 30),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            BackColor = Color.LightCoral
        };
        btnDelete.Click += BtnDelete_Click;

        // DataGridView for formulations
        dgvFormulations = new DataGridView
        {
            Location = new Point(30, 60),
            Size = new Size(990, 250),
            Font = new Font("Segoe UI", 9F),
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        dgvFormulations.CellDoubleClick += (s, e) =>
        {
            if (e.RowIndex >= 0) BtnEdit_Click(s, e);
        };

        // Formulation details GroupBox
        grpFormulationDetails = new GroupBox
        {
            Text = "Detalhes da Formulacao",
            Location = new Point(30, 320),
            Size = new Size(990, 350),
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            Visible = false
        };

        // Name label and textbox
        lblName = new Label
        {
            Text = "Nome da Formulacao:",
            Location = new Point(20, 35),
            Size = new Size(150, 25),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };

        txtName = new TextBox
        {
            Location = new Point(180, 33),
            Size = new Size(400, 25),
            Font = new Font("Segoe UI", 10F)
        };

        // Yield label and textbox
        lblYield = new Label
        {
            Text = "Rendimento (%):",
            Location = new Point(600, 35),
            Size = new Size(120, 25),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };

        txtYield = new TextBox
        {
            Location = new Point(730, 33),
            Size = new Size(100, 25),
            Font = new Font("Segoe UI", 10F),
            Text = "100,00"
        };
        txtYield.KeyPress += NumericTextBox_KeyPress;

        // Ingredients label
        lblIngredients = new Label
        {
            Text = "Ingredientes:",
            Location = new Point(20, 75),
            Size = new Size(150, 25),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };

        // Ingredients DataGridView
        dgvIngredients = new DataGridView
        {
            Location = new Point(20, 105),
            Size = new Size(950, 150),
            Font = new Font("Segoe UI", 9F),
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };

        // Ingredient input controls
        var lblAddIngredient = new Label
        {
            Text = "Adicionar Ingrediente:",
            Location = new Point(20, 265),
            Size = new Size(150, 20),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };

        cmbIngredientProduct = new ComboBox
        {
            Location = new Point(20, 290),
            Size = new Size(300, 25),
            Font = new Font("Segoe UI", 9F),
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        txtIngredientQuantity = new TextBox
        {
            Location = new Point(330, 290),
            Size = new Size(100, 25),
            Font = new Font("Segoe UI", 9F),
            PlaceholderText = "Qtd (kg)"
        };
        txtIngredientQuantity.KeyPress += NumericTextBox_KeyPress;

        txtIngredientPercentage = new TextBox
        {
            Location = new Point(440, 290),
            Size = new Size(100, 25),
            Font = new Font("Segoe UI", 9F),
            PlaceholderText = "% (%)"
        };
        txtIngredientPercentage.KeyPress += NumericTextBox_KeyPress;

        btnAddIngredient = new Button
        {
            Text = "Adicionar",
            Location = new Point(550, 288),
            Size = new Size(90, 28),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            BackColor = Color.LightGreen
        };
        btnAddIngredient.Click += BtnAddIngredient_Click;

        btnRemoveIngredient = new Button
        {
            Text = "Remover Selecionado",
            Location = new Point(650, 288),
            Size = new Size(140, 28),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            BackColor = Color.LightCoral
        };
        btnRemoveIngredient.Click += BtnRemoveIngredient_Click;

        // Save and Cancel buttons
        btnSave = new Button
        {
            Text = "Salvar Formulacao",
            Location = new Point(800, 285),
            Size = new Size(140, 35),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            BackColor = Color.LightGreen
        };
        btnSave.Click += BtnSave_Click;

        btnCancel = new Button
        {
            Text = "Cancelar",
            Location = new Point(800, 250),
            Size = new Size(140, 30),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            BackColor = Color.LightGray
        };
        btnCancel.Click += BtnCancel_Click;

        // Add controls to GroupBox
        grpFormulationDetails.Controls.Add(lblName);
        grpFormulationDetails.Controls.Add(txtName);
        grpFormulationDetails.Controls.Add(lblYield);
        grpFormulationDetails.Controls.Add(txtYield);
        grpFormulationDetails.Controls.Add(lblIngredients);
        grpFormulationDetails.Controls.Add(dgvIngredients);
        grpFormulationDetails.Controls.Add(lblAddIngredient);
        grpFormulationDetails.Controls.Add(cmbIngredientProduct);
        grpFormulationDetails.Controls.Add(txtIngredientQuantity);
        grpFormulationDetails.Controls.Add(txtIngredientPercentage);
        grpFormulationDetails.Controls.Add(btnAddIngredient);
        grpFormulationDetails.Controls.Add(btnRemoveIngredient);
        grpFormulationDetails.Controls.Add(btnSave);
        grpFormulationDetails.Controls.Add(btnCancel);

        // Add controls to form
        this.Controls.Add(lblTitle);
        this.Controls.Add(btnAdd);
        this.Controls.Add(btnEdit);
        this.Controls.Add(btnDelete);
        this.Controls.Add(dgvFormulations);
        this.Controls.Add(grpFormulationDetails);
    }

    private void LoadFormulations()
    {
        try
        {
            var formulations = formulationRepo.GetAll();

            dgvFormulations.DataSource = null;
            dgvFormulations.Columns.Clear();

            var displayData = formulations.Select(f => new
            {
                f.Id,
                Nome = f.Name,
                Rendimento = $"{f.YieldPercentage:F2}%",
                Ingredientes = f.Ingredients.Count,
                DataCriacao = f.CreatedDate.ToString("dd/MM/yyyy")
            }).ToList();

            dgvFormulations.DataSource = displayData;

            // Hide ID column
            if (dgvFormulations.Columns["Id"] != null)
                dgvFormulations.Columns["Id"]!.Visible = false;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao carregar formulacoes: {ex.Message}",
                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LoadProductsForIngredients()
    {
        try
        {
            var products = productRepo.GetAll();

            cmbIngredientProduct.Items.Clear();

            foreach (var product in products)
            {
                cmbIngredientProduct.Items.Add(new ComboBoxItem
                {
                    Text = $"{product.Name} ({product.Type})",
                    Value = product
                });
            }

            if (cmbIngredientProduct.Items.Count > 0)
                cmbIngredientProduct.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao carregar produtos: {ex.Message}",
                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void RefreshIngredientsGrid()
    {
        dgvIngredients.DataSource = null;
        dgvIngredients.Columns.Clear();

        var displayData = currentIngredients.Select(i => new
        {
            Produto = i.Product?.Name ?? "Produto Desconhecido",
            Quantidade = $"{i.Quantity:F3} kg",
            Percentual = $"{i.Percentage:F2}%"
        }).ToList();

        dgvIngredients.DataSource = displayData;
    }

    private void BtnAdd_Click(object? sender, EventArgs e)
    {
        isEditMode = false;
        currentFormulation = null;
        currentIngredients.Clear();

        txtName.Clear();
        txtYield.Text = "100,00";

        RefreshIngredientsGrid();
        grpFormulationDetails.Visible = true;
        txtName.Focus();
    }

    private void BtnEdit_Click(object? sender, EventArgs e)
    {
        if (dgvFormulations.SelectedRows.Count == 0)
        {
            MessageBox.Show("Selecione uma formulacao para editar.",
                "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            var selectedRow = dgvFormulations.SelectedRows[0];
            int formulationId = Convert.ToInt32(selectedRow.Cells["Id"].Value);

            currentFormulation = formulationRepo.GetById(formulationId);
            if (currentFormulation == null)
            {
                MessageBox.Show("Formulacao nao encontrada.",
                    "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            isEditMode = true;

            txtName.Text = currentFormulation.Name;
            txtYield.Text = currentFormulation.YieldPercentage.ToString("F2", CultureInfo.GetCultureInfo("pt-BR"));

            currentIngredients.Clear();
            currentIngredients.AddRange(currentFormulation.Ingredients);

            RefreshIngredientsGrid();
            grpFormulationDetails.Visible = true;
            txtName.Focus();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao carregar formulacao: {ex.Message}",
                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (dgvFormulations.SelectedRows.Count == 0)
        {
            MessageBox.Show("Selecione uma formulacao para excluir.",
                "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            var selectedRow = dgvFormulations.SelectedRows[0];
            string formulationName = selectedRow.Cells["Nome"].Value?.ToString() ?? "";
            int formulationId = Convert.ToInt32(selectedRow.Cells["Id"].Value);

            var result = MessageBox.Show(
                $"Deseja realmente excluir a formulacao '{formulationName}'?\n\n" +
                "Todos os ingredientes serao removidos.\n" +
                "Esta acao nao pode ser desfeita.",
                "Confirmar Exclusao",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                if (formulationRepo.Delete(formulationId))
                {
                    MessageBox.Show("Formulacao excluida com sucesso!",
                        "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadFormulations();
                }
                else
                {
                    MessageBox.Show("Erro ao excluir formulacao.",
                        "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao excluir formulacao: {ex.Message}",
                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnAddIngredient_Click(object? sender, EventArgs e)
    {
        if (cmbIngredientProduct.SelectedItem == null)
        {
            MessageBox.Show("Selecione um produto.",
                "Validacao", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(txtIngredientQuantity.Text) ||
            string.IsNullOrWhiteSpace(txtIngredientPercentage.Text))
        {
            MessageBox.Show("Informe a quantidade e o percentual.",
                "Validacao", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            var product = (cmbIngredientProduct.SelectedItem as ComboBoxItem)?.Value as Product;
            decimal quantity = decimal.Parse(txtIngredientQuantity.Text, CultureInfo.GetCultureInfo("pt-BR"));
            decimal percentage = decimal.Parse(txtIngredientPercentage.Text, CultureInfo.GetCultureInfo("pt-BR"));

            var ingredient = new FormulationIngredient
            {
                Product = product!,
                ProductId = product!.Id,
                Quantity = quantity,
                Percentage = percentage
            };

            currentIngredients.Add(ingredient);
            RefreshIngredientsGrid();

            // Clear input fields
            txtIngredientQuantity.Clear();
            txtIngredientPercentage.Clear();
            cmbIngredientProduct.Focus();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao adicionar ingrediente: {ex.Message}",
                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnRemoveIngredient_Click(object? sender, EventArgs e)
    {
        if (dgvIngredients.SelectedRows.Count == 0)
        {
            MessageBox.Show("Selecione um ingrediente para remover.",
                "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        int selectedIndex = dgvIngredients.SelectedRows[0].Index;
        if (selectedIndex >= 0 && selectedIndex < currentIngredients.Count)
        {
            currentIngredients.RemoveAt(selectedIndex);
            RefreshIngredientsGrid();
        }
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            MessageBox.Show("Por favor, informe o nome da formulacao.",
                "Validacao", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtName.Focus();
            return;
        }

        if (currentIngredients.Count == 0)
        {
            MessageBox.Show("Por favor, adicione pelo menos um ingrediente.",
                "Validacao", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            decimal yieldPercentage = decimal.Parse(txtYield.Text, CultureInfo.GetCultureInfo("pt-BR"));

            if (isEditMode && currentFormulation != null)
            {
                // Update existing formulation
                currentFormulation.Name = txtName.Text;
                currentFormulation.YieldPercentage = yieldPercentage;

                formulationRepo.Update(currentFormulation);
                formulationRepo.UpdateIngredients(currentFormulation.Id, currentIngredients);

                MessageBox.Show("Formulacao atualizada com sucesso!",
                    "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                // Add new formulation
                var newFormulation = new Formulation
                {
                    Name = txtName.Text,
                    YieldPercentage = yieldPercentage,
                    CreatedDate = DateTime.Now
                };

                formulationRepo.Add(newFormulation);

                // Add ingredients
                foreach (var ingredient in currentIngredients)
                {
                    ingredient.FormulationId = newFormulation.Id;
                    formulationRepo.AddIngredient(ingredient);
                }

                MessageBox.Show("Formulacao adicionada com sucesso!",
                    "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            grpFormulationDetails.Visible = false;
            LoadFormulations();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao salvar formulacao: {ex.Message}",
                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnCancel_Click(object? sender, EventArgs e)
    {
        grpFormulationDetails.Visible = false;
        currentFormulation = null;
        currentIngredients.Clear();
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

    private class ComboBoxItem
    {
        public string Text { get; set; } = string.Empty;
        public object? Value { get; set; }

        public override string ToString() => Text;
    }
}
