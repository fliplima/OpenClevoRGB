using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace OpenClevoRGBInstaller
{
    public class InstallerForm : Form
    {
        private Panel headerPanel;
        private Label lblTitle;
        private Label lblSubtitle;

        private Panel cardFolder;
        private Label lblFolderTitle;
        private TextBox txtInstallPath;
        private Button btnBrowse;

        private Panel cardOptions;
        private Label lblOptionsTitle;
        private CheckBox chkDesktopShortcut;
        private CheckBox chkStartMenuShortcut;
        private CheckBox chkAutoStart;
        private CheckBox chkRegisterDriver;

        private ProgressBar progressBar;
        private Label lblStatus;

        private Button btnCancel;
        private Button btnInstall;

        // Controle de Modo de Atualização (In-Place Update)
        private bool isUpdateMode = false;
        private string detectedExistingVersion = string.Empty;
        private string detectedExistingPath = string.Empty;

        // Cores Dark Theme Moderno
        private readonly Color ClrBgWindow     = Color.FromArgb(12, 13, 17);
        private readonly Color ClrCardBg       = Color.FromArgb(19, 22, 30);
        private readonly Color ClrCardBorder   = Color.FromArgb(34, 39, 53);
        private readonly Color ClrAccentCyan   = Color.FromArgb(0, 229, 255);
        private readonly Color ClrAccentGreen  = Color.FromArgb(0, 230, 118);
        private readonly Color ClrTextPrimary  = Color.FromArgb(240, 243, 250);
        private readonly Color ClrTextMuted    = Color.FromArgb(140, 148, 168);

        public InstallerForm()
        {
            DetectExistingInstallation();
            InitializeComponent();
            ApplyModeVisuals();
        }

        private void DetectExistingInstallation()
        {
            try
            {
                // 1. Tenta verificar chave de registro oficial do OpenClevoRGB
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\OpenClevoRGB", false))
                {
                    if (key != null)
                    {
                        object loc = key.GetValue("InstallLocation");
                        object ver = key.GetValue("DisplayVersion");
                        if (loc != null && !string.IsNullOrEmpty(loc.ToString()))
                        {
                            detectedExistingPath = loc.ToString();
                            detectedExistingVersion = ver != null ? ver.ToString() : "1.x";
                            isUpdateMode = true;
                            return;
                        }
                    }
                }

                // 2. Tenta verificar se a pasta padrão em Arquivos de Programas já tem o executável
                string defaultPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    "OpenClevoRGB"
                );
                string exePath = Path.Combine(defaultPath, "OpenClevoRGB.exe");
                if (File.Exists(exePath))
                {
                    detectedExistingPath = defaultPath;
                    try
                    {
                        FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(exePath);
                        detectedExistingVersion = fvi.ProductVersion;
                    }
                    catch
                    {
                        detectedExistingVersion = "1.x";
                    }
                    isUpdateMode = true;
                    return;
                }

                // 3. Fallback: Verifica versão legada AvellRGBKeyboardMaster
                using (RegistryKey oldKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\AvellRGBKeyboardMaster", false))
                {
                    if (oldKey != null)
                    {
                        object loc = oldKey.GetValue("InstallLocation");
                        object ver = oldKey.GetValue("DisplayVersion");
                        if (loc != null && !string.IsNullOrEmpty(loc.ToString()))
                        {
                            detectedExistingPath = defaultPath;
                            detectedExistingVersion = ver != null ? ver.ToString() : "1.2.0";
                            isUpdateMode = true;
                        }
                    }
                }
            }
            catch {}
        }

        private void ApplyModeVisuals()
        {
            if (isUpdateMode)
            {
                this.Text = "Atualizador - OpenClevoRGB";
                lblTitle.Text = "ATUALIZAR • OPENCLEVO RGB";
                lblTitle.ForeColor = ClrAccentGreen;
                lblSubtitle.Text = string.Format("Instalação detectada (v{0} ➔ v1.4.0). Suas preferências e cores serão mantidas!", detectedExistingVersion);
                lblSubtitle.ForeColor = ClrAccentGreen;

                if (!string.IsNullOrEmpty(detectedExistingPath))
                {
                    txtInstallPath.Text = detectedExistingPath;
                }

                btnInstall.Text = "🔄 Atualizar Agora";
                btnInstall.BackColor = Color.FromArgb(0, 180, 90);
                lblStatus.Text = string.Format("Pronto para atualizar da v{0} para a v1.4.0.", detectedExistingVersion);
            }
        }

        private void InitializeComponent()
        {
            this.Text = "Instalador - OpenClevoRGB";
            this.Size = new Size(680, 560);
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
            lblTitle.Text = "INSTALADOR • OPENCLEVO RGB";
            lblTitle.Font = new Font("Segoe UI", 13.5f, FontStyle.Bold);
            lblTitle.ForeColor = ClrAccentCyan;
            lblTitle.Location = new Point(25, 16);
            lblTitle.AutoSize = true;

            lblSubtitle = new Label();
            lblSubtitle.Text = "Assistente de Instalação e Inicialização Automática com o Windows";
            lblSubtitle.Font = new Font("Segoe UI", 9f);
            lblSubtitle.ForeColor = ClrTextMuted;
            lblSubtitle.Location = new Point(26, 44);
            lblSubtitle.AutoSize = true;

            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(lblSubtitle);
            this.Controls.Add(headerPanel);

            int startY = 100;

            // 2. Card de Pasta de Instalação
            cardFolder = CreateCardPanel(25, startY, 615, 105);

            lblFolderTitle = new Label();
            lblFolderTitle.Text = "1. PASTA DE INSTALAÇÃO";
            lblFolderTitle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            lblFolderTitle.ForeColor = ClrTextMuted;
            lblFolderTitle.Location = new Point(15, 12);
            lblFolderTitle.AutoSize = true;
            cardFolder.Controls.Add(lblFolderTitle);

            string defaultPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "OpenClevoRGB"
            );

            txtInstallPath = new TextBox();
            txtInstallPath.Text = defaultPath;
            txtInstallPath.Location = new Point(15, 45);
            txtInstallPath.Size = new Size(470, 26);
            txtInstallPath.BackColor = Color.FromArgb(28, 32, 44);
            txtInstallPath.ForeColor = Color.White;
            txtInstallPath.BorderStyle = BorderStyle.FixedSingle;
            txtInstallPath.Font = new Font("Segoe UI", 9.5f);
            cardFolder.Controls.Add(txtInstallPath);

            btnBrowse = new Button();
            btnBrowse.Text = "Procurar...";
            btnBrowse.Location = new Point(495, 43);
            btnBrowse.Size = new Size(105, 30);
            btnBrowse.BackColor = Color.FromArgb(32, 38, 52);
            btnBrowse.ForeColor = ClrAccentCyan;
            btnBrowse.FlatStyle = FlatStyle.Flat;
            btnBrowse.FlatAppearance.BorderColor = ClrAccentCyan;
            btnBrowse.Cursor = Cursors.Hand;
            btnBrowse.Click += BtnBrowse_Click;
            cardFolder.Controls.Add(btnBrowse);

            this.Controls.Add(cardFolder);

            startY += 120;

            // 3. Card de Opções
            cardOptions = CreateCardPanel(25, startY, 615, 160);

            lblOptionsTitle = new Label();
            lblOptionsTitle.Text = "2. OPÇÕES DE CONFIGURAÇÃO";
            lblOptionsTitle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            lblOptionsTitle.ForeColor = ClrTextMuted;
            lblOptionsTitle.Location = new Point(15, 12);
            lblOptionsTitle.AutoSize = true;
            cardOptions.Controls.Add(lblOptionsTitle);

            chkDesktopShortcut = CreateCheckBox("Criar atalho na Área de Trabalho (Desktop)", 15, 40, true);
            chkStartMenuShortcut = CreateCheckBox("Criar atalho no Menu Iniciar", 15, 68, true);
            chkAutoStart = CreateCheckBox("Iniciar automaticamente com o Windows (via Agendador com Admin)", 15, 96, true);
            chkRegisterDriver = CreateCheckBox("Registrar metadados de driver ACPI Clevo (clevomof.dll)", 15, 124, true);

            cardOptions.Controls.Add(chkDesktopShortcut);
            cardOptions.Controls.Add(chkStartMenuShortcut);
            cardOptions.Controls.Add(chkAutoStart);
            cardOptions.Controls.Add(chkRegisterDriver);

            this.Controls.Add(cardOptions);

            startY += 175;

            // 4. Barra de Progresso e Status
            lblStatus = new Label();
            lblStatus.Text = "Pronto para iniciar a instalação.";
            lblStatus.Font = new Font("Segoe UI", 9f);
            lblStatus.ForeColor = ClrTextMuted;
            lblStatus.Location = new Point(25, startY);
            lblStatus.Size = new Size(615, 20);
            this.Controls.Add(lblStatus);

            progressBar = new ProgressBar();
            progressBar.Location = new Point(25, startY + 24);
            progressBar.Size = new Size(615, 14);
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.Value = 0;
            this.Controls.Add(progressBar);

            startY += 48;

            // 5. Botões de Ação
            btnCancel = new Button();
            btnCancel.Text = "Cancelar";
            btnCancel.Location = new Point(365, startY);
            btnCancel.Size = new Size(130, 36);
            btnCancel.BackColor = Color.FromArgb(32, 36, 48);
            btnCancel.ForeColor = ClrTextPrimary;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.FlatAppearance.BorderColor = ClrCardBorder;
            btnCancel.Cursor = Cursors.Hand;
            btnCancel.Click += (s, e) => this.Close();
            this.Controls.Add(btnCancel);

            btnInstall = new Button();
            btnInstall.Text = "🚀 Instalar Agora";
            btnInstall.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            btnInstall.Location = new Point(505, startY);
            btnInstall.Size = new Size(135, 36);
            btnInstall.BackColor = Color.FromArgb(0, 160, 230);
            btnInstall.ForeColor = Color.White;
            btnInstall.FlatStyle = FlatStyle.Flat;
            btnInstall.FlatAppearance.BorderSize = 0;
            btnInstall.Cursor = Cursors.Hand;
            btnInstall.Click += BtnInstall_Click;
            this.Controls.Add(btnInstall);
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

        private CheckBox CreateCheckBox(string text, int x, int y, bool isChecked)
        {
            CheckBox chk = new CheckBox();
            chk.Text = text;
            chk.Location = new Point(x, y);
            chk.AutoSize = true;
            chk.Checked = isChecked;
            chk.ForeColor = ClrTextPrimary;
            chk.Font = new Font("Segoe UI", 9f);
            chk.Cursor = Cursors.Hand;
            return chk;
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Selecione a pasta onde o OpenClevoRGB será instalado:";
                fbd.SelectedPath = txtInstallPath.Text;
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtInstallPath.Text = fbd.SelectedPath;
                }
            }
        }

        private void BtnInstall_Click(object sender, EventArgs e)
        {
            string targetDir = txtInstallPath.Text.Trim();
            if (string.IsNullOrEmpty(targetDir))
            {
                MessageBox.Show("Por favor, selecione uma pasta de instalação válida.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnInstall.Enabled = false;
            btnBrowse.Enabled = false;
            txtInstallPath.Enabled = false;
            btnCancel.Enabled = false;

            Thread worker = new Thread(() => PerformInstallation(targetDir));
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

        private void KillRunningProcesses()
        {
            string[] procsToKill = new string[] { "OpenClevoRGB", "AvellRGBMaster" };
            foreach (string procName in procsToKill)
            {
                try
                {
                    Process[] procs = Process.GetProcessesByName(procName);
                    if (procs != null && procs.Length > 0)
                    {
                        foreach (Process p in procs)
                        {
                            try { p.Kill(); } catch {}
                        }
                    }
                }
                catch {}

                try
                {
                    ProcessStartInfo killPsi = new ProcessStartInfo("taskkill.exe", string.Format("/F /IM {0}.exe", procName));
                    killPsi.CreateNoWindow = true;
                    killPsi.UseShellExecute = false;
                    using (Process p = Process.Start(killPsi))
                    {
                        p.WaitForExit(3000);
                    }
                }
                catch {}
            }

            // Aguarda o encerramento completo para o Windows liberar os arquivos do disco
            for (int i = 0; i < 10; i++)
            {
                Process[] remaining = Process.GetProcessesByName("OpenClevoRGB");
                if (remaining == null || remaining.Length == 0) break;
                Thread.Sleep(200);
            }
            Thread.Sleep(300);
        }

        private void PerformInstallation(string targetDir)
        {
            try
            {
                // Passo 1: Finalizar instâncias anteriores para permitir a atualização
                SetStatus(isUpdateMode ? "Encerrando versão em execução para atualizar..." : "Fechando processos em execução...", 10);
                KillRunningProcesses();

                // Passo 2: Criar diretório de destino
                SetStatus(isUpdateMode ? "Preparando pasta do programa..." : "Criando diretório de instalação...", 25);
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                // Passo 3: Copiar/Extrair arquivos (100% independente com recursos embutidos)
                SetStatus(isUpdateMode ? "Atualizando binários para a v1.4.0..." : "Instalando arquivos do programa...", 45);
                string targetExe = Path.Combine(targetDir, "OpenClevoRGB.exe");
                string targetDll = Path.Combine(targetDir, "clevomof.dll");
                string targetUninstaller = Path.Combine(targetDir, "Desinstalador.exe");

                InstallFile("OpenClevoRGB.exe", targetExe);
                InstallFile("clevomof.dll", targetDll);
                InstallFile("Desinstalador.exe", targetUninstaller);

                // Passo 4: Registrar Driver ACPI Clevo
                if (chkRegisterDriver.Checked)
                {
                    SetStatus("Verificando metadados do driver ACPI...", 65);
                    RegisterAcpiMetadata(targetDll);
                }

                // Passo 5: Criar / Atualizar Atalhos
                SetStatus(isUpdateMode ? "Atualizando atalhos do sistema..." : "Criando atalhos...", 80);
                if (chkDesktopShortcut.Checked)
                {
                    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    string lnkDesktop = Path.Combine(desktopPath, "OpenClevoRGB.lnk");
                    CreateShortcut(lnkDesktop, targetExe, "Controle de Iluminação RGB para laptops Clevo / Avell");
                }

                if (chkStartMenuShortcut.Checked)
                {
                    string startMenu = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms), "OpenClevoRGB");
                    if (!Directory.Exists(startMenu)) Directory.CreateDirectory(startMenu);
                    
                    string lnkMenu = Path.Combine(startMenu, "OpenClevoRGB.lnk");
                    CreateShortcut(lnkMenu, targetExe, "Controle de Iluminação RGB para laptops Clevo / Avell");

                    string lnkUninstall = Path.Combine(startMenu, "Desinstalar OpenClevoRGB.lnk");
                    CreateShortcut(lnkUninstall, targetUninstaller, "Desinstalar o OpenClevoRGB");
                }

                // Passo 6: Configurar Inicialização com o Windows (Task Scheduler com Admin)
                if (chkAutoStart.Checked)
                {
                    SetStatus(isUpdateMode ? "Atualizando inicialização automática no Windows..." : "Configurando inicialização automática...", 90);
                    ConfigureAutoStartTask(targetExe, true);
                }

                // Passo 7: Criar Desinstalador e Registro no Windows
                SetStatus(isUpdateMode ? "Atualizando registros de versão no Windows..." : "Criando registros de desinstalação...", 95);
                CreateUninstallEntry(targetDir, targetExe);

                SetStatus(isUpdateMode ? "Atualização concluída com sucesso!" : "Instalação concluída com sucesso!", 100);
                Thread.Sleep(500);

                this.Invoke(new Action(() => {
                    string msgTitle = isUpdateMode ? "Atualização Concluída" : "Instalação Concluída";
                    string msgText = isUpdateMode
                        ? "O OpenClevoRGB foi atualizado com sucesso para a versão 1.4.0!\n\n" +
                          "Todas as suas configurações salvas, atalhos de teclado e histórico de cores foram preservados.\n\n" +
                          "Deseja abrir o aplicativo atualizado agora?"
                        : "O OpenClevoRGB foi instalado com sucesso!\n\n" +
                          "Deseja abrir o aplicativo agora?";

                    DialogResult res = MessageBox.Show(
                        msgText,
                        msgTitle,
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information
                    );

                    if (res == DialogResult.Yes)
                    {
                        ProcessStartInfo psi = new ProcessStartInfo();
                        psi.FileName = targetExe;
                        psi.Verb = "runas";
                        Process.Start(psi);
                    }

                    Application.Exit();
                }));
            }
            catch (Exception ex)
            {
                this.Invoke(new Action(() => {
                    MessageBox.Show(
                        "Ocorreu um erro durante o processo:\n\n" + ex.Message,
                        "Erro na Operação",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    btnInstall.Enabled = true;
                    btnBrowse.Enabled = true;
                    txtInstallPath.Enabled = true;
                    btnCancel.Enabled = true;
                    SetStatus("Operação cancelada devido a erro.", 0);
                }));
            }
        }

        private void InstallFile(string fileName, string targetPath)
        {
            int maxRetries = 5;
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    // 1. Tenta extrair do recurso embutido (100% autossuficiente para binário único do GitHub)
                    if (ExtractEmbeddedFile(fileName, targetPath))
                    {
                        return;
                    }

                    // 2. Fallback: Procura na mesma pasta do executável
                    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    string localPath = Path.Combine(baseDir, fileName);
                    if (File.Exists(localPath))
                    {
                        File.Copy(localPath, targetPath, true);
                        return;
                    }

                    // 3. Fallback: Procura na estrutura de desenvolvimento (resources/ ou bin/)
                    string resPath = Path.Combine(baseDir, "..", "resources", fileName);
                    if (File.Exists(resPath))
                    {
                        File.Copy(resPath, targetPath, true);
                        return;
                    }

                    string binPath = Path.Combine(baseDir, "..", "bin", fileName);
                    if (File.Exists(binPath))
                    {
                        File.Copy(binPath, targetPath, true);
                        return;
                    }

                    throw new FileNotFoundException("Arquivo não encontrado embutido nem localmente: " + fileName);
                }
                catch (IOException ioEx)
                {
                    if (attempt == maxRetries) throw ioEx;
                    Thread.Sleep(400); // Aguarda o Windows liberar o lock do arquivo
                }
            }
        }

        private bool ExtractEmbeddedFile(string resourceName, string targetPath)
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                string fullName = null;
                foreach (string name in assembly.GetManifestResourceNames())
                {
                    if (name.EndsWith(resourceName, StringComparison.OrdinalIgnoreCase))
                    {
                        fullName = name;
                        break;
                    }
                }

                if (fullName != null)
                {
                    using (Stream stream = assembly.GetManifestResourceStream(fullName))
                    {
                        if (stream != null && stream.Length > 0)
                        {
                            using (FileStream fs = new FileStream(targetPath, FileMode.Create, FileAccess.Write))
                            {
                                stream.CopyTo(fs);
                            }
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Falha ao extrair recurso embutido: " + ex.Message);
            }
            return false;
        }

        private void RegisterAcpiMetadata(string dllPath)
        {
            try
            {
                string winDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                string syswow64 = Path.Combine(winDir, "SysWOW64");
                string system32 = Path.Combine(winDir, "System32");

                if (Directory.Exists(syswow64))
                {
                    File.Copy(dllPath, Path.Combine(syswow64, "clevomof.dll"), true);
                }
                File.Copy(dllPath, Path.Combine(system32, "clevomof.dll"), true);

                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\WmiAcpi", true))
                {
                    if (key != null)
                    {
                        key.SetValue("MofImagePath", "syswow64\\clevomof.dll", RegistryValueKind.String);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Erro ao registrar metadados: " + ex.Message);
            }
        }

        private void ConfigureAutoStartTask(string exePath, bool enable)
        {
            try
            {
                string taskName = "OpenClevoRGB";
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "schtasks.exe";
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;

                if (enable)
                {
                    // Inicia no logon com os maiores privilégios (Admin), sem prompt do UAC!
                    psi.Arguments = string.Format("/create /tn \"{0}\" /tr \"\\\"{1}\\\" -silent\" /sc onlogon /rl highest /f", taskName, exePath);
                }
                else
                {
                    psi.Arguments = string.Format("/delete /tn \"{0}\" /f", taskName);
                }

                using (Process proc = Process.Start(psi))
                {
                    proc.WaitForExit(4000);
                }
            }
            catch {}
        }

        private void CreateShortcut(string shortcutPath, string targetPath, string description)
        {
            try
            {
                Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType == null) return;

                object shell = Activator.CreateInstance(shellType);
                object shortcut = shellType.InvokeMember("CreateShortcut", BindingFlags.InvokeMethod, null, shell, new object[] { shortcutPath });
                Type shortcutType = shortcut.GetType();

                shortcutType.InvokeMember("TargetPath", BindingFlags.SetProperty, null, shortcut, new object[] { targetPath });
                shortcutType.InvokeMember("WorkingDirectory", BindingFlags.SetProperty, null, shortcut, new object[] { Path.GetDirectoryName(targetPath) });
                shortcutType.InvokeMember("Description", BindingFlags.SetProperty, null, shortcut, new object[] { description });
                shortcutType.InvokeMember("Save", BindingFlags.InvokeMethod, null, shortcut, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Erro ao criar atalho: " + ex.Message);
            }
        }

        private void CreateUninstallEntry(string installDir, string exePath)
        {
            try
            {
                // Cria script Desinstalar.bat
                string uninstallerBat = Path.Combine(installDir, "Desinstalar.bat");
                string batContent = 
                    "@echo off\r\n" +
                    "title Desinstalando OpenClevoRGB\r\n" +
                    "echo Desinstalando...\r\n" +
                    "taskkill /F /IM OpenClevoRGB.exe >nul 2>&1\r\n" +
                    "schtasks /delete /tn \"OpenClevoRGB\" /f >nul 2>&1\r\n" +
                    "del /f /q \"" + Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "OpenClevoRGB.lnk") + "\" >nul 2>&1\r\n" +
                    "rmdir /s /q \"" + Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms), "OpenClevoRGB") + "\" >nul 2>&1\r\n" +
                    "reg delete \"HKLM\\Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\OpenClevoRGB\" /f >nul 2>&1\r\n" +
                    "echo Removendo arquivos...\r\n" +
                    "timeout /t 2 /nobreak >nul\r\n" +
                    "rmdir /s /q \"" + installDir + "\" >nul 2>&1\r\n" +
                    "echo Desinstalacao concluida!\r\n" +
                    "pause\r\n";

                File.WriteAllText(uninstallerBat, batContent);

                string uninstallerExe = Path.Combine(installDir, "Desinstalador.exe");
                string uninstallerCmd = File.Exists(uninstallerExe) ? ("\"" + uninstallerExe + "\"") : ("\"" + uninstallerBat + "\"");

                // Registro no Windows (Painel de Controle -> Programas e Recursos / Configurações)
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\OpenClevoRGB"))
                {
                    if (key != null)
                    {
                        key.SetValue("DisplayName", "OpenClevoRGB", RegistryValueKind.String);
                        key.SetValue("DisplayIcon", File.Exists(uninstallerExe) ? uninstallerExe : exePath, RegistryValueKind.String);
                        key.SetValue("DisplayVersion", "1.4.0", RegistryValueKind.String);
                        key.SetValue("Publisher", "OpenClevoRGB / Felipe Lima", RegistryValueKind.String);
                        key.SetValue("InstallLocation", installDir, RegistryValueKind.String);
                        key.SetValue("UninstallString", uninstallerCmd, RegistryValueKind.String);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Erro ao criar desinstalador: " + ex.Message);
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

                using (Brush b1 = new SolidBrush(ClrAccentCyan))
                using (Brush b2 = new SolidBrush(ClrAccentGreen))
                using (Brush b3 = new SolidBrush(Color.FromArgb(170, 0, 255)))
                {
                    g.FillRectangle(b1, 5, 10, 6, 12);
                    g.FillRectangle(b2, 13, 10, 6, 12);
                    g.FillRectangle(b3, 21, 10, 6, 12);
                }

                using (Pen pen = new Pen(Color.FromArgb(60, 65, 85), 1))
                {
                    g.DrawRectangle(pen, 2, 6, 28, 20);
                }

                IntPtr hIcon = bmp.GetHicon();
                return Icon.FromHandle(hIcon);
            }
        }
    }
}

