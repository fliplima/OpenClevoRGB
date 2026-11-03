using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace OpenClevoRGB
{
    public class HotkeyKeyItem
    {
        public string DisplayName { get; set; }
        public Keys Key { get; set; }

        public HotkeyKeyItem(string displayName, Keys key)
        {
            DisplayName = displayName;
            Key = key;
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }

    public class HotkeySettingsForm : Form
    {
        // Controles para Ciclo de Cores
        private CheckBox chkCycleCtrl;
        private CheckBox chkCycleAlt;
        private CheckBox chkCycleShift;
        private CheckBox chkCycleWin;
        private ComboBox cmbCycleKey;

        // Controles para Ligar/Desligar
        private CheckBox chkPowerCtrl;
        private CheckBox chkPowerAlt;
        private CheckBox chkPowerShift;
        private CheckBox chkPowerWin;
        private ComboBox cmbPowerKey;

        // Controles para Aumentar Brilho
        private CheckBox chkUpCtrl;
        private CheckBox chkUpAlt;
        private CheckBox chkUpShift;
        private CheckBox chkUpWin;
        private ComboBox cmbUpKey;

        // Controles para Reduzir Brilho
        private CheckBox chkDownCtrl;
        private CheckBox chkDownAlt;
        private CheckBox chkDownShift;
        private CheckBox chkDownWin;
        private ComboBox cmbDownKey;

        // Botões de Ação
        private Button btnRestoreDefaults;
        private Button btnCancel;
        private Button btnSave;

        // Cores Dark Theme Moderno
        private readonly Color ClrBgWindow     = Color.FromArgb(12, 13, 17);
        private readonly Color ClrCardBg       = Color.FromArgb(19, 22, 30);
        private readonly Color ClrCardBorder   = Color.FromArgb(34, 39, 53);
        private readonly Color ClrAccentCyan   = Color.FromArgb(0, 229, 255);
        private readonly Color ClrTextPrimary  = Color.FromArgb(240, 243, 250);
        private readonly Color ClrTextMuted    = Color.FromArgb(140, 148, 168);

        public HotkeySettingsForm()
        {
            InitializeComponent();
            LoadCurrentBindings();
        }

        private void InitializeComponent()
        {
            this.Text = "Configurar Atalhos de Teclado • OpenClevoRGB";
            this.Size = new Size(720, 560);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = ClrBgWindow;
            this.ForeColor = ClrTextPrimary;
            this.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);

            // 1. Cabeçalho
            Panel headerPanel = new Panel();
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 78;
            headerPanel.BackColor = Color.FromArgb(16, 18, 25);
            headerPanel.Paint += (s, e) => {
                using (Pen borderPen = new Pen(ClrCardBorder, 1))
                {
                    e.Graphics.DrawLine(borderPen, 0, headerPanel.Height - 1, headerPanel.Width, headerPanel.Height - 1);
                }
            };

            Label lblTitle = new Label();
            lblTitle.Text = "CONFIGURAR ATALHOS DE TECLADO";
            lblTitle.Font = new Font("Segoe UI", 13f, FontStyle.Bold);
            lblTitle.ForeColor = ClrAccentCyan;
            lblTitle.Location = new Point(25, 14);
            lblTitle.AutoSize = true;

            Label lblSubtitle = new Label();
            lblSubtitle.Text = "Personalize as combinações de teclas para controle rápido da iluminação";
            lblSubtitle.Font = new Font("Segoe UI", 9f);
            lblSubtitle.ForeColor = ClrTextMuted;
            lblSubtitle.Location = new Point(26, 42);
            lblSubtitle.AutoSize = true;

            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(lblSubtitle);
            this.Controls.Add(headerPanel);

            int startY = 92;

            // 2. Card Principal com as 4 Ações
            Panel cardBindings = CreateCardPanel(25, startY, 655, 260);

            int rowY = 16;
            CreateBindingRow(cardBindings, "🎨 Alternar Cor (Ciclo):", rowY, 
                out chkCycleCtrl, out chkCycleAlt, out chkCycleShift, out chkCycleWin, out cmbCycleKey);

            rowY += 58;
            CreateBindingRow(cardBindings, "💡 Ligar / Desligar Luz:", rowY, 
                out chkPowerCtrl, out chkPowerAlt, out chkPowerShift, out chkPowerWin, out cmbPowerKey);

            rowY += 58;
            CreateBindingRow(cardBindings, "🔆 Aumentar Brilho (+):", rowY, 
                out chkUpCtrl, out chkUpAlt, out chkUpShift, out chkUpWin, out cmbUpKey);

            rowY += 58;
            CreateBindingRow(cardBindings, "🔅 Reduzir Brilho (-):", rowY, 
                out chkDownCtrl, out chkDownAlt, out chkDownShift, out chkDownWin, out cmbDownKey);

            this.Controls.Add(cardBindings);

            startY += 272;

            // 3. Card Informativo sobre Tecla Fn
            Panel cardInfo = CreateCardPanel(25, startY, 655, 75);
            cardInfo.BackColor = Color.FromArgb(17, 23, 34);

            Label lblInfo = new Label();
            lblInfo.Text = 
                "💡 Teclas Físicas do Notebook (Fn + NumPad):\n" +
                "As teclas originais com ícones de luz (Fn + /, *, +, -) são disparadas pelo BIOS. Os atalhos globais acima\n" +
                "permitem controlar as luzes de dentro de qualquer jogo ou programa utilizando [Ctrl + NumPad] (ou como você preferir).";
            lblInfo.Font = new Font("Segoe UI", 8.5f);
            lblInfo.ForeColor = Color.FromArgb(165, 185, 215);
            lblInfo.Location = new Point(14, 10);
            lblInfo.Size = new Size(625, 55);
            cardInfo.Controls.Add(lblInfo);

            this.Controls.Add(cardInfo);

            startY += 88;

            // 4. Rodapé com Botões de Ação
            btnRestoreDefaults = new Button();
            btnRestoreDefaults.Text = "🔄 Restaurar Padrões";
            btnRestoreDefaults.Location = new Point(25, startY);
            btnRestoreDefaults.Size = new Size(165, 36);
            btnRestoreDefaults.BackColor = Color.FromArgb(32, 36, 48);
            btnRestoreDefaults.ForeColor = ClrTextPrimary;
            btnRestoreDefaults.FlatStyle = FlatStyle.Flat;
            btnRestoreDefaults.FlatAppearance.BorderColor = ClrCardBorder;
            btnRestoreDefaults.Cursor = Cursors.Hand;
            btnRestoreDefaults.Click += (s, e) => RestoreDefaults();
            this.Controls.Add(btnRestoreDefaults);

            btnCancel = new Button();
            btnCancel.Text = "Cancelar";
            btnCancel.Location = new Point(415, startY);
            btnCancel.Size = new Size(125, 36);
            btnCancel.BackColor = Color.FromArgb(32, 36, 48);
            btnCancel.ForeColor = ClrTextPrimary;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.FlatAppearance.BorderColor = ClrCardBorder;
            btnCancel.Cursor = Cursors.Hand;
            btnCancel.Click += (s, e) => this.Close();
            this.Controls.Add(btnCancel);

            btnSave = new Button();
            btnSave.Text = "💾 Salvar e Aplicar";
            btnSave.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            btnSave.Location = new Point(550, startY);
            btnSave.Size = new Size(130, 36);
            btnSave.BackColor = Color.FromArgb(0, 150, 220);
            btnSave.ForeColor = Color.White;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Cursor = Cursors.Hand;
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);
        }

        private void CreateBindingRow(Panel parent, string labelText, int y, 
            out CheckBox chkCtrl, out CheckBox chkAlt, out CheckBox chkShift, out CheckBox chkWin, out ComboBox cmbKey)
        {
            Label lbl = new Label();
            lbl.Text = labelText;
            lbl.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            lbl.ForeColor = ClrTextPrimary;
            lbl.Location = new Point(15, y + 4);
            lbl.Size = new Size(180, 24);
            parent.Controls.Add(lbl);

            int x = 205;
            chkCtrl = CreateModifierCheckBox("Ctrl", x, y + 4);
            x += 60;
            chkAlt = CreateModifierCheckBox("Alt", x, y + 4);
            x += 60;
            chkShift = CreateModifierCheckBox("Shift", x, y + 4);
            x += 65;
            chkWin = CreateModifierCheckBox("Win", x, y + 4);
            x += 65;

            parent.Controls.Add(chkCtrl);
            parent.Controls.Add(chkAlt);
            parent.Controls.Add(chkShift);
            parent.Controls.Add(chkWin);

            Label lblPlus = new Label();
            lblPlus.Text = "+";
            lblPlus.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            lblPlus.ForeColor = ClrTextMuted;
            lblPlus.Location = new Point(x, y + 3);
            lblPlus.Size = new Size(16, 24);
            parent.Controls.Add(lblPlus);
            x += 20;

            cmbKey = new ComboBox();
            cmbKey.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbKey.Location = new Point(x, y + 2);
            cmbKey.Size = new Size(160, 28);
            cmbKey.BackColor = Color.FromArgb(28, 32, 44);
            cmbKey.ForeColor = Color.White;
            cmbKey.Font = new Font("Segoe UI", 9.5f);
            PopulateKeyComboBox(cmbKey);
            parent.Controls.Add(cmbKey);
        }

        private CheckBox CreateModifierCheckBox(string text, int x, int y)
        {
            CheckBox chk = new CheckBox();
            chk.Text = text;
            chk.Location = new Point(x, y);
            chk.AutoSize = true;
            chk.ForeColor = ClrTextPrimary;
            chk.Font = new Font("Segoe UI", 9f);
            chk.Cursor = Cursors.Hand;
            return chk;
        }

        private void PopulateKeyComboBox(ComboBox cmb)
        {
            List<HotkeyKeyItem> items = new List<HotkeyKeyItem>();

            // Teclado Numérico (NumPad)
            items.Add(new HotkeyKeyItem("Num / (Divide)", Keys.Divide));
            items.Add(new HotkeyKeyItem("Num * (Multiply)", Keys.Multiply));
            items.Add(new HotkeyKeyItem("Num + (Add)", Keys.Add));
            items.Add(new HotkeyKeyItem("Num - (Subtract)", Keys.Subtract));
            items.Add(new HotkeyKeyItem("Num . (Decimal)", Keys.Decimal));
            items.Add(new HotkeyKeyItem("Num 0", Keys.NumPad0));
            items.Add(new HotkeyKeyItem("Num 1", Keys.NumPad1));
            items.Add(new HotkeyKeyItem("Num 2", Keys.NumPad2));
            items.Add(new HotkeyKeyItem("Num 3", Keys.NumPad3));
            items.Add(new HotkeyKeyItem("Num 4", Keys.NumPad4));
            items.Add(new HotkeyKeyItem("Num 5", Keys.NumPad5));
            items.Add(new HotkeyKeyItem("Num 6", Keys.NumPad6));
            items.Add(new HotkeyKeyItem("Num 7", Keys.NumPad7));
            items.Add(new HotkeyKeyItem("Num 8", Keys.NumPad8));
            items.Add(new HotkeyKeyItem("Num 9", Keys.NumPad9));

            // Teclas de Função
            for (int f = 1; f <= 12; f++)
            {
                Keys fk = (Keys)Enum.Parse(typeof(Keys), "F" + f);
                items.Add(new HotkeyKeyItem("F" + f, fk));
            }

            // Teclas de Navegação
            items.Add(new HotkeyKeyItem("Espaço (Space)", Keys.Space));
            items.Add(new HotkeyKeyItem("Page Up", Keys.PageUp));
            items.Add(new HotkeyKeyItem("Page Down", Keys.PageDown));
            items.Add(new HotkeyKeyItem("Home", Keys.Home));
            items.Add(new HotkeyKeyItem("End", Keys.End));
            items.Add(new HotkeyKeyItem("Insert", Keys.Insert));
            items.Add(new HotkeyKeyItem("Delete", Keys.Delete));
            items.Add(new HotkeyKeyItem("Seta Cima", Keys.Up));
            items.Add(new HotkeyKeyItem("Seta Baixo", Keys.Down));
            items.Add(new HotkeyKeyItem("Seta Esquerda", Keys.Left));
            items.Add(new HotkeyKeyItem("Seta Direita", Keys.Right));

            // Letras A-Z
            for (char c = 'A'; c <= 'Z'; c++)
            {
                Keys k = (Keys)Enum.Parse(typeof(Keys), c.ToString());
                items.Add(new HotkeyKeyItem(c.ToString(), k));
            }

            // Números 0-9
            for (int d = 0; d <= 9; d++)
            {
                Keys dk = (Keys)Enum.Parse(typeof(Keys), "D" + d);
                items.Add(new HotkeyKeyItem(d.ToString(), dk));
            }

            cmb.DataSource = items;
        }

        private void SetComboBoxKey(ComboBox cmb, Keys key)
        {
            for (int i = 0; i < cmb.Items.Count; i++)
            {
                HotkeyKeyItem item = cmb.Items[i] as HotkeyKeyItem;
                if (item != null && item.Key == key)
                {
                    cmb.SelectedIndex = i;
                    return;
                }
            }
            if (cmb.Items.Count > 0) cmb.SelectedIndex = 0;
        }

        private Keys GetComboBoxKey(ComboBox cmb)
        {
            HotkeyKeyItem item = cmb.SelectedItem as HotkeyKeyItem;
            return item != null ? item.Key : Keys.None;
        }

        private void LoadCurrentBindings()
        {
            SettingsData cfg = AppSettings.Current;

            // Ciclo de Cores
            chkCycleCtrl.Checked = cfg.HotkeyCycleColor.Ctrl;
            chkCycleAlt.Checked = cfg.HotkeyCycleColor.Alt;
            chkCycleShift.Checked = cfg.HotkeyCycleColor.Shift;
            chkCycleWin.Checked = cfg.HotkeyCycleColor.Win;
            SetComboBoxKey(cmbCycleKey, cfg.HotkeyCycleColor.Key);

            // Ligar/Desligar
            chkPowerCtrl.Checked = cfg.HotkeyTogglePower.Ctrl;
            chkPowerAlt.Checked = cfg.HotkeyTogglePower.Alt;
            chkPowerShift.Checked = cfg.HotkeyTogglePower.Shift;
            chkPowerWin.Checked = cfg.HotkeyTogglePower.Win;
            SetComboBoxKey(cmbPowerKey, cfg.HotkeyTogglePower.Key);

            // Aumentar Brilho
            chkUpCtrl.Checked = cfg.HotkeyBrightnessUp.Ctrl;
            chkUpAlt.Checked = cfg.HotkeyBrightnessUp.Alt;
            chkUpShift.Checked = cfg.HotkeyBrightnessUp.Shift;
            chkUpWin.Checked = cfg.HotkeyBrightnessUp.Win;
            SetComboBoxKey(cmbUpKey, cfg.HotkeyBrightnessUp.Key);

            // Reduzir Brilho
            chkDownCtrl.Checked = cfg.HotkeyBrightnessDown.Ctrl;
            chkDownAlt.Checked = cfg.HotkeyBrightnessDown.Alt;
            chkDownShift.Checked = cfg.HotkeyBrightnessDown.Shift;
            chkDownWin.Checked = cfg.HotkeyBrightnessDown.Win;
            SetComboBoxKey(cmbDownKey, cfg.HotkeyBrightnessDown.Key);
        }

        private void RestoreDefaults()
        {
            // Padrão: Teclas NumPad padrão Clevo com Ctrl
            chkCycleCtrl.Checked = true;
            chkCycleAlt.Checked = false;
            chkCycleShift.Checked = false;
            chkCycleWin.Checked = false;
            SetComboBoxKey(cmbCycleKey, Keys.Divide); // Num /

            chkPowerCtrl.Checked = true;
            chkPowerAlt.Checked = false;
            chkPowerShift.Checked = false;
            chkPowerWin.Checked = false;
            SetComboBoxKey(cmbPowerKey, Keys.Multiply); // Num *

            chkUpCtrl.Checked = true;
            chkUpAlt.Checked = false;
            chkUpShift.Checked = false;
            chkUpWin.Checked = false;
            SetComboBoxKey(cmbUpKey, Keys.Add); // Num +

            chkDownCtrl.Checked = true;
            chkDownAlt.Checked = false;
            chkDownShift.Checked = false;
            chkDownWin.Checked = false;
            SetComboBoxKey(cmbDownKey, Keys.Subtract); // Num -
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            SettingsData cfg = AppSettings.Current;

            cfg.HotkeyCycleColor = new HotkeyBinding(
                chkCycleCtrl.Checked, chkCycleAlt.Checked, chkCycleShift.Checked, chkCycleWin.Checked, GetComboBoxKey(cmbCycleKey)
            );

            cfg.HotkeyTogglePower = new HotkeyBinding(
                chkPowerCtrl.Checked, chkPowerAlt.Checked, chkPowerShift.Checked, chkPowerWin.Checked, GetComboBoxKey(cmbPowerKey)
            );

            cfg.HotkeyBrightnessUp = new HotkeyBinding(
                chkUpCtrl.Checked, chkUpAlt.Checked, chkUpShift.Checked, chkUpWin.Checked, GetComboBoxKey(cmbUpKey)
            );

            cfg.HotkeyBrightnessDown = new HotkeyBinding(
                chkDownCtrl.Checked, chkDownAlt.Checked, chkDownShift.Checked, chkDownWin.Checked, GetComboBoxKey(cmbDownKey)
            );

            AppSettings.Save();
            this.DialogResult = DialogResult.OK;
            this.Close();
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
    }
}

