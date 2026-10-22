using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace OpenClevoRGB
{
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

