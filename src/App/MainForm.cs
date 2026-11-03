using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace OpenClevoRGB
{
    public class MainForm : Form
    {
        private ClevoController controller;
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;
        private ToolTip toolTip;

        // Cabeçalho
        private Panel headerPanel;
        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblStatusDot;
        private Label lblStatusText;
        private Button btnInstallDriver;

        // Seção 1: Zonas
        private Panel cardZones;
        private Panel pnlZoneLeft;
        private Panel pnlZoneMid;
        private Panel pnlZoneRight;
        private Label lblZoneLeft;
        private Label lblZoneMid;
        private Label lblZoneRight;
        private CheckBox chkSyncZones;

        // Seção 2: Paleta
        private Panel cardColors;
        private FlowLayoutPanel pnlPresets;
        private Button btnCustomColor;

        // Seção 3: Brilho
        private Panel cardBrightness;
        private TrackBar trackBrightness;
        private Label lblBrightnessValue;
        private Button btnBright0;
        private Button btnBright25;
        private Button btnBright50;
        private Button btnBright75;
        private Button btnBright100;

        // Seção 4: Efeitos & Ações
        private Panel cardEffects;
        private ComboBox cmbEffects;
        private Button btnTogglePower;
        private Button btnApply;

        // Seção 5: Atalhos de Teclado (Hotkeys)
        private Panel cardHotkeys;
        private CheckBox chkEnableHotkeys;
        private Label lblHotkeysSummary;
        private Label lblHotkeysSubtext;
        private Button btnConfigureHotkeys;
        private HotkeyManager hotkeyManager;

        // Seção 6: Rodapé / Opções
        private Panel footerPanel;
        private CheckBox chkStartWithWindows;
        private CheckBox chkMinimizeToTray;

        private KeyboardZone selectedZone = KeyboardZone.Left;
        private bool isLightOn = true;

        // Paleta Dark Theme Premium
        private readonly Color ClrBgWindow     = Color.FromArgb(12, 13, 17);
        private readonly Color ClrCardBg       = Color.FromArgb(19, 22, 30);
        private readonly Color ClrCardBorder   = Color.FromArgb(34, 39, 53);
        private readonly Color ClrAccentCyan   = Color.FromArgb(0, 229, 255);
        private readonly Color ClrAccentGreen  = Color.FromArgb(0, 230, 118);
        private readonly Color ClrTextPrimary  = Color.FromArgb(240, 243, 250);
        private readonly Color ClrTextMuted    = Color.FromArgb(140, 148, 168);

        public MainForm()
        {
            controller = new ClevoController();
            AppSettings.Load();

            // Configuração do ToolTip estilo Dark
            toolTip = new ToolTip();
            toolTip.AutoPopDelay = 6000;
            toolTip.InitialDelay = 120;
            toolTip.ReshowDelay = 80;
            toolTip.ShowAlways = true;
            toolTip.BackColor = Color.FromArgb(24, 28, 38);
            toolTip.ForeColor = Color.FromArgb(0, 229, 255);

            InitializeComponent();
            SetupTrayIcon();

            RefreshDriverStatus();
            ApplySavedSettings();

            // Inicializa gerenciador de atalhos globais e eventos ACPI
            hotkeyManager = new HotkeyManager();
            hotkeyManager.OnCycleColor += CycleColor;
            hotkeyManager.OnTogglePower += TogglePower;
            hotkeyManager.OnBrightnessUp += BrightnessUp;
            hotkeyManager.OnBrightnessDown += BrightnessDown;
            hotkeyManager.Start(this.Handle, AppSettings.Current);

            // Escuta eventos de energia do Windows para restaurar cores após Suspensão (Sleep / Resume)
            Microsoft.Win32.SystemEvents.PowerModeChanged += OnPowerModeChanged;
        }

        private void InitializeComponent()
        {
            this.Text = "OpenClevoRGB";
            this.Size = new Size(880, 835);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = ClrBgWindow;
            this.ForeColor = ClrTextPrimary;
            this.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            this.Icon = CreateAppIcon();

            // ========================================================
            // 1. CABEÇALHO MODERNO
            // ========================================================
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
            lblTitle.Text = "OPENCLEVO RGB";
            lblTitle.Font = new Font("Segoe UI", 14f, FontStyle.Bold);
            lblTitle.ForeColor = ClrAccentCyan;
            lblTitle.Location = new Point(25, 15);
            lblTitle.AutoSize = true;

            lblSubtitle = new Label();
            lblSubtitle.Text = "Controle de Teclado RGB • Clevo / Avell / Sager N850 & N870";
            lblSubtitle.Font = new Font("Segoe UI", 9f);
            lblSubtitle.ForeColor = ClrTextMuted;
            lblSubtitle.Location = new Point(26, 44);
            lblSubtitle.AutoSize = true;

            // Badge de Status com dimensões ampliadas
            Panel pnlStatusBadge = new Panel();
            pnlStatusBadge.Location = new Point(540, 16);
            pnlStatusBadge.Size = new Size(295, 54);
            pnlStatusBadge.BackColor = Color.FromArgb(23, 26, 36);
            pnlStatusBadge.Paint += (s, e) => {
                using (Pen p = new Pen(ClrCardBorder, 1))
                {
                    e.Graphics.DrawRectangle(p, 0, 0, pnlStatusBadge.Width - 1, pnlStatusBadge.Height - 1);
                }
            };

            lblStatusDot = new Label();
            lblStatusDot.Text = "●";
            lblStatusDot.Font = new Font("Segoe UI", 13f, FontStyle.Bold);
            lblStatusDot.ForeColor = Color.Orange;
            lblStatusDot.Location = new Point(10, 8);
            lblStatusDot.AutoSize = true;

            lblStatusText = new Label();
            lblStatusText.Text = "Verificando driver...";
            lblStatusText.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblStatusText.ForeColor = ClrTextPrimary;
            lblStatusText.Location = new Point(32, 11);
            lblStatusText.Size = new Size(255, 18);

            btnInstallDriver = new Button();
            btnInstallDriver.Text = "Reiniciar como Admin";
            btnInstallDriver.Font = new Font("Segoe UI", 8f, FontStyle.Bold);
            btnInstallDriver.BackColor = Color.FromArgb(200, 40, 40);
            btnInstallDriver.ForeColor = Color.White;
            btnInstallDriver.FlatStyle = FlatStyle.Flat;
            btnInstallDriver.FlatAppearance.BorderSize = 0;
            btnInstallDriver.Size = new Size(245, 22);
            btnInstallDriver.Location = new Point(32, 27);
            btnInstallDriver.Cursor = Cursors.Hand;
            btnInstallDriver.Visible = false;
            btnInstallDriver.Click += BtnInstallDriver_Click;
            toolTip.SetToolTip(btnInstallDriver, "Requer privilégios de Administrador para controlar o hardware ACPI.");

            pnlStatusBadge.Controls.Add(lblStatusDot);
            pnlStatusBadge.Controls.Add(lblStatusText);
            pnlStatusBadge.Controls.Add(btnInstallDriver);

            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(lblSubtitle);
            headerPanel.Controls.Add(pnlStatusBadge);
            this.Controls.Add(headerPanel);

            int startY = 100;

            // ========================================================
            // 2. SEÇÃO 1: ZONAS DO TECLADO
            // ========================================================
            cardZones = CreateCardPanel(25, startY, 815, 120);

            Label lblSectionZones = CreateSectionHeader("1. ZONAS DO TECLADO", 15, 12);
            cardZones.Controls.Add(lblSectionZones);

            chkSyncZones = new CheckBox();
            chkSyncZones.Text = "Sincronizar Zonas (todo o teclado)";
            chkSyncZones.Checked = AppSettings.Current.SyncZones;
            chkSyncZones.ForeColor = ClrTextPrimary;
            chkSyncZones.Font = new Font("Segoe UI", 9f);
            chkSyncZones.Location = new Point(540, 12);
            chkSyncZones.AutoSize = true;
            chkSyncZones.Cursor = Cursors.Hand;
            chkSyncZones.CheckedChanged += (s, e) => {
                AppSettings.Current.SyncZones = chkSyncZones.Checked;
                AppSettings.Save();
                UpdateZoneSelectionHighlights();
            };
            toolTip.SetToolTip(chkSyncZones, "Aplica a mesma cor nas 3 zonas simultaneamente.");
            cardZones.Controls.Add(chkSyncZones);

            int totalPreviewW = 785;
            int zoneW = (totalPreviewW - 16) / 3;

            pnlZoneLeft = CreateZoneButton("ESQUERDA", 15, 42, zoneW, 62, KeyboardZone.Left, Color.FromArgb(AppSettings.Current.ColorLeftArgb), "Zona Esquerda (QWE / ASD / Shift)");
            pnlZoneMid = CreateZoneButton("CENTRO", 15 + zoneW + 8, 42, zoneW, 62, KeyboardZone.Middle, Color.FromArgb(AppSettings.Current.ColorMidArgb), "Zona Central (TYU / GHJ / Barra de Espaço)");
            pnlZoneRight = CreateZoneButton("DIREITA", 15 + (zoneW * 2) + 16, 42, zoneW, 62, KeyboardZone.Right, Color.FromArgb(AppSettings.Current.ColorRightArgb), "Zona Direita (Teclado Numérico / Setas)");

            cardZones.Controls.Add(pnlZoneLeft);
            cardZones.Controls.Add(pnlZoneMid);
            cardZones.Controls.Add(pnlZoneRight);
            this.Controls.Add(cardZones);

            startY += 132;

            // ========================================================
            // 3. SEÇÃO 2: PALETA DE CORES
            // ========================================================
            cardColors = CreateCardPanel(25, startY, 815, 115);

            Label lblSectionColors = CreateSectionHeader("2. CORES RÁPIDAS", 15, 12);
            cardColors.Controls.Add(lblSectionColors);

            pnlPresets = new FlowLayoutPanel();
            pnlPresets.Location = new Point(15, 40);
            pnlPresets.Size = new Size(620, 60);
            pnlPresets.BackColor = Color.Transparent;

            // Paleta com 12 cores elegantes e dicas curtas
            AddPresetColorButton(pnlPresets, Color.FromArgb(255, 255, 255), "Branco", "Branco Puro");
            AddPresetColorButton(pnlPresets, Color.FromArgb(0, 229, 255), "Ciano", "Ciano Neon");
            AddPresetColorButton(pnlPresets, Color.FromArgb(0, 140, 255), "Azul", "Azul Cobalto");
            AddPresetColorButton(pnlPresets, Color.FromArgb(0, 70, 255), "Azul Escuro", "Azul Profundo");
            AddPresetColorButton(pnlPresets, Color.FromArgb(0, 230, 118), "Verde Neon", "Verde Vibrante");
            AddPresetColorButton(pnlPresets, Color.FromArgb(0, 200, 80), "Esmeralda", "Verde Esmeralda");
            AddPresetColorButton(pnlPresets, Color.FromArgb(255, 214, 0), "Amarelo", "Amarelo Ouro");
            AddPresetColorButton(pnlPresets, Color.FromArgb(255, 109, 0), "Laranja", "Laranja Sunset");
            AddPresetColorButton(pnlPresets, Color.FromArgb(255, 23, 68), "Vermelho", "Vermelho Rubi");
            AddPresetColorButton(pnlPresets, Color.FromArgb(245, 0, 87), "Rosa", "Rosa Neon");
            AddPresetColorButton(pnlPresets, Color.FromArgb(170, 0, 255), "Roxo", "Roxo Neon");
            AddPresetColorButton(pnlPresets, Color.FromArgb(101, 31, 255), "Violeta", "Violeta Elétrico");

            cardColors.Controls.Add(pnlPresets);

            btnCustomColor = new Button();
            btnCustomColor.Text = "🎨 Mais Cores...";
            btnCustomColor.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            btnCustomColor.Location = new Point(645, 40);
            btnCustomColor.Size = new Size(155, 50);
            btnCustomColor.BackColor = Color.FromArgb(30, 35, 48);
            btnCustomColor.ForeColor = ClrAccentCyan;
            //btnCustomColor.FlatStyle = FlatStyle.Flat;
            //btnCustomColor.FlatAppearance.BorderColor = ClrAccentCyan;
            //btnCustomColor.FlatAppearance.BorderSize = 1;
            //btnCustomColor.Cursor = Cursors.Hand;
            ApplyHoverEffect(btnCustomColor, Color.FromArgb(30, 35, 48), Color.FromArgb(42, 50, 70));
            btnCustomColor.Click += BtnCustomColor_Click;
            toolTip.SetToolTip(btnCustomColor, "Seletor com mais de 16 milhões de cores RGB personalizadas.");
            cardColors.Controls.Add(btnCustomColor);

            this.Controls.Add(cardColors);

            startY += 127;

            // ========================================================
            // 4. SEÇÃO 3: LUMINOSIDADE / BRILHO
            // ========================================================
            cardBrightness = CreateCardPanel(25, startY, 815, 130);

            Label lblSectionBrightness = CreateSectionHeader("3. LUMINOSIDADE", 15, 12);
            cardBrightness.Controls.Add(lblSectionBrightness);

            lblBrightnessValue = new Label();
            lblBrightnessValue.Text = GetBrightnessString(AppSettings.Current.Brightness);
            lblBrightnessValue.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            lblBrightnessValue.ForeColor = ClrAccentCyan;
            lblBrightnessValue.Location = new Point(700, 10);
            lblBrightnessValue.Size = new Size(100, 22);
            lblBrightnessValue.TextAlign = ContentAlignment.MiddleRight;
            cardBrightness.Controls.Add(lblBrightnessValue);

            trackBrightness = new TrackBar();
            trackBrightness.Location = new Point(15, 36);
            trackBrightness.Size = new Size(785, 42);
            trackBrightness.Minimum = 0;
            trackBrightness.Maximum = 4;
            trackBrightness.TickStyle = TickStyle.BottomRight;
            trackBrightness.Value = AppSettings.Current.Brightness;
            trackBrightness.Cursor = Cursors.Hand;
            trackBrightness.Scroll += TrackBrightness_Scroll;
            toolTip.SetToolTip(trackBrightness, "Ajuste contínuo de brilho.");
            cardBrightness.Controls.Add(trackBrightness);

            // Botões de brilho elegantes com texto curto
            int btnBrightW = 145;
            int gap = (785 - (btnBrightW * 5)) / 4;
            int bX = 15;

            btnBright0 = CreateQuickBrightButton("Off (0%)", bX, 84, btnBrightW, 0, "Desliga os LEDs");
            bX += btnBrightW + gap;
            btnBright25 = CreateQuickBrightButton("25%", bX, 84, btnBrightW, 1, "Brilho suave");
            bX += btnBrightW + gap;
            btnBright50 = CreateQuickBrightButton("50%", bX, 84, btnBrightW, 2, "Brilho médio");
            bX += btnBrightW + gap;
            btnBright75 = CreateQuickBrightButton("75%", bX, 84, btnBrightW, 3, "Brilho forte");
            bX += btnBrightW + gap;
            btnBright100 = CreateQuickBrightButton("100%", bX, 84, btnBrightW, 4, "Brilho máximo");

            cardBrightness.Controls.Add(btnBright0);
            cardBrightness.Controls.Add(btnBright25);
            cardBrightness.Controls.Add(btnBright50);
            cardBrightness.Controls.Add(btnBright75);
            cardBrightness.Controls.Add(btnBright100);

            this.Controls.Add(cardBrightness);

            startY += 142;

            // ========================================================
            // 5. SEÇÃO 4: EFEITOS & AÇÕES
            // ========================================================
            cardEffects = CreateCardPanel(25, startY, 815, 95);

            Label lblSectionEffects = CreateSectionHeader("4. EFEITOS DA BIOS", 15, 12);
            cardEffects.Controls.Add(lblSectionEffects);

            // ComboBox estilizado com desenho OwnerDraw moderno
            cmbEffects = new ComboBox();
            cmbEffects.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbEffects.DrawMode = DrawMode.OwnerDrawFixed;
            cmbEffects.ItemHeight = 26;
            cmbEffects.BackColor = Color.FromArgb(28, 32, 44);
            cmbEffects.ForeColor = Color.White;
            cmbEffects.Font = new Font("Segoe UI", 9.5f);
            cmbEffects.Location = new Point(15, 42);
            cmbEffects.Size = new Size(330, 32);
            cmbEffects.Cursor = Cursors.Hand;
            cmbEffects.Items.AddRange(new object[] {
                "Estático (Cor Fixa)",
                "Respiração (Breathe)",
                "Ciclo de Cores (Cycle)",
                "Onda de Cores (Wave)",
                "Dança (Dance)",
                "Flash / Tempo",
                "Aleatório (Random)"
            });
            cmbEffects.DrawItem += CmbEffects_DrawItem;
            cmbEffects.SelectedIndex = AppSettings.Current.Effect;
            cmbEffects.SelectedIndexChanged += CmbEffects_SelectedIndexChanged;
            toolTip.SetToolTip(cmbEffects, "Efeitos de iluminação gravados na BIOS da Clevo.");
            cardEffects.Controls.Add(cmbEffects);

            btnTogglePower = new Button();
            btnTogglePower.Text = "💡 Desligar LEDs";
            btnTogglePower.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            btnTogglePower.Location = new Point(365, 40);
            btnTogglePower.Size = new Size(185, 36);
            btnTogglePower.BackColor = Color.FromArgb(38, 43, 58);
            btnTogglePower.ForeColor = ClrTextPrimary;
            btnTogglePower.FlatStyle = FlatStyle.Flat;
            btnTogglePower.FlatAppearance.BorderColor = ClrCardBorder;
            btnTogglePower.Cursor = Cursors.Hand;
            ApplyHoverEffect(btnTogglePower, Color.FromArgb(38, 43, 58), Color.FromArgb(52, 59, 80));
            btnTogglePower.Click += BtnTogglePower_Click;
            toolTip.SetToolTip(btnTogglePower, "Alterna ligar/desligar iluminação.");
            cardEffects.Controls.Add(btnTogglePower);

            btnApply = new Button();
            btnApply.Text = "⚡ Aplicar ao Teclado";
            btnApply.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            btnApply.Location = new Point(570, 40);
            btnApply.Size = new Size(230, 36);
            btnApply.BackColor = Color.FromArgb(0, 150, 220);
            btnApply.ForeColor = Color.White;
            btnApply.FlatStyle = FlatStyle.Flat;
            btnApply.FlatAppearance.BorderSize = 0;
            btnApply.Cursor = Cursors.Hand;
            ApplyHoverEffect(btnApply, Color.FromArgb(0, 150, 220), Color.FromArgb(0, 185, 255));
            btnApply.Click += (s, e) => ApplyCurrentSettingsToHardware();
            toolTip.SetToolTip(btnApply, "Grava as configurações no teclado agora.");
            cardEffects.Controls.Add(btnApply);

            this.Controls.Add(cardEffects);

            startY += 107;

            // ========================================================
            // 6. SEÇÃO 5: ATALHOS DE TECLADO (HOTKEYS)
            // ========================================================
            cardHotkeys = CreateCardPanel(25, startY, 815, 92);

            Label lblSectionHotkeys = CreateSectionHeader("5. ATALHOS DE TECLADO (HOTKEYS)", 15, 12);
            cardHotkeys.Controls.Add(lblSectionHotkeys);

            chkEnableHotkeys = new CheckBox();
            chkEnableHotkeys.Text = "Habilitar Atalhos Globais";
            chkEnableHotkeys.Checked = AppSettings.Current.EnableHotkeys;
            chkEnableHotkeys.ForeColor = ClrTextPrimary;
            chkEnableHotkeys.Font = new Font("Segoe UI", 9f);
            chkEnableHotkeys.Location = new Point(540, 12);
            chkEnableHotkeys.AutoSize = true;
            chkEnableHotkeys.Cursor = Cursors.Hand;
            chkEnableHotkeys.CheckedChanged += (s, e) => {
                AppSettings.Current.EnableHotkeys = chkEnableHotkeys.Checked;
                AppSettings.Save();
                if (hotkeyManager != null)
                {
                    hotkeyManager.RegisterAll(AppSettings.Current);
                }
                UpdateHotkeysSummary();
            };
            toolTip.SetToolTip(chkEnableHotkeys, "Ativa o controle do teclado RGB via teclas de atalho de dentro de qualquer jogo ou aplicativo.");
            cardHotkeys.Controls.Add(chkEnableHotkeys);

            lblHotkeysSummary = new Label();
            lblHotkeysSummary.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            lblHotkeysSummary.Location = new Point(15, 38);
            lblHotkeysSummary.Size = new Size(610, 22);
            cardHotkeys.Controls.Add(lblHotkeysSummary);

            lblHotkeysSubtext = new Label();
            lblHotkeysSubtext.Text = "* Padrão: Teclas NumPad com Ctrl (ou Fn + NumPad do notebook via ACPI hardware).";
            lblHotkeysSubtext.Font = new Font("Segoe UI", 8.5f);
            lblHotkeysSubtext.ForeColor = ClrTextMuted;
            lblHotkeysSubtext.Location = new Point(15, 62);
            lblHotkeysSubtext.Size = new Size(610, 20);
            cardHotkeys.Controls.Add(lblHotkeysSubtext);

            btnConfigureHotkeys = new Button();
            btnConfigureHotkeys.Text = "⌨️ Personalizar...";
            btnConfigureHotkeys.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnConfigureHotkeys.Location = new Point(640, 38);
            btnConfigureHotkeys.Size = new Size(160, 36);
            btnConfigureHotkeys.BackColor = Color.FromArgb(32, 38, 52);
            btnConfigureHotkeys.ForeColor = ClrAccentCyan;
            btnConfigureHotkeys.FlatStyle = FlatStyle.Flat;
            btnConfigureHotkeys.FlatAppearance.BorderColor = ClrAccentCyan;
            btnConfigureHotkeys.Cursor = Cursors.Hand;
            ApplyHoverEffect(btnConfigureHotkeys, Color.FromArgb(32, 38, 52), Color.FromArgb(44, 52, 72));
            btnConfigureHotkeys.Click += BtnConfigureHotkeys_Click;
            toolTip.SetToolTip(btnConfigureHotkeys, "Abre a tela de personalização das combinações de teclas de atalho.");
            cardHotkeys.Controls.Add(btnConfigureHotkeys);

            this.Controls.Add(cardHotkeys);

            UpdateHotkeysSummary();

            startY += 104;

            // ========================================================
            // 7. SEÇÃO 6: RODAPÉ COM OPÇÕES
            // ========================================================
            footerPanel = CreateCardPanel(25, startY, 815, 54);

            chkStartWithWindows = new CheckBox();
            chkStartWithWindows.Text = "Iniciar com o Windows";
            chkStartWithWindows.Checked = AppSettings.Current.StartWithWindows;
            chkStartWithWindows.Location = new Point(20, 16);
            chkStartWithWindows.AutoSize = true;
            chkStartWithWindows.ForeColor = ClrTextPrimary;
            chkStartWithWindows.Cursor = Cursors.Hand;
            chkStartWithWindows.CheckedChanged += (s, e) => {
                AppSettings.Current.StartWithWindows = chkStartWithWindows.Checked;
                AppSettings.SetStartWithWindows(chkStartWithWindows.Checked, Application.ExecutablePath);
                AppSettings.Save();
            };
            toolTip.SetToolTip(chkStartWithWindows, "Inicia minimizado junto ao Windows e restaura suas cores.");

            chkMinimizeToTray = new CheckBox();
            chkMinimizeToTray.Text = "Minimizar para a bandeja ao fechar";
            chkMinimizeToTray.Checked = AppSettings.Current.MinimizeToTray;
            chkMinimizeToTray.Location = new Point(340, 16);
            chkMinimizeToTray.AutoSize = true;
            chkMinimizeToTray.ForeColor = ClrTextPrimary;
            chkMinimizeToTray.Cursor = Cursors.Hand;
            chkMinimizeToTray.CheckedChanged += (s, e) => {
                AppSettings.Current.MinimizeToTray = chkMinimizeToTray.Checked;
                AppSettings.Save();
            };
            toolTip.SetToolTip(chkMinimizeToTray, "Fecha para o relógio da barra de tarefas.");

            footerPanel.Controls.Add(chkStartWithWindows);
            footerPanel.Controls.Add(chkMinimizeToTray);

            Label lblAiCredit = new Label();
            lblAiCredit.Text = "⚡ Powered by Gemini 3.8 Flash";
            lblAiCredit.Font = new Font("Segoe UI", 8f, FontStyle.Italic);
            lblAiCredit.ForeColor = Color.FromArgb(115, 125, 150);
            lblAiCredit.Location = new Point(605, 18);
            lblAiCredit.AutoSize = true;
            toolTip.SetToolTip(lblAiCredit, "Desenvolvido em Pair Programming com Google Gemini 3.8 Flash.");
            footerPanel.Controls.Add(lblAiCredit);

            this.Controls.Add(footerPanel);

            this.FormClosing += MainForm_FormClosing;
        }

        private void CmbEffects_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color bgColor = isSelected ? Color.FromArgb(0, 120, 215) : Color.FromArgb(28, 32, 44);
            Color fgColor = Color.White;

            using (SolidBrush bgBrush = new SolidBrush(bgColor))
            {
                e.Graphics.FillRectangle(bgBrush, e.Bounds);
            }

            string text = cmbEffects.Items[e.Index].ToString();
            using (SolidBrush textBrush = new SolidBrush(fgColor))
            {
                e.Graphics.DrawString(text, cmbEffects.Font, textBrush, e.Bounds.X + 8, e.Bounds.Y + 4);
            }
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

        private Label CreateSectionHeader(string text, int x, int y)
        {
            Label lbl = new Label();
            lbl.Text = text;
            lbl.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            lbl.ForeColor = ClrTextMuted;
            lbl.Location = new Point(x, y);
            lbl.AutoSize = true;
            return lbl;
        }

        private Panel CreateZoneButton(string text, int x, int y, int width, int height, KeyboardZone zone, Color initialColor, string tip)
        {
            Panel pnl = new Panel();
            pnl.Location = new Point(x, y);
            pnl.Size = new Size(width, height);
            pnl.BackColor = initialColor;
            pnl.Cursor = Cursors.Hand;

            Label lbl = new Label();
            lbl.Text = text;
            lbl.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            lbl.ForeColor = GetContrastColor(initialColor);
            lbl.Dock = DockStyle.Fill;
            lbl.TextAlign = ContentAlignment.MiddleCenter;
            lbl.Cursor = Cursors.Hand;

            Action selectAction = () => {
                selectedZone = zone;
                UpdateZoneSelectionHighlights();
            };

            lbl.Click += (s, e) => selectAction();
            pnl.Click += (s, e) => selectAction();
            pnl.Controls.Add(lbl);

            pnl.Paint += (s, e) => {
                bool isFocused = (selectedZone == zone || chkSyncZones.Checked);
                Color borderColor = isFocused ? ClrAccentCyan : Color.FromArgb(70, 75, 95);
                int thickness = isFocused ? 3 : 1;
                using (Pen p = new Pen(borderColor, thickness))
                {
                    e.Graphics.DrawRectangle(p, 0, 0, pnl.Width - 1, pnl.Height - 1);
                }
            };

            if (zone == KeyboardZone.Left) lblZoneLeft = lbl;
            if (zone == KeyboardZone.Middle) lblZoneMid = lbl;
            if (zone == KeyboardZone.Right) lblZoneRight = lbl;

            toolTip.SetToolTip(pnl, tip);
            toolTip.SetToolTip(lbl, tip);

            return pnl;
        }

        private Button CreateQuickBrightButton(string text, int x, int y, int width, int level, string tip)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btn.Location = new Point(x, y);
            btn.Size = new Size(width, 28);
            btn.BackColor = Color.FromArgb(30, 34, 46);
            btn.ForeColor = ClrTextPrimary;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = ClrCardBorder;
            btn.Cursor = Cursors.Hand;
            ApplyHoverEffect(btn, Color.FromArgb(30, 34, 46), Color.FromArgb(46, 54, 74));

            btn.Click += (s, e) => {
                trackBrightness.Value = level;
                TrackBrightness_Scroll(null, null);
            };

            toolTip.SetToolTip(btn, tip);
            return btn;
        }

        private void AddPresetColorButton(Control parent, Color color, string name, string tip)
        {
            Button btn = new Button();
            btn.Size = new Size(46, 36);
            btn.Margin = new Padding(2, 2, 4, 2);
            btn.BackColor = color;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Color.FromArgb(60, 65, 80);
            btn.Cursor = Cursors.Hand;

            btn.MouseEnter += (s, e) => {
                btn.FlatAppearance.BorderSize = 2;
                btn.FlatAppearance.BorderColor = Color.White;
            };
            btn.MouseLeave += (s, e) => {
                btn.FlatAppearance.BorderSize = 1;
                btn.FlatAppearance.BorderColor = Color.FromArgb(60, 65, 80);
            };

            btn.Click += (s, e) => SetZoneColor(color);

            toolTip.SetToolTip(btn, name + ": " + tip);
            parent.Controls.Add(btn);
        }

        private void ApplyHoverEffect(Button btn, Color normalBg, Color hoverBg)
        {
            btn.MouseEnter += (s, e) => {
                btn.BackColor = hoverBg;
            };
            btn.MouseLeave += (s, e) => {
                btn.BackColor = normalBg;
            };
        }

        private void UpdateZoneSelectionHighlights()
        {
            pnlZoneLeft.Invalidate();
            pnlZoneMid.Invalidate();
            pnlZoneRight.Invalidate();
        }

        private void BtnCustomColor_Click(object sender, EventArgs e)
        {
            using (ColorDialog cd = new ColorDialog())
            {
                cd.FullOpen = true;
                if (cd.ShowDialog() == DialogResult.OK)
                {
                    SetZoneColor(cd.Color);
                }
            }
        }

        private void SetZoneColor(Color color)
        {
            if (chkSyncZones.Checked)
            {
                pnlZoneLeft.BackColor = color;
                pnlZoneMid.BackColor = color;
                pnlZoneRight.BackColor = color;
                lblZoneLeft.ForeColor = GetContrastColor(color);
                lblZoneMid.ForeColor = GetContrastColor(color);
                lblZoneRight.ForeColor = GetContrastColor(color);

                AppSettings.Current.ColorLeftArgb = color.ToArgb();
                AppSettings.Current.ColorMidArgb = color.ToArgb();
                AppSettings.Current.ColorRightArgb = color.ToArgb();

                controller.SetAllZonesColor(color);
            }
            else
            {
                switch (selectedZone)
                {
                    case KeyboardZone.Left:
                        pnlZoneLeft.BackColor = color;
                        lblZoneLeft.ForeColor = GetContrastColor(color);
                        AppSettings.Current.ColorLeftArgb = color.ToArgb();
                        controller.SetZoneColor(KeyboardZone.Left, color);
                        break;

                    case KeyboardZone.Middle:
                        pnlZoneMid.BackColor = color;
                        lblZoneMid.ForeColor = GetContrastColor(color);
                        AppSettings.Current.ColorMidArgb = color.ToArgb();
                        controller.SetZoneColor(KeyboardZone.Middle, color);
                        break;

                    case KeyboardZone.Right:
                        pnlZoneRight.BackColor = color;
                        lblZoneRight.ForeColor = GetContrastColor(color);
                        AppSettings.Current.ColorRightArgb = color.ToArgb();
                        controller.SetZoneColor(KeyboardZone.Right, color);
                        break;
                }
            }

            UpdateZoneSelectionHighlights();
            AppSettings.Save();
        }

        private void TrackBrightness_Scroll(object sender, EventArgs e)
        {
            lblBrightnessValue.Text = GetBrightnessString(trackBrightness.Value);
            AppSettings.Current.Brightness = trackBrightness.Value;
            AppSettings.Save();

            controller.SetBrightness(trackBrightness.Value);
            isLightOn = (trackBrightness.Value > 0);
            btnTogglePower.Text = isLightOn ? "💡 Desligar LEDs" : "💡 Ligar LEDs";
        }

        private void CmbEffects_SelectedIndexChanged(object sender, EventArgs e)
        {
            AppSettings.Current.Effect = cmbEffects.SelectedIndex;
            AppSettings.Save();

            if (cmbEffects.SelectedIndex > 0)
            {
                controller.SetEffect((KeyboardEffect)cmbEffects.SelectedIndex);
            }
            else
            {
                ApplyCurrentSettingsToHardware();
            }
        }

        private void BtnTogglePower_Click(object sender, EventArgs e)
        {
            isLightOn = !isLightOn;
            if (isLightOn)
            {
                if (trackBrightness.Value == 0) trackBrightness.Value = 4;
                lblBrightnessValue.Text = GetBrightnessString(trackBrightness.Value);
                btnTogglePower.Text = "💡 Desligar LEDs";
                controller.SetBrightness(trackBrightness.Value);
                ApplyCurrentSettingsToHardware();
            }
            else
            {
                btnTogglePower.Text = "💡 Ligar LEDs";
                controller.SetPower(false);
            }
        }

        private void ApplySavedSettings()
        {
            Color cLeft = Color.FromArgb(AppSettings.Current.ColorLeftArgb);
            Color cMid = Color.FromArgb(AppSettings.Current.ColorMidArgb);
            Color cRight = Color.FromArgb(AppSettings.Current.ColorRightArgb);

            pnlZoneLeft.BackColor = cLeft;
            pnlZoneMid.BackColor = cMid;
            pnlZoneRight.BackColor = cRight;
            lblZoneLeft.ForeColor = GetContrastColor(cLeft);
            lblZoneMid.ForeColor = GetContrastColor(cMid);
            lblZoneRight.ForeColor = GetContrastColor(cRight);

            trackBrightness.Value = AppSettings.Current.Brightness;
            lblBrightnessValue.Text = GetBrightnessString(trackBrightness.Value);
            cmbEffects.SelectedIndex = AppSettings.Current.Effect;

            if (controller.IsInitialized)
            {
                ApplyCurrentSettingsToHardware();
            }
        }

        public void ApplyCurrentSettingsToHardware()
        {
            if (trackBrightness.Value == 0)
            {
                controller.SetPower(false);
                return;
            }

            controller.SetBrightness(trackBrightness.Value);

            Color cLeft = Color.FromArgb(AppSettings.Current.ColorLeftArgb);
            Color cMid = Color.FromArgb(AppSettings.Current.ColorMidArgb);
            Color cRight = Color.FromArgb(AppSettings.Current.ColorRightArgb);

            controller.SetZoneColor(KeyboardZone.Left, cLeft);
            controller.SetZoneColor(KeyboardZone.Middle, cMid);
            controller.SetZoneColor(KeyboardZone.Right, cRight);

            if (cmbEffects.SelectedIndex > 0)
            {
                controller.SetEffect((KeyboardEffect)cmbEffects.SelectedIndex);
            }
        }

        private void RefreshDriverStatus()
        {
            bool isWmiReady = controller.TryInitialize();
            bool isConfigured = controller.IsDriverConfigured();

            if (isWmiReady)
            {
                lblStatusDot.ForeColor = ClrAccentGreen;
                lblStatusText.Text = "Driver WMI Ativo";
                lblStatusText.ForeColor = ClrAccentGreen;
                btnInstallDriver.Visible = false;
                toolTip.SetToolTip(lblStatusText, "Hardware ACPI Clevo conectado e respondendo perfeitamente.");
            }
            else if (controller.IsAccessDenied)
            {
                lblStatusDot.ForeColor = Color.Red;
                lblStatusText.Text = "Requer Modo Admin";
                lblStatusText.ForeColor = Color.FromArgb(255, 80, 80);
                btnInstallDriver.Text = "Reiniciar como Admin";
                btnInstallDriver.BackColor = Color.FromArgb(210, 40, 40);
                btnInstallDriver.Visible = true;
                toolTip.SetToolTip(lblStatusText, "Clique no botão abaixo para reiniciar com permissões de Administrador.");
            }
            else if (isConfigured)
            {
                lblStatusDot.ForeColor = Color.Orange;
                lblStatusText.Text = "Metadados OK";
                lblStatusText.ForeColor = Color.Orange;
                btnInstallDriver.Visible = false;
            }
            else
            {
                lblStatusDot.ForeColor = Color.Red;
                lblStatusText.Text = "Sem Metadados ACPI";
                lblStatusText.ForeColor = Color.Red;
                btnInstallDriver.Text = "Instalar Metadados";
                btnInstallDriver.BackColor = Color.FromArgb(220, 120, 0);
                btnInstallDriver.Visible = true;
            }
        }

        private void BtnInstallDriver_Click(object sender, EventArgs e)
        {
            if (controller.IsAccessDenied)
            {
                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = Application.ExecutablePath;
                    psi.Verb = "runas";
                    Process.Start(psi);
                    Application.Exit();
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Falha ao elevar privilégios: " + ex.Message, "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            string localDll = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "clevomof.dll");
            if (!File.Exists(localDll))
            {
                MessageBox.Show(
                    "O arquivo 'clevomof.dll' não foi encontrado na pasta do programa.\nCertifique-se de que ele esteja junto ao executável.",
                    "Arquivo não encontrado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            bool success = controller.InstallDriverMetadata(localDll);
            if (success)
            {
                MessageBox.Show(
                    "Metadados Clevo (clevomof.dll) instalados e registrados no serviço WmiAcpi com sucesso!\n\n" +
                    "O subsistema ACPI/WMI do Windows foi configurado.\n" +
                    "Caso os LEDs ainda não respondam de imediato, reinicie o Windows uma vez para que o driver WmiAcpi recarregue.",
                    "Configuração Concluída",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                RefreshDriverStatus();
            }
            else
            {
                DialogResult res = MessageBox.Show(
                    "Não foi possível gravar no registro do Windows (requer permissão de Administrador).\n\n" +
                    "Deseja reiniciar este aplicativo como Administrador para concluir a configuração?",
                    "Permissão Necessária",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (res == DialogResult.Yes)
                {
                    try
                    {
                        ProcessStartInfo psi = new ProcessStartInfo();
                        psi.FileName = Application.ExecutablePath;
                        psi.Verb = "runas";
                        Process.Start(psi);
                        Application.Exit();
                    }
                    catch
                    {
                        MessageBox.Show("Elevação cancelada pelo usuário.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private void SetupTrayIcon()
        {
            trayMenu = new ContextMenuStrip();
            trayMenu.BackColor = Color.FromArgb(24, 28, 38);
            trayMenu.ForeColor = Color.White;

            trayMenu.Items.Add("Abrir Painel", null, (s, e) => ShowWindow());
            trayMenu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem menuBrightness = new ToolStripMenuItem("Brilho");
            menuBrightness.DropDownItems.Add("0% (Desligado)", null, (s, e) => SetBrightnessFromTray(0));
            menuBrightness.DropDownItems.Add("25%", null, (s, e) => SetBrightnessFromTray(1));
            menuBrightness.DropDownItems.Add("50%", null, (s, e) => SetBrightnessFromTray(2));
            menuBrightness.DropDownItems.Add("75%", null, (s, e) => SetBrightnessFromTray(3));
            menuBrightness.DropDownItems.Add("100%", null, (s, e) => SetBrightnessFromTray(4));
            trayMenu.Items.Add(menuBrightness);

            ToolStripMenuItem menuColors = new ToolStripMenuItem("Cores");
            menuColors.DropDownItems.Add("Branco", null, (s, e) => SetColorFromTray(Color.FromArgb(255, 255, 255)));
            menuColors.DropDownItems.Add("Ciano", null, (s, e) => SetColorFromTray(Color.FromArgb(0, 229, 255)));
            menuColors.DropDownItems.Add("Azul", null, (s, e) => SetColorFromTray(Color.FromArgb(0, 140, 255)));
            menuColors.DropDownItems.Add("Verde", null, (s, e) => SetColorFromTray(Color.FromArgb(0, 230, 118)));
            menuColors.DropDownItems.Add("Amarelo", null, (s, e) => SetColorFromTray(Color.FromArgb(255, 214, 0)));
            menuColors.DropDownItems.Add("Vermelho", null, (s, e) => SetColorFromTray(Color.FromArgb(255, 23, 68)));
            menuColors.DropDownItems.Add("Roxo", null, (s, e) => SetColorFromTray(Color.FromArgb(170, 0, 255)));
            trayMenu.Items.Add(menuColors);

            trayMenu.Items.Add(new ToolStripSeparator());
            trayMenu.Items.Add("Sobre o OpenClevoRGB...", null, (s, e) => {
                MessageBox.Show(
                    "OpenClevoRGB v1.4.0\n\n" +
                    "Desenvolvido por: Felipe Lima\n" +
                    "Engenharia e Pair Programming: Google Gemini 3.8 Flash\n" +
                    "Licença: MIT Open Source\n\n" +
                    "Utilitário nativo e independente de controle de iluminação RGB para notebooks Clevo / Avell / Sager.",
                    "Sobre o OpenClevoRGB",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            });
            trayMenu.Items.Add("Desligar LEDs", null, (s, e) => SetBrightnessFromTray(0));
            trayMenu.Items.Add("Sair", null, (s, e) => {
                trayIcon.Visible = false;
                Application.Exit();
            });

            trayIcon = new NotifyIcon();
            trayIcon.Text = "OpenClevoRGB";
            trayIcon.Icon = this.Icon;
            trayIcon.ContextMenuStrip = trayMenu;
            trayIcon.Visible = true;
            trayIcon.DoubleClick += (s, e) => ShowWindow();
        }

        private void SetBrightnessFromTray(int level)
        {
            trackBrightness.Value = level;
            lblBrightnessValue.Text = GetBrightnessString(level);
            AppSettings.Current.Brightness = level;
            AppSettings.Save();
            controller.SetBrightness(level);
        }

        private void SetColorFromTray(Color color)
        {
            chkSyncZones.Checked = true;
            SetZoneColor(color);
        }

        public void ShowWindow()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
            this.Activate();
        }

        protected override void WndProc(ref Message m)
        {
            if (hotkeyManager != null && hotkeyManager.ProcessMessage(ref m))
            {
                return;
            }
            base.WndProc(ref m);
        }

        private static readonly Color[] CycleColors = new Color[]
        {
            Color.FromArgb(0, 229, 255),   // Ciano
            Color.FromArgb(0, 140, 255),   // Azul
            Color.FromArgb(0, 70, 255),    // Azul Escuro
            Color.FromArgb(0, 230, 118),   // Verde Neon
            Color.FromArgb(0, 200, 80),    // Esmeralda
            Color.FromArgb(255, 214, 0),   // Amarelo
            Color.FromArgb(255, 109, 0),   // Laranja
            Color.FromArgb(255, 23, 68),    // Vermelho
            Color.FromArgb(245, 0, 87),    // Rosa
            Color.FromArgb(170, 0, 255),   // Roxo
            Color.FromArgb(101, 31, 255),  // Violeta
            Color.FromArgb(255, 255, 255)  // Branco
        };

        private static readonly string[] CycleColorNames = new string[]
        {
            "Ciano",
            "Azul",
            "Azul Escuro",
            "Verde Neon",
            "Esmeralda",
            "Amarelo",
            "Laranja",
            "Vermelho",
            "Rosa",
            "Roxo",
            "Violeta",
            "Branco"
        };

        private int cycleColorIndex = 0;

        public void CycleColor()
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(CycleColor));
                return;
            }

            cycleColorIndex = (cycleColorIndex + 1) % CycleColors.Length;
            Color nextColor = CycleColors[cycleColorIndex];
            string name = CycleColorNames[cycleColorIndex];

            chkSyncZones.Checked = true;
            SetZoneColor(nextColor);

            if (this.WindowState == FormWindowState.Minimized || !this.Visible)
            {
                trayIcon.ShowBalloonTip(1000, "OpenClevoRGB", "Cor do teclado alterada para: " + name, ToolTipIcon.Info);
            }
        }

        public void TogglePower()
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(TogglePower));
                return;
            }

            BtnTogglePower_Click(null, null);

            if (this.WindowState == FormWindowState.Minimized || !this.Visible)
            {
                trayIcon.ShowBalloonTip(1000, "OpenClevoRGB", isLightOn ? "LEDs Ligados" : "LEDs Desligados", ToolTipIcon.Info);
            }
        }

        public void BrightnessUp()
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(BrightnessUp));
                return;
            }

            if (trackBrightness.Value < 4)
            {
                trackBrightness.Value++;
                TrackBrightness_Scroll(null, null);
            }

            if (this.WindowState == FormWindowState.Minimized || !this.Visible)
            {
                trayIcon.ShowBalloonTip(1000, "OpenClevoRGB", "Brilho do Teclado: " + GetBrightnessString(trackBrightness.Value), ToolTipIcon.Info);
            }
        }

        public void BrightnessDown()
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(BrightnessDown));
                return;
            }

            if (trackBrightness.Value > 0)
            {
                trackBrightness.Value--;
                TrackBrightness_Scroll(null, null);
            }

            if (this.WindowState == FormWindowState.Minimized || !this.Visible)
            {
                trayIcon.ShowBalloonTip(1000, "OpenClevoRGB", "Brilho do Teclado: " + GetBrightnessString(trackBrightness.Value), ToolTipIcon.Info);
            }
        }

        private void UpdateHotkeysSummary()
        {
            if (AppSettings.Current.EnableHotkeys)
            {
                lblHotkeysSummary.Text = string.Format(
                    "🎨 Cor: [{0}]   •   💡 Luz: [{1}]   •   🔆 Brilho +: [{2}]   •   🔅 Brilho -: [{3}]",
                    AppSettings.Current.HotkeyCycleColor.ToDisplayString(),
                    AppSettings.Current.HotkeyTogglePower.ToDisplayString(),
                    AppSettings.Current.HotkeyBrightnessUp.ToDisplayString(),
                    AppSettings.Current.HotkeyBrightnessDown.ToDisplayString()
                );
                lblHotkeysSummary.ForeColor = ClrTextPrimary;
            }
            else
            {
                lblHotkeysSummary.Text = "Atalhos globais de teclado desativados.";
                lblHotkeysSummary.ForeColor = ClrTextMuted;
            }
        }

        private void BtnConfigureHotkeys_Click(object sender, EventArgs e)
        {
            using (HotkeySettingsForm dlg = new HotkeySettingsForm())
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    UpdateHotkeysSummary();
                    if (hotkeyManager != null)
                    {
                        hotkeyManager.RegisterAll(AppSettings.Current);
                    }
                }
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (AppSettings.Current.MinimizeToTray && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
                trayIcon.ShowBalloonTip(1500, "OpenClevoRGB", "O utilitário continua ativo na bandeja do sistema.", ToolTipIcon.Info);
            }
            else
            {
                if (hotkeyManager != null)
                {
                    hotkeyManager.Dispose();
                    hotkeyManager = null;
                }
                Microsoft.Win32.SystemEvents.PowerModeChanged -= OnPowerModeChanged;
                trayIcon.Visible = false;
            }
        }

        private void OnPowerModeChanged(object sender, Microsoft.Win32.PowerModeChangedEventArgs e)
        {
            if (e.Mode == Microsoft.Win32.PowerModes.Resume)
            {
                // Quando o notebook acorda da suspensão, o EC da Clevo costuma resetar as luzes.
                // Aguardamos 1.5s para os drivers ACPI acordarem e restauramos as cores configuradas.
                System.Threading.ThreadPool.QueueUserWorkItem(state =>
                {
                    System.Threading.Thread.Sleep(1500);
                    try
                    {
                        if (!this.IsDisposed && this.IsHandleCreated)
                        {
                            this.BeginInvoke(new Action(() => {
                                if (controller != null)
                                {
                                    ApplyCurrentSettingsToHardware();
                                }
                            }));
                        }
                    }
                    catch {}
                });
            }
        }

        private string GetBrightnessString(int val)
        {
            switch (val)
            {
                case 0: return "0% (Off)";
                case 1: return "25%";
                case 2: return "50%";
                case 3: return "75%";
                case 4: return "100%";
                default: return val.ToString();
            }
        }

        private Color GetContrastColor(Color color)
        {
            double luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255.0;
            return (luminance > 0.5) ? Color.FromArgb(12, 14, 20) : Color.White;
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
