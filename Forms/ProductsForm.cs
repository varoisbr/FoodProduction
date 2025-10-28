using System.Globalization;
using ZebraLabelPrinter.Models;
using ZebraLabelPrinter.Repositories;

namespace ZebraLabelPrinter.Forms;

/// <summary>
/// Products management form
/// Provides CRUD operations for managing products (both finished products and raw materials)
/// </summary>
public class ProductsForm : Form
{
    private DataGridView dgvProducts = null!;
    private Button btnAdd = null!;
    private Button btnEdit = null!;
    private Button btnDelete = null!;
    private Button btnRefresh = null!;
    private TextBox txtSearch = null!;
    private Label lblSearch = null!;
    private ComboBox cmbFilterType = null!;
    private Label lblFilterType = null!;
    private GroupBox grpProductDetails = null!;
    private TextBox txtName = null!;
    private Label lblName = null!;
    private ComboBox cmbType = null!;
    private Label lblType = null!;
    private TextBox txtDefaultPrice = null!;
    private Label lblDefaultPrice = null!;
    private Button btnSave = null!;
    private Button btnCancel = null!;

    private readonly ProductRepository productRepo = new();
    private Product? currentProduct = null;
    private bool isEditMode = false;

    public ProductsForm()
    {
        InitializeComponents();
        this.Load += ProductsForm_Load;
    }

    private void ProductsForm_Load(object? sender, EventArgs e)
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
    }

    private void InitializeComponents()
    {
        // Form configuration
        this.Text = "Gerenciamento de Produtos";
        this.Size = new Size(1000, 700);
        this.FormBorderStyle = FormBorderStyle.None;
        this.BackColor = Color.White;
        this.Padding = new Padding(20);

        // Search label
        lblSearch = new Label
        {
            Text = "Buscar:",
            Location = new Point(30, 20),
            Size = new Size(60, 25),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };

        // Search textbox
        txtSearch = new TextBox
        {
            Location = new Point(100, 18),
            Size = new Size(200, 25),
            Font = new Font("Segoe UI", 10F)
        };
        txtSearch.TextChanged += TxtSearch_TextChanged;

        // Filter type label
        lblFilterType = new Label
        {
            Text = "Tipo:",
            Location = new Point(320, 20),
            Size = new Size(40, 25),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };

        // Filter type combobox
        cmbFilterType = new ComboBox
        {
            Location = new Point(370, 18),
            Size = new Size(150, 25),
            Font = new Font("Segoe UI", 10F),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cmbFilterType.Items.AddRange(new[] { "Todos", "Finished", "Raw" });
        cmbFilterType.SelectedIndex = 0;
        cmbFilterType.SelectedIndexChanged += CmbFilterType_SelectedIndexChanged;

        // Buttons
        btnAdd = new Button
        {
            Text = "Novo Produto",
            Location = new Point(550, 15),
            Size = new Size(120, 30),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            BackColor = Color.LightGreen
        };
        btnAdd.Click += BtnAdd_Click;

        btnEdit = new Button
        {
            Text = "Editar",
            Location = new Point(680, 15),
            Size = new Size(80, 30),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            BackColor = Color.LightBlue
        };
        btnEdit.Click += BtnEdit_Click;

        btnDelete = new Button
        {
            Text = "Excluir",
            Location = new Point(770, 15),
            Size = new Size(80, 30),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            BackColor = Color.LightCoral
        };
        btnDelete.Click += BtnDelete_Click;

        btnRefresh = new Button
        {
            Text = "Atualizar",
            Location = new Point(860, 15),
            Size = new Size(90, 30),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            BackColor = Color.LightGray
        };
        btnRefresh.Click += (s, e) => LoadProducts();

        // DataGridView
        dgvProducts = new DataGridView
        {
            Location = new Point(30, 60),
            Size = new Size(920, 300),
            Font = new Font("Segoe UI", 9F),
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };

        // Product details GroupBox
        grpProductDetails = new GroupBox
        {
            Text = "Detalhes do Produto",
            Location = new Point(30, 380),
            Size = new Size(920, 270),
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            Visible = false
        };

        // Name label
        lblName = new Label
        {
            Text = "Nome do Produto:",
            Location = new Point(20, 40),
            Size = new Size(150, 25),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };

        // Name textbox
        txtName = new TextBox
        {
            Location = new Point(180, 38),
            Size = new Size(700, 25),
            Font = new Font("Segoe UI", 10F)
        };

        // Type label
        lblType = new Label
        {
            Text = "Tipo:",
            Location = new Point(20, 80),
            Size = new Size(150, 25),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };

        // Type combobox
        cmbType = new ComboBox
        {
            Location = new Point(180, 78),
            Size = new Size(200, 25),
            Font = new Font("Segoe UI", 10F),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cmbType.Items.AddRange(new[] { "Finished", "Raw" });
        cmbType.SelectedIndex = 0;

        // Default price label
        lblDefaultPrice = new Label
        {
            Text = "Preco Padrao (R$/kg):",
            Location = new Point(20, 120),
            Size = new Size(150, 25),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };

        // Default price textbox
        txtDefaultPrice = new TextBox
        {
            Location = new Point(180, 118),
            Size = new Size(200, 25),
            Font = new Font("Segoe UI", 10F)
        };
        txtDefaultPrice.KeyPress += NumericTextBox_KeyPress;

        // Save button
        btnSave = new Button
        {
            Text = "Salvar",
            Location = new Point(180, 170),
            Size = new Size(120, 40),
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            BackColor = Color.LightGreen
        };
        btnSave.Click += BtnSave_Click;

        // Cancel button
        btnCancel = new Button
        {
            Text = "Cancelar",
            Location = new Point(310, 170),
            Size = new Size(120, 40),
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            BackColor = Color.LightGray
        };
        btnCancel.Click += BtnCancel_Click;

        // Add controls to GroupBox
        grpProductDetails.Controls.Add(lblName);
        grpProductDetails.Controls.Add(txtName);
        grpProductDetails.Controls.Add(lblType);
        grpProductDetails.Controls.Add(cmbType);
        grpProductDetails.Controls.Add(lblDefaultPrice);
        grpProductDetails.Controls.Add(txtDefaultPrice);
        grpProductDetails.Controls.Add(btnSave);
        grpProductDetails.Controls.Add(btnCancel);

        // Add controls to form
        this.Controls.Add(lblSearch);
        this.Controls.Add(txtSearch);
        this.Controls.Add(lblFilterType);
        this.Controls.Add(cmbFilterType);
        this.Controls.Add(btnAdd);
        this.Controls.Add(btnEdit);
        this.Controls.Add(btnDelete);
        this.Controls.Add(btnRefresh);
        this.Controls.Add(dgvProducts);
        this.Controls.Add(grpProductDetails);
    }

    private void LoadProducts()
    {
        try
        {
            var products = productRepo.GetAll();

            dgvProducts.DataSource = null;
            dgvProducts.Columns.Clear();

            var displayData = products.Select(p => new
            {
                p.Id,
                Nome = p.Name,
                Tipo = p.Type == "Finished" ? "Acabado" : "Materia-Prima",
                PrecoKg = $"R$ {p.DefaultPricePerKg:F2}"
            }).ToList();

            dgvProducts.DataSource = displayData;

            // Hide ID column
            if (dgvProducts.Columns["Id"] != null)
                dgvProducts.Columns["Id"]!.Visible = false;

            ApplyFilters();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao carregar produtos: {ex.Message}",
                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ApplyFilters()
    {
        if (dgvProducts.DataSource == null) return;

        try
        {
            var searchText = txtSearch.Text.ToLower();
            var filterType = cmbFilterType.SelectedItem?.ToString();

            foreach (DataGridViewRow row in dgvProducts.Rows)
            {
                var nome = row.Cells["Nome"].Value?.ToString()?.ToLower() ?? "";
                var tipo = row.Cells["Tipo"].Value?.ToString() ?? "";

                bool matchesSearch = string.IsNullOrWhiteSpace(searchText) || nome.Contains(searchText);
                bool matchesType = filterType == "Todos" ||
                    (filterType == "Finished" && tipo == "Acabado") ||
                    (filterType == "Raw" && tipo == "Materia-Prima");

                row.Visible = matchesSearch && matchesType;
            }
        }
        catch { }
    }

    private void TxtSearch_TextChanged(object? sender, EventArgs e)
    {
        ApplyFilters();
    }

    private void CmbFilterType_SelectedIndexChanged(object? sender, EventArgs e)
    {
        ApplyFilters();
    }

    private void BtnAdd_Click(object? sender, EventArgs e)
    {
        isEditMode = false;
        currentProduct = null;

        txtName.Clear();
        cmbType.SelectedIndex = 0;
        txtDefaultPrice.Clear();

        grpProductDetails.Visible = true;
        txtName.Focus();
    }

    private void BtnEdit_Click(object? sender, EventArgs e)
    {
        if (dgvProducts.SelectedRows.Count == 0)
        {
            MessageBox.Show("Selecione um produto para editar.",
                "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            var selectedRow = dgvProducts.SelectedRows[0];
            int productId = Convert.ToInt32(selectedRow.Cells["Id"].Value);

            currentProduct = productRepo.GetById(productId);
            if (currentProduct == null)
            {
                MessageBox.Show("Produto nao encontrado.",
                    "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            isEditMode = true;

            txtName.Text = currentProduct.Name;
            cmbType.SelectedItem = currentProduct.Type;
            txtDefaultPrice.Text = currentProduct.DefaultPricePerKg.ToString("F2", CultureInfo.GetCultureInfo("pt-BR"));

            grpProductDetails.Visible = true;
            txtName.Focus();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao carregar produto: {ex.Message}",
                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (dgvProducts.SelectedRows.Count == 0)
        {
            MessageBox.Show("Selecione um produto para excluir.",
                "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            var selectedRow = dgvProducts.SelectedRows[0];
            string productName = selectedRow.Cells["Nome"].Value?.ToString() ?? "";
            int productId = Convert.ToInt32(selectedRow.Cells["Id"].Value);

            var result = MessageBox.Show(
                $"Deseja realmente excluir o produto '{productName}'?\n\n" +
                "Esta acao nao pode ser desfeita.",
                "Confirmar Exclusao",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                if (productRepo.Delete(productId))
                {
                    MessageBox.Show("Produto excluido com sucesso!",
                        "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadProducts();
                }
                else
                {
                    MessageBox.Show("Erro ao excluir produto.",
                        "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao excluir produto: {ex.Message}\n\n" +
                "O produto pode estar em uso em producoes ou formulacoes.",
                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            MessageBox.Show("Por favor, informe o nome do produto.",
                "Validacao", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtName.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(txtDefaultPrice.Text))
        {
            MessageBox.Show("Por favor, informe o preco padrao.",
                "Validacao", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtDefaultPrice.Focus();
            return;
        }

        try
        {
            decimal defaultPrice = decimal.Parse(txtDefaultPrice.Text, CultureInfo.GetCultureInfo("pt-BR"));

            if (defaultPrice < 0)
            {
                MessageBox.Show("O preco deve ser maior ou igual a zero.",
                    "Validacao", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check if name already exists
            if (productRepo.NameExists(txtName.Text, currentProduct?.Id))
            {
                MessageBox.Show("Ja existe um produto com este nome.",
                    "Validacao", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            if (isEditMode && currentProduct != null)
            {
                // Update existing product
                currentProduct.Name = txtName.Text;
                currentProduct.Type = cmbType.SelectedItem?.ToString() ?? "Finished";
                currentProduct.DefaultPricePerKg = defaultPrice;

                productRepo.Update(currentProduct);

                MessageBox.Show("Produto atualizado com sucesso!",
                    "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                // Add new product
                var newProduct = new Product
                {
                    Name = txtName.Text,
                    Type = cmbType.SelectedItem?.ToString() ?? "Finished",
                    DefaultPricePerKg = defaultPrice
                };

                productRepo.Add(newProduct);

                MessageBox.Show("Produto adicionado com sucesso!",
                    "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            grpProductDetails.Visible = false;
            LoadProducts();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao salvar produto: {ex.Message}",
                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnCancel_Click(object? sender, EventArgs e)
    {
        grpProductDetails.Visible = false;
        currentProduct = null;
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
}
