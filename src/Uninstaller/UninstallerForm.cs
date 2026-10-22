using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace OpenClevoRGBUninstaller
{
    public class UninstallerForm : Form
    {
        private Panel headerPanel;
        private Label lblTitle;
        private Label lblSubtitle;

        private Panel cardInfo;
        private Label lblInfoTitle;
        private Label lblItemsList;
        private CheckBox chkRemoveSettings;

        private ProgressBar progressBar;
        private Label lblStatus;

        private Button btnCancel;
        private Button btnUninstall;

        // Cores do Tema Dark
        private readonly Color ClrBgWindow     = Color.FromArgb(12, 13, 17);
        private readonly Color ClrCardBg       = Color.FromArgb(19, 22, 30);
        private readonly Color ClrCardBorder   = Color.FromArgb(34, 39, 53);
        private readonly Color ClrAccentRed    = Color.FromArgb(255, 60, 60);
        private readonly Color ClrTextPrimary  = Color.FromArgb(240, 243, 250);
        private readonly Color ClrTextMuted    = Color.FromArgb(140, 148, 168);

        public UninstallerForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Desinstalador - OpenClevoRGB";
            this.Size = new Size(620, 480);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = ClrBgWindow;
            this.ForeColor = ClrTextPrimary;
            this.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            this.Icon = CreateAppIcon();

            // 1. Cabeçalho
            headerPanel = new Panel();
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 85;
            headerPanel.BackColor = Color.FromArgb(16, 18, 25);
            headerPanel.Paint += (s, e) => {
                using (Pen borderPen = new Pen(ClrCardBorder, 1))
                {
                    e.Graphics.DrawLine(borderPen, 0, headerPanel.Height - 1, headerPanel.Width, headerPanel.Height - 1);
                }
            };

            lblTitle = new Label();
            lblTitle.Text = "DESINSTALAR • OPENCLEVO RGB";
            lblTitle.Font = new Font("Segoe UI", 13.5f, FontStyle.Bold);
            lblTitle.ForeColor = ClrAccentRed;
            lblTitle.Location = new Point(25, 16);
            lblTitle.AutoSize = true;

            lblSubtitle = new Label();
            lblSubtitle.Text = "Remoção limpa do software e de todos os componentes do sistema";
            lblSubtitle.Font = new Font("Segoe UI", 9f);
            lblSubtitle.ForeColor = ClrTextMuted;
            lblSubtitle.Location = new Point(26, 44);
            lblSubtitle.AutoSize = true;

            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(lblSubtitle);
            this.Controls.Add(headerPanel);

            int startY = 100;

            // 2. Card de Informações
            cardInfo = CreateCardPanel(25, startY, 555, 175);

            lblInfoTitle = new Label();
            lblInfoTitle.Text = "VOCÊ ESTÁ PRESTES A DESINSTALAR O UTILITÁRIO";
            lblInfoTitle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            lblInfoTitle.ForeColor = ClrTextPrimary;
            lblInfoTitle.Location = new Point(15, 14);
            lblInfoTitle.AutoSize = true;
            cardInfo.Controls.Add(lblInfoTitle);

            lblItemsList = new Label();
            lblItemsList.Text = 
                "Os seguintes itens serão removidos do seu computador:\n\n" +
                "  • Arquivos executáveis e bibliotecas da pasta de instalação\n" +
                "  • Atalhos da Área de Trabalho e do Menu Iniciar\n" +
                "  • Tarefa do Agendador do Windows (inicialização automática no boot)\n" +
                "  • Registros de instalação do Windows";
            lblItemsList.Font = new Font("Segoe UI", 9f);
            lblItemsList.ForeColor = ClrTextMuted;
            lblItemsList.Location = new Point(15, 42);
            lblItemsList.Size = new Size(520, 85);
            cardInfo.Controls.Add(lblItemsList);

            chkRemoveSettings = new CheckBox();
            chkRemoveSettings.Text = "Remover também as preferências e histórico de cores salvas (%AppData%)";
            chkRemoveSettings.ForeColor = ClrTextPrimary;
            chkRemoveSettings.Font = new Font("Segoe UI", 8.5f);
            chkRemoveSettings.Location = new Point(18, 138);
            chkRemoveSettings.AutoSize = true;
            chkRemoveSettings.Cursor = Cursors.Hand;
            cardInfo.Controls.Add(chkRemoveSettings);

            this.Controls.Add(cardInfo);

            startY += 190;

            // 3. Barra de Progresso e Status
            lblStatus = new Label();
            lblStatus.Text = "Clique em 'Desinstalar Agora' para confirmar a remoção.";
            lblStatus.Font = new Font("Segoe UI", 9f);
            lblStatus.ForeColor = ClrTextMuted;
            lblStatus.Location = new Point(25, startY);
            lblStatus.Size = new Size(555, 20);
            this.Controls.Add(lblStatus);

            progressBar = new ProgressBar();
            progressBar.Location = new Point(25, startY + 24);
            progressBar.Size = new Size(555, 14);
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.Value = 0;
            this.Controls.Add(progressBar);

            startY += 48;

            // 4. Botões de Ação
            btnCancel = new Button();
            btnCancel.Text = "Cancelar";
            btnCancel.Location = new Point(305, startY);
            btnCancel.Size = new Size(125, 36);
            btnCancel.BackColor = Color.FromArgb(32, 36, 48);
            btnCancel.ForeColor = ClrTextPrimary;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.FlatAppearance.BorderColor = ClrCardBorder;
            btnCancel.Cursor = Cursors.Hand;
            btnCancel.Click += (s, e) => this.Close();
            this.Controls.Add(btnCancel);

            btnUninstall = new Button();
            btnUninstall.Text = "🗑️ Desinstalar";
            btnUninstall.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            btnUninstall.Location = new Point(440, startY);
            btnUninstall.Size = new Size(140, 36);
            btnUninstall.BackColor = Color.FromArgb(200, 35, 35);
            btnUninstall.ForeColor = Color.White;
            btnUninstall.FlatStyle = FlatStyle.Flat;
            btnUninstall.FlatAppearance.BorderSize = 0;
            btnUninstall.Cursor = Cursors.Hand;
            btnUninstall.Click += BtnUninstall_Click;
            this.Controls.Add(btnUninstall);
        }

        private Panel CreateCardPanel(int x, int y, int width, int height)
        {
            Panel p = new Panel();
            p.Location = new Point(x, y);
            p.Size = new Size(width, height);
            p.BackColor = ClrCardBg;
            p.Paint += (s, e) => {
                using (Pen borderPen = new Pen(ClrCardBorder, 1))
                {
                    e.Graphics.DrawRectangle(borderPen, 0, 0, p.Width - 1, p.Height - 1);
                }
            };
            return p;
        }

        private void BtnUninstall_Click(object sender, EventArgs e)
        {
            DialogResult confirm = MessageBox.Show(
                "Tem certeza de que deseja remover o OpenClevoRGB do seu computador?",
                "Confirmar Desinstalação",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirm != DialogResult.Yes) return;

            btnUninstall.Enabled = false;
            btnCancel.Enabled = false;
            chkRemoveSettings.Enabled = false;

            Thread worker = new Thread(PerformUninstallation);
            worker.IsBackground = true;
            worker.Start();
        }

        private void SetStatus(string text, int progress)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => SetStatus(text, progress)));
                return;
            }

            lblStatus.Text = text;
            progressBar.Value = Math.Max(0, Math.Min(100, progress));
        }

        private void PerformUninstallation()
        {
            try
            {
                // 1. Finaliza qualquer processo aberto
                SetStatus("Fechando processos do aplicativo...", 15);
                try
                {
                    ProcessStartInfo killPsi = new ProcessStartInfo("taskkill.exe", "/F /IM OpenClevoRGB.exe");
                    killPsi.CreateNoWindow = true;
                    killPsi.UseShellExecute = false;
                    using (Process p = Process.Start(killPsi))
                    {
                        p.WaitForExit(3000);
                    }
                }
                catch {}
                try
                {
                    ProcessStartInfo killLegacy = new ProcessStartInfo("taskkill.exe", "/F /IM AvellRGBMaster.exe");
                    killLegacy.CreateNoWindow = true;
                    killLegacy.UseShellExecute = false;
                    using (Process p = Process.Start(killLegacy))
                    {
                        p.WaitForExit(3000);
                    }
                }
                catch {}
                Thread.Sleep(300);

                // 2. Remove a tarefa do Agendador do Windows
                SetStatus("Removendo tarefa agendada no Windows...", 35);
                try
                {
                    ProcessStartInfo taskPsi = new ProcessStartInfo("schtasks.exe", "/delete /tn \"OpenClevoRGB\" /f");
                    taskPsi.CreateNoWindow = true;
                    taskPsi.UseShellExecute = false;
                    using (Process p = Process.Start(taskPsi))
                    {
                        p.WaitForExit(3000);
                    }
                }
                catch {}
                try
                {
                    ProcessStartInfo taskLegacy = new ProcessStartInfo("schtasks.exe", "/delete /tn \"AvellRGBKeyboardMaster\" /f");
                    taskLegacy.CreateNoWindow = true;
                    taskLegacy.UseShellExecute = false;
                    using (Process p = Process.Start(taskLegacy))
                    {
                        p.WaitForExit(3000);
                    }
                }
                catch {}

                // 3. Remove atalhos
                SetStatus("Removendo atalhos do Desktop e Menu Iniciar...", 55);
                try
                {
                    string deskUser = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "OpenClevoRGB.lnk");
                    string deskCommon = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory), "OpenClevoRGB.lnk");
                    if (File.Exists(deskUser)) File.Delete(deskUser);
                    if (File.Exists(deskCommon)) File.Delete(deskCommon);

                    string deskOld = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Avell RGB Keyboard Master.lnk");
                    if (File.Exists(deskOld)) File.Delete(deskOld);

                    string startMenu = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms), "OpenClevoRGB");
                    if (Directory.Exists(startMenu))
                    {
                        Directory.Delete(startMenu, true);
                    }

                    string startMenuOld = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms), "Avell RGB Keyboard Master");
                    if (Directory.Exists(startMenuOld))
                    {
                        Directory.Delete(startMenuOld, true);
                    }
                }
                catch {}

                // 4. Remove chave de registro de desinstalação
                SetStatus("Limpando registros do Windows...", 75);
                try
                {
                    Registry.LocalMachine.DeleteSubKeyTree(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\OpenClevoRGB", false);
                }
                catch {}
                try
                {
                    Registry.LocalMachine.DeleteSubKeyTree(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\AvellRGBKeyboardMaster", false);
                }
                catch {}

                // 5. Remove preferências salvas se solicitado
                if (chkRemoveSettings.Checked)
                {
                    SetStatus("Removendo histórico e preferências salvas...", 85);
                    try
                    {
                        string appDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OpenClevoRGB");
                        if (Directory.Exists(appDataDir))
                        {
                            Directory.Delete(appDataDir, true);
                        }

                        string appDataDirOld = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AvellKeyboardRGB");
                        if (Directory.Exists(appDataDirOld))
                        {
                            Directory.Delete(appDataDirOld, true);
                        }
                    }
                    catch {}
                }

                // 6. Agenda remoção da pasta do aplicativo após fechar
                SetStatus("Finalizando desinstalação...", 100);
                string installDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\', '/');

                Thread.Sleep(400);

                this.Invoke(new Action(() => {
                    MessageBox.Show(
                        "O OpenClevoRGB foi desinstalado com sucesso do seu computador.",
                        "Desinstalação Concluída",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );

                    // Comando em lote para apagar os arquivos restantes e o próprio Desinstalador após encerrar o processo
                    ProcessStartInfo cleanup = new ProcessStartInfo();
                    cleanup.FileName = "cmd.exe";
                    cleanup.Arguments = string.Format("/c timeout /t 2 /nobreak >nul & rmdir /s /q \"{0}\"", installDir);
                    cleanup.CreateNoWindow = true;
                    cleanup.UseShellExecute = false;
                    Process.Start(cleanup);

                    Application.Exit();
                }));
            }
            catch (Exception ex)
            {
                this.Invoke(new Action(() => {
                    MessageBox.Show(
                        "Ocorreu um erro durante a desinstalação:\n\n" + ex.Message,
                        "Erro na Desinstalação",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    btnUninstall.Enabled = true;
                    btnCancel.Enabled = true;
                }));
            }
        }

        private Icon CreateAppIcon()
        {
            using (Bitmap bmp = new Bitmap(32, 32))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                using (Brush brush = new SolidBrush(Color.FromArgb(24, 26, 36)))
                {
                    g.FillRectangle(brush, 2, 6, 28, 20);
                }

                // Ícone em tom avermelhado de desinstalação
                using (Brush b1 = new SolidBrush(ClrAccentRed))
                {
                    g.FillRectangle(b1, 5, 10, 22, 12);
                }

                using (Pen pen = new Pen(Color.FromArgb(80, 40, 40), 1))
                {
                    g.DrawRectangle(pen, 2, 6, 28, 20);
                }

                IntPtr hIcon = bmp.GetHicon();
                return Icon.FromHandle(hIcon);
            }
        }
    }
}

