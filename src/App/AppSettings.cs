using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace OpenClevoRGB
{
    public class HotkeyBinding
    {
        public bool Ctrl { get; set; }
        public bool Alt { get; set; }
        public bool Shift { get; set; }
        public bool Win { get; set; }
        public Keys Key { get; set; }

        public HotkeyBinding()
        {
            Ctrl = false;
            Alt = false;
            Shift = false;
            Win = false;
            Key = Keys.None;
        }

        public HotkeyBinding(bool ctrl, bool alt, bool shift, bool win, Keys key)
        {
            Ctrl = ctrl;
            Alt = alt;
            Shift = shift;
            Win = win;
            Key = key;
        }

        public string ToDisplayString()
        {
            List<string> parts = new List<string>();
            if (Ctrl) parts.Add("Ctrl");
            if (Alt) parts.Add("Alt");
            if (Shift) parts.Add("Shift");
            if (Win) parts.Add("Win");

            parts.Add(GetKeyFriendlyName(Key));
            return string.Join(" + ", parts.ToArray());
        }

        public static string GetKeyFriendlyName(Keys k)
        {
            switch (k)
            {
                case Keys.Divide: return "Num /";
                case Keys.Multiply: return "Num *";
                case Keys.Add: return "Num +";
                case Keys.Subtract: return "Num -";
                case Keys.Decimal: return "Num .";
                case Keys.NumPad0: return "Num 0";
                case Keys.NumPad1: return "Num 1";
                case Keys.NumPad2: return "Num 2";
                case Keys.NumPad3: return "Num 3";
                case Keys.NumPad4: return "Num 4";
                case Keys.NumPad5: return "Num 5";
                case Keys.NumPad6: return "Num 6";
                case Keys.NumPad7: return "Num 7";
                case Keys.NumPad8: return "Num 8";
                case Keys.NumPad9: return "Num 9";
                case Keys.Up: return "Seta Cima";
                case Keys.Down: return "Seta Baixo";
                case Keys.Left: return "Seta Esquerda";
                case Keys.Right: return "Seta Direita";
                case Keys.Space: return "Espaço";
                case Keys.None: return "(Nenhum)";
                default: return k.ToString();
            }
        }
    }

    public class SettingsData
    {
        public int ColorLeftArgb { get; set; }
        public int ColorMidArgb { get; set; }
        public int ColorRightArgb { get; set; }
        public int Brightness { get; set; }
        public int Effect { get; set; }
        public bool SyncZones { get; set; }
        public bool StartWithWindows { get; set; }
        public bool MinimizeToTray { get; set; }

        // Atalhos de Teclado
        public bool EnableHotkeys { get; set; }
        public HotkeyBinding HotkeyCycleColor { get; set; }
        public HotkeyBinding HotkeyTogglePower { get; set; }
        public HotkeyBinding HotkeyBrightnessUp { get; set; }
        public HotkeyBinding HotkeyBrightnessDown { get; set; }

        public SettingsData()
        {
            ColorLeftArgb = Color.FromArgb(0, 200, 255).ToArgb();
            ColorMidArgb = Color.FromArgb(0, 200, 255).ToArgb();
            ColorRightArgb = Color.FromArgb(0, 200, 255).ToArgb();
            Brightness = 4;
            Effect = 0;
            SyncZones = true;
            StartWithWindows = false;
            MinimizeToTray = true;

            // Padrões dos Atalhos: Teclas NumPad padrão Clevo com Ctrl
            EnableHotkeys = true;
            HotkeyCycleColor     = new HotkeyBinding(true, false, false, false, Keys.Divide);   // Ctrl + Num /
            HotkeyTogglePower    = new HotkeyBinding(true, false, false, false, Keys.Multiply); // Ctrl + Num *
            HotkeyBrightnessUp   = new HotkeyBinding(true, false, false, false, Keys.Add);      // Ctrl + Num +
            HotkeyBrightnessDown = new HotkeyBinding(true, false, false, false, Keys.Subtract); // Ctrl + Num -
        }
    }

    public static class AppSettings
    {
        private static readonly string FolderPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "OpenClevoRGB"
        );
        private static readonly string FilePath = Path.Combine(FolderPath, "settings.xml");

        public static SettingsData Current = new SettingsData();

        public static void Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(SettingsData));
                    using (StreamReader reader = new StreamReader(FilePath))
                    {
                        Current = (SettingsData)serializer.Deserialize(reader);
                    }
                }
            }
            catch
            {
                Current = new SettingsData();
            }
        }

        public static void Save()
        {
            try
            {
                if (!Directory.Exists(FolderPath))
                {
                    Directory.CreateDirectory(FolderPath);
                }

                XmlSerializer serializer = new XmlSerializer(typeof(SettingsData));
                using (StreamWriter writer = new StreamWriter(FilePath))
                {
                    serializer.Serialize(writer, Current);
                }
            }
            catch
            {
                // Silencioso em caso de falha de gravação
            }
        }

        public const string TaskName = "OpenClevoRGB";

        public static void SetStartWithWindows(bool enable, string exePath)
        {
            // 1. Limpa resíduo de chaves antigas, se existirem
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (key != null)
                    {
                        if (key.GetValue("OpenClevoRGB") != null) key.DeleteValue("OpenClevoRGB", false);
                        if (key.GetValue("AvellKeyboardRGB") != null) key.DeleteValue("AvellKeyboardRGB", false);
                    }
                }
            }
            catch {}

            // 2. Configura a Tarefa Agendada com privilégios de Administrador no Windows
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "schtasks.exe";
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;

                if (enable)
                {
                    // Inicia no logon do Windows com privilégios elevados sem bloquear no UAC
                    psi.Arguments = string.Format("/create /tn \"{0}\" /tr \"\\\"{1}\\\" -silent\" /sc onlogon /rl highest /f", TaskName, exePath);
                }
                else
                {
                    psi.Arguments = string.Format("/delete /tn \"{0}\" /f", TaskName);
                }

                using (Process proc = Process.Start(psi))
                {
                    proc.WaitForExit(4000);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Erro ao configurar schtasks: " + ex.Message);
            }
        }
    }
}

