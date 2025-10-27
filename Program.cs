using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Globalization;
using System.Collections.Generic;

namespace ZebraLabelPrinter
{
    public class MainForm : Form
    {
        private TextBox txtPrecoPorKg;
        private TextBox txtPeso;
        private Label lblTotal;
        private Button btnCalcular;
        private Button btnImprimir;
        private Label lblPrecoPorKgLabel;
        private Label lblPesoLabel;
        private TextBox txtZplTemplate;
        private Label lblZplTemplateLabel;
        private TextBox txtNomeProducao;
        private Label lblNomeProducaoLabel;
        private Button btnExportarCSV;
        private Button btnNovaProducao;
        private Button btnCarregarCSV;

        private decimal totalCalculado = 0;
        private decimal precoPorKg = 0;

        // Lista para armazenar os registros de impressão
        private List<RegistroImpressao> registrosImpressao = new List<RegistroImpressao>();

        // Caminho do arquivo CSV da produção atual
        private string caminhoCSVAtual = string.Empty;
        
        // Template ZPL padrão - usado se o campo customizado estiver vazio
        private const string ZPL_TEMPLATE = @"^XA
^FO50,50^ADN,36,20^FDPreco Total: ##TOTAL##^FS
^FO50,100^ADN,36,20^FDPeso: ##PESO## kg^FS
^XZ";

        // Classe para armazenar dados de cada impressão
        private class RegistroImpressao
        {
            public string NomeProducao { get; set; }
            public DateTime DataHora { get; set; }
            public decimal PrecoPorKg { get; set; }
            public decimal Peso { get; set; }
            public decimal Total { get; set; }
        }

        public MainForm()
        {
            InitializeComponents();
            ConfigureTabOrder();
        }

        private void InitializeComponents()
        {
            // Configurações do Form
            this.Text = "Impressora de Etiquetas Zebra";
            this.Size = new Size(500, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Font = new Font("Segoe UI", 10F);

            // Label Nome da Produção
            lblNomeProducaoLabel = new Label
            {
                Text = "Nome da Produção:",
                Location = new Point(30, 30),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            };

            // TextBox Nome da Produção
            txtNomeProducao = new TextBox
            {
                Location = new Point(30, 60),
                Size = new Size(420, 35),
                Font = new Font("Segoe UI", 14F),
                TabIndex = 0,
                PlaceholderText = "Ex: Lote 001, Produção Manhã, etc."
            };
            txtNomeProducao.KeyDown += TxtNomeProducao_KeyDown;

            // Label Preço por KG
            lblPrecoPorKgLabel = new Label
            {
                Text = "Preço por KG (R$):",
                Location = new Point(30, 110),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            };

            // TextBox Preço por KG
            txtPrecoPorKg = new TextBox
            {
                Location = new Point(30, 140),
                Size = new Size(420, 35),
                Font = new Font("Segoe UI", 14F),
                TabIndex = 1
            };
            txtPrecoPorKg.KeyPress += NumericTextBox_KeyPress;
            txtPrecoPorKg.KeyDown += TxtPrecoPorKg_KeyDown;

            // Label Peso
            lblPesoLabel = new Label
            {
                Text = "Peso (kg):",
                Location = new Point(30, 190),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            };

            // TextBox Peso
            txtPeso = new TextBox
            {
                Location = new Point(30, 220),
                Size = new Size(420, 35),
                Font = new Font("Segoe UI", 14F),
                TabIndex = 2
            };
            txtPeso.KeyPress += NumericTextBox_KeyPress;
            txtPeso.KeyDown += TxtPeso_KeyDown;

            // Label Total
            lblTotal = new Label
            {
                Text = "Total: R$ 0,00",
                Location = new Point(30, 270),
                Size = new Size(420, 40),
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
                Location = new Point(30, 325),
                Size = new Size(420, 25),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            // TextBox ZPL Template
            txtZplTemplate = new TextBox
            {
                Location = new Point(30, 355),
                Size = new Size(420, 100),
                Font = new Font("Consolas", 9F),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                TabIndex = 3,
                Text = "Cole aqui o código ZPL customizado\r\nUse ##TOTAL## para o valor total e ##PESO## para o peso\r\nDeixe vazio para usar o template padrão",
                ForeColor = Color.Gray
            };
            txtZplTemplate.GotFocus += TxtZplTemplate_GotFocus;
            txtZplTemplate.LostFocus += TxtZplTemplate_LostFocus;

            // Botão Calcular
            btnCalcular = new Button
            {
                Text = "Calcular (Enter)",
                Location = new Point(30, 470),
                Size = new Size(200, 45),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                TabIndex = 4,
                BackColor = Color.LightBlue
            };
            btnCalcular.Click += BtnCalcular_Click;

            // Botão Imprimir
            btnImprimir = new Button
            {
                Text = "Imprimir (Enter)",
                Location = new Point(250, 470),
                Size = new Size(200, 45),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                TabIndex = 5,
                BackColor = Color.LightGreen,
                Enabled = false
            };
            btnImprimir.Click += BtnImprimir_Click;
            btnImprimir.KeyDown += BtnImprimir_KeyDown;

            // Botão Exportar CSV
            btnExportarCSV = new Button
            {
                Text = "📊 Exportar CSV",
                Location = new Point(30, 530),
                Size = new Size(420, 45),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                TabIndex = 6,
                BackColor = Color.LightCoral
            };
            btnExportarCSV.Click += BtnExportarCSV_Click;

            // Botão Nova Produção
            btnNovaProducao = new Button
            {
                Text = "🔄 Nova Produção",
                Location = new Point(30, 590),
                Size = new Size(200, 45),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                TabIndex = 7,
                BackColor = Color.LightSalmon
            };
            btnNovaProducao.Click += BtnNovaProducao_Click;

            // Botão Carregar CSV
            btnCarregarCSV = new Button
            {
                Text = "📂 Carregar CSV",
                Location = new Point(250, 590),
                Size = new Size(200, 45),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                TabIndex = 8,
                BackColor = Color.LightSkyBlue
            };
            btnCarregarCSV.Click += BtnCarregarCSV_Click;

            // Adicionar controles ao Form
            this.Controls.Add(lblNomeProducaoLabel);
            this.Controls.Add(txtNomeProducao);
            this.Controls.Add(lblPrecoPorKgLabel);
            this.Controls.Add(txtPrecoPorKg);
            this.Controls.Add(lblPesoLabel);
            this.Controls.Add(txtPeso);
            this.Controls.Add(lblTotal);
            this.Controls.Add(lblZplTemplateLabel);
            this.Controls.Add(txtZplTemplate);
            this.Controls.Add(btnCalcular);
            this.Controls.Add(btnImprimir);
            this.Controls.Add(btnExportarCSV);
            this.Controls.Add(btnNovaProducao);
            this.Controls.Add(btnCarregarCSV);
        }

        private void ConfigureTabOrder()
        {
            txtNomeProducao.TabIndex = 0;
            txtPrecoPorKg.TabIndex = 1;
            txtPeso.TabIndex = 2;
            txtZplTemplate.TabIndex = 3;
            btnCalcular.TabIndex = 4;
            btnImprimir.TabIndex = 5;
            btnExportarCSV.TabIndex = 6;
            btnNovaProducao.TabIndex = 7;
            btnCarregarCSV.TabIndex = 8;
        }

        private void TxtNomeProducao_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                txtPrecoPorKg.Focus();
            }
        }

        private void TxtZplTemplate_GotFocus(object sender, EventArgs e)
        {
            if (txtZplTemplate.ForeColor == Color.Gray)
            {
                txtZplTemplate.Text = "";
                txtZplTemplate.ForeColor = Color.Black;
            }
        }

        private void TxtZplTemplate_LostFocus(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtZplTemplate.Text))
            {
                txtZplTemplate.Text = "Cole aqui o código ZPL customizado\r\nUse ##TOTAL## para o valor total e ##PESO## para o peso\r\nDeixe vazio para usar o template padrão";
                txtZplTemplate.ForeColor = Color.Gray;
            }
        }

        private void NumericTextBox_KeyPress(object sender, KeyPressEventArgs e)
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
            if (e.KeyChar == ',' && (sender as TextBox).Text.Contains(","))
            {
                e.Handled = true;
            }
        }

        private void TxtPrecoPorKg_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                txtPeso.Focus();
            }
        }

        private void TxtPeso_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                BtnCalcular_Click(sender, e);
            }
        }

        private void BtnImprimir_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                BtnImprimir_Click(sender, e);
            }
        }

        private void BtnCalcular_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPrecoPorKg.Text) || 
                string.IsNullOrWhiteSpace(txtPeso.Text))
            {
                MessageBox.Show("Por favor, preencha o preço e o peso.", 
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

        private void BtnImprimir_Click(object sender, EventArgs e)
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
                        "O template ZPL não contém os marcadores ##TOTAL## ou ##PESO##.\n\n" +
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
                // Manter case sensitive - não alterar maiúsculas/minúsculas
                string totalFormatado = totalCalculado.ToString("F2", 
                    CultureInfo.GetCultureInfo("pt-BR"));
                string pesoFormatado = peso.ToString("F3", 
                    CultureInfo.GetCultureInfo("pt-BR"));
                
                string zplFinal = templateZpl
                    .Replace("##TOTAL##", totalFormatado)
                    .Replace("##PESO##", pesoFormatado);

                // Enviar para LPT1 usando System.Diagnostics.Process (mesmo método que CMD)
                EnviarParaImpressora(zplFinal);

                MessageBox.Show("Impressão enviada com sucesso!", 
                    "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Registrar a impressão para exportação CSV
                RegistrarImpressao();

                // Limpar campos para próxima etiqueta
                LimparCampos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao enviar para impressora: {ex.Message}\n\n" +
                    "Verifique se a impressora está mapeada corretamente (net use LPT1).", 
                    "Erro de Impressão", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LimparCampos()
        {
            // NÃO limpar o preço por kg - mantém para próximas impressões
            // txtPrecoPorKg.Clear();
            
            // Limpar apenas peso e total
            txtPeso.Clear();
            lblTotal.Text = "Total: R$ 0,00";
            totalCalculado = 0;
            btnImprimir.Enabled = false;
            
            // Voltar foco para o campo de peso (ciclo peso -> imprimir)
            txtPeso.Focus();
        }

        private void RegistrarImpressao()
        {
            decimal peso = decimal.Parse(txtPeso.Text, CultureInfo.GetCultureInfo("pt-BR"));

            var registro = new RegistroImpressao
            {
                NomeProducao = string.IsNullOrWhiteSpace(txtNomeProducao.Text)
                    ? "Sem Nome"
                    : txtNomeProducao.Text,
                DataHora = DateTime.Now,
                PrecoPorKg = precoPorKg,
                Peso = peso,
                Total = totalCalculado
            };

            registrosImpressao.Add(registro);

            // Atualizar texto do botão com contador
            btnExportarCSV.Text = $"📊 Exportar CSV ({registrosImpressao.Count} registros)";

            // Salvar automaticamente o CSV
            SalvarCSVAutomatico();
        }

        private void SalvarCSVAutomatico()
        {
            try
            {
                // Se não existe arquivo CSV, criar um novo
                if (string.IsNullOrEmpty(caminhoCSVAtual))
                {
                    string nomeProducao = string.IsNullOrWhiteSpace(txtNomeProducao.Text)
                        ? "Sem_Nome"
                        : txtNomeProducao.Text.Replace(" ", "_");

                    string nomeArquivo = $"Producao_{nomeProducao}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    caminhoCSVAtual = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, nomeArquivo);
                }

                // Criar conteúdo CSV
                var csv = new System.Text.StringBuilder();
                csv.AppendLine("Nome Producao;Data/Hora;Preco por KG;Peso (kg);Total (R$)");

                foreach (var registro in registrosImpressao)
                {
                    csv.AppendLine($"{registro.NomeProducao};{registro.DataHora:dd/MM/yyyy HH:mm:ss};{registro.PrecoPorKg:F2};{registro.Peso:F3};{registro.Total:F2}");
                }

                // Salvar arquivo
                File.WriteAllText(caminhoCSVAtual, csv.ToString(), System.Text.Encoding.UTF8);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar CSV automaticamente: {ex.Message}",
                    "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnExportarCSV_Click(object sender, EventArgs e)
        {
            if (registrosImpressao.Count == 0)
            {
                MessageBox.Show("Não há registros para exportar.\n\nImprima algumas etiquetas primeiro.", 
                    "Nenhum Registro", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                // Criar nome do arquivo com data/hora
                string nomeArquivo = $"Producao_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                string caminhoCompleto = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, nomeArquivo);

                // Criar conteúdo CSV
                var csv = new System.Text.StringBuilder();
                csv.AppendLine("Nome Producao;Data/Hora;Preco por KG;Peso (kg);Total (R$)");

                foreach (var registro in registrosImpressao)
                {
                    csv.AppendLine($"{registro.NomeProducao};{registro.DataHora:dd/MM/yyyy HH:mm:ss};{registro.PrecoPorKg:F2};{registro.Peso:F3};{registro.Total:F2}");
                }

                // Salvar arquivo
                File.WriteAllText(caminhoCompleto, csv.ToString(), System.Text.Encoding.UTF8);

                var result = MessageBox.Show(
                    $"CSV exportado com sucesso!\n\n" +
                    $"Arquivo: {nomeArquivo}\n" +
                    $"Total de registros: {registrosImpressao.Count}\n\n" +
                    $"Deseja abrir a pasta?",
                    "Exportação Concluída",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (result == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{caminhoCompleto}\"");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao exportar CSV: {ex.Message}", 
                    "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnNovaProducao_Click(object sender, EventArgs e)
        {
            // Verificar se há registros não salvos
            if (registrosImpressao.Count > 0)
            {
                var result = MessageBox.Show(
                    $"Você tem {registrosImpressao.Count} registro(s) na produção atual.\n\n" +
                    "Deseja realmente iniciar uma nova produção?\n\n" +
                    "Os dados atuais foram salvos automaticamente em:\n" +
                    $"{Path.GetFileName(caminhoCSVAtual)}",
                    "Confirmar Nova Produção",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.No)
                    return;
            }

            // Limpar todos os campos e registros
            txtNomeProducao.Clear();
            txtPrecoPorKg.Clear();
            txtPeso.Clear();
            lblTotal.Text = "Total: R$ 0,00";
            totalCalculado = 0;
            precoPorKg = 0;
            btnImprimir.Enabled = false;

            // Limpar registros e caminho do CSV
            registrosImpressao.Clear();
            caminhoCSVAtual = string.Empty;

            // Atualizar botão de exportar
            btnExportarCSV.Text = "📊 Exportar CSV";

            // Focar no campo de nome da produção
            txtNomeProducao.Focus();

            MessageBox.Show("Nova produção iniciada!\n\nPreencha os campos para começar.",
                "Produção Zerada", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnCarregarCSV_Click(object sender, EventArgs e)
        {
            // Verificar se há registros não salvos
            if (registrosImpressao.Count > 0)
            {
                var result = MessageBox.Show(
                    $"Você tem {registrosImpressao.Count} registro(s) na produção atual.\n\n" +
                    "Deseja realmente carregar outra produção?\n\n" +
                    "Os dados atuais foram salvos em:\n" +
                    $"{Path.GetFileName(caminhoCSVAtual)}",
                    "Confirmar Carregamento",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.No)
                    return;
            }

            // Abrir diálogo para selecionar arquivo CSV
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Arquivos CSV (*.csv)|*.csv|Todos os arquivos (*.*)|*.*";
                openFileDialog.Title = "Selecione o arquivo CSV da produção";
                openFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        CarregarCSV(openFileDialog.FileName);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erro ao carregar CSV: {ex.Message}",
                            "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void CarregarCSV(string caminho)
        {
            // Limpar registros atuais
            registrosImpressao.Clear();

            // Ler arquivo CSV
            var linhas = File.ReadAllLines(caminho, System.Text.Encoding.UTF8);

            if (linhas.Length < 2)
            {
                throw new Exception("Arquivo CSV vazio ou inválido.");
            }

            // Pular o cabeçalho (primeira linha)
            for (int i = 1; i < linhas.Length; i++)
            {
                var campos = linhas[i].Split(';');

                if (campos.Length != 5)
                    continue;

                try
                {
                    var registro = new RegistroImpressao
                    {
                        NomeProducao = campos[0],
                        DataHora = DateTime.ParseExact(campos[1], "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                        PrecoPorKg = decimal.Parse(campos[2], CultureInfo.GetCultureInfo("pt-BR")),
                        Peso = decimal.Parse(campos[3], CultureInfo.GetCultureInfo("pt-BR")),
                        Total = decimal.Parse(campos[4], CultureInfo.GetCultureInfo("pt-BR"))
                    };

                    registrosImpressao.Add(registro);
                }
                catch
                {
                    // Ignorar linhas com erro
                    continue;
                }
            }

            if (registrosImpressao.Count == 0)
            {
                throw new Exception("Nenhum registro válido encontrado no CSV.");
            }

            // Configurar campos com base no último registro
            var ultimoRegistro = registrosImpressao[registrosImpressao.Count - 1];
            txtNomeProducao.Text = ultimoRegistro.NomeProducao;
            txtPrecoPorKg.Text = ultimoRegistro.PrecoPorKg.ToString("F2", CultureInfo.GetCultureInfo("pt-BR"));

            // Definir caminho do CSV atual
            caminhoCSVAtual = caminho;

            // Atualizar botão de exportar
            btnExportarCSV.Text = $"📊 Exportar CSV ({registrosImpressao.Count} registros)";

            // Limpar campos de pesagem
            txtPeso.Clear();
            lblTotal.Text = "Total: R$ 0,00";
            totalCalculado = 0;
            btnImprimir.Enabled = false;

            // Focar no campo de peso
            txtPeso.Focus();

            MessageBox.Show(
                $"CSV carregado com sucesso!\n\n" +
                $"Arquivo: {Path.GetFileName(caminho)}\n" +
                $"Registros carregados: {registrosImpressao.Count}\n\n" +
                $"Continue a produção inserindo novos pesos.",
                "Carregamento Concluído",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

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
                    process.WaitForExit(5000); // Timeout de 5 segundos

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

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}