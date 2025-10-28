using System.Windows.Forms;

namespace ZebraLabelPrinter.Forms;

/// <summary>
/// Main menu form with navigation to all application modules
/// Uses MenuStrip for easy navigation between different features
/// </summary>
public class MainMenuForm : Form
{
    private MenuStrip menuStrip = null!;
    private Panel contentPanel = null!;
    private Label welcomeLabel = null!;

    public MainMenuForm()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        // Form configuration
        this.Text = "Sistema de Producao - Menu Principal";
        this.Size = new Size(1200, 800);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.WindowState = FormWindowState.Maximized;
        this.IsMdiContainer = true; // Enable MDI for child forms

        // Create MenuStrip
        menuStrip = new MenuStrip
        {
            Font = new Font("Segoe UI", 11F, FontStyle.Regular),
            BackColor = Color.FromArgb(240, 240, 240)
        };

        // Production menu
        var productionMenu = new ToolStripMenuItem("Producao")
        {
            Font = new Font("Segoe UI", 11F, FontStyle.Bold)
        };
        productionMenu.Click += (s, e) => OpenProductionForm();

        // Products menu
        var productsMenu = new ToolStripMenuItem("Produtos")
        {
            Font = new Font("Segoe UI", 11F, FontStyle.Bold)
        };
        productsMenu.Click += (s, e) => OpenProductsForm();

        // Formulations menu
        var formulationsMenu = new ToolStripMenuItem("Formulacoes")
        {
            Font = new Font("Segoe UI", 11F, FontStyle.Bold)
        };
        formulationsMenu.Click += (s, e) => OpenFormulationsForm();

        // Costs menu
        var costsMenu = new ToolStripMenuItem("Custos")
        {
            Font = new Font("Segoe UI", 11F, FontStyle.Bold)
        };
        costsMenu.Click += (s, e) => OpenCostsForm();

        // Reports menu
        var reportsMenu = new ToolStripMenuItem("Relatorios")
        {
            Font = new Font("Segoe UI", 11F, FontStyle.Bold)
        };
        reportsMenu.Click += (s, e) => OpenReportsForm();

        // Add menus to MenuStrip
        menuStrip.Items.Add(productionMenu);
        menuStrip.Items.Add(productsMenu);
        menuStrip.Items.Add(formulationsMenu);
        menuStrip.Items.Add(costsMenu);
        menuStrip.Items.Add(reportsMenu);

        // Content panel for welcome message
        contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White
        };

        welcomeLabel = new Label
        {
            Text = "Bem-vindo ao Sistema de Producao\n\n" +
                   "Selecione uma opcao no menu acima:\n\n" +
                   "• Producao - Gerenciar producao e impressao de etiquetas\n" +
                   "• Produtos - Cadastrar produtos acabados e materias-primas\n" +
                   "• Formulacoes - Criar receitas com ingredientes\n" +
                   "• Custos - Registrar custos de producao\n" +
                   "• Relatorios - Visualizar lucros e resumos",
            AutoSize = false,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 14F, FontStyle.Regular),
            ForeColor = Color.FromArgb(64, 64, 64)
        };

        contentPanel.Controls.Add(welcomeLabel);

        // Add controls to form
        this.MainMenuStrip = menuStrip;
        this.Controls.Add(contentPanel);
        this.Controls.Add(menuStrip);
    }

    private void OpenProductionForm()
    {
        try
        {
            // Hide welcome panel to show MDI child forms
            contentPanel.Visible = false;

            // Close any existing child forms
            foreach (Form childForm in this.MdiChildren)
            {
                childForm.Close();
            }

            var productionForm = new ProductionForm
            {
                MdiParent = this,
                Dock = DockStyle.Fill
            };
            productionForm.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"ERRO ao abrir Producao:\n\n{ex.Message}\n\nStack:\n{ex.StackTrace}\n\nInner: {ex.InnerException?.Message}",
                "Erro - Producao", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OpenProductsForm()
    {
        try
        {
            // Hide welcome panel to show MDI child forms
            contentPanel.Visible = false;

            // Close any existing child forms
            foreach (Form childForm in this.MdiChildren)
            {
                childForm.Close();
            }

            var productsForm = new ProductsForm
            {
                MdiParent = this,
                Dock = DockStyle.Fill
            };
            productsForm.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"ERRO ao abrir Produtos:\n\n{ex.Message}\n\nStack:\n{ex.StackTrace}",
                "Erro - Produtos", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OpenFormulationsForm()
    {
        try
        {
            // Hide welcome panel to show MDI child forms
            contentPanel.Visible = false;

            // Close any existing child forms
            foreach (Form childForm in this.MdiChildren)
            {
                childForm.Close();
            }

            var formulationsForm = new FormulationsForm
            {
                MdiParent = this,
                Dock = DockStyle.Fill
            };
            formulationsForm.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"ERRO ao abrir Formulacoes:\n\n{ex.Message}\n\nStack:\n{ex.StackTrace}",
                "Erro - Formulacoes", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OpenCostsForm()
    {
        try
        {
            // Hide welcome panel to show MDI child forms
            contentPanel.Visible = false;

            // Close any existing child forms
            foreach (Form childForm in this.MdiChildren)
            {
                childForm.Close();
            }

            var costsForm = new CostsForm
            {
                MdiParent = this,
                Dock = DockStyle.Fill
            };
            costsForm.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"ERRO ao abrir Custos:\n\n{ex.Message}\n\nStack:\n{ex.StackTrace}",
                "Erro - Custos", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OpenReportsForm()
    {
        try
        {
            // Hide welcome panel to show MDI child forms
            contentPanel.Visible = false;

            // Close any existing child forms
            foreach (Form childForm in this.MdiChildren)
            {
                childForm.Close();
            }

            var reportsForm = new ReportsForm
            {
                MdiParent = this,
                Dock = DockStyle.Fill
            };
            reportsForm.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"ERRO ao abrir Relatorios:\n\n{ex.Message}\n\nStack:\n{ex.StackTrace}",
                "Erro - Relatorios", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
