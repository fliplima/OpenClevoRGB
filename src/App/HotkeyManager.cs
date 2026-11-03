using System;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace OpenClevoRGB
{
    public class HotkeyManager : IDisposable
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public const int WM_HOTKEY = 0x0312;

        private const uint MOD_NONE      = 0x0000;
        private const uint MOD_ALT       = 0x0001;
        private const uint MOD_CONTROL   = 0x0002;
        private const uint MOD_SHIFT     = 0x0004;
        private const uint MOD_WIN       = 0x0008;
        private const uint MOD_NOREPEAT  = 0x4000;

        public const int ID_CYCLE_COLOR     = 9101;
        public const int ID_TOGGLE_POWER    = 9102;
        public const int ID_BRIGHTNESS_UP   = 9103;
        public const int ID_BRIGHTNESS_DOWN = 9104;

        private IntPtr currentHwnd = IntPtr.Zero;
        private bool isRegistered = false;

        public event Action OnCycleColor;
        public event Action OnTogglePower;
        public event Action OnBrightnessUp;
        public event Action OnBrightnessDown;

        private ClevoEventListener clevoListener;

        public HotkeyManager()
        {
            clevoListener = new ClevoEventListener();
            clevoListener.OnCycleColor += () => { if (OnCycleColor != null) OnCycleColor(); };
            clevoListener.OnTogglePower += () => { if (OnTogglePower != null) OnTogglePower(); };
            clevoListener.OnBrightnessUp += () => { if (OnBrightnessUp != null) OnBrightnessUp(); };
            clevoListener.OnBrightnessDown += () => { if (OnBrightnessDown != null) OnBrightnessDown(); };
        }

        public void Start(IntPtr hWnd, SettingsData settings)
        {
            this.currentHwnd = hWnd;
            RegisterAll(settings);
            clevoListener.Start();
        }

        public void RegisterAll(SettingsData settings)
        {
            UnregisterAll();

            if (currentHwnd == IntPtr.Zero || settings == null || !settings.EnableHotkeys)
            {
                return;
            }

            RegisterSingle(ID_CYCLE_COLOR, settings.HotkeyCycleColor);
            RegisterSingle(ID_TOGGLE_POWER, settings.HotkeyTogglePower);
            RegisterSingle(ID_BRIGHTNESS_UP, settings.HotkeyBrightnessUp);
            RegisterSingle(ID_BRIGHTNESS_DOWN, settings.HotkeyBrightnessDown);

            isRegistered = true;
        }

        private bool RegisterSingle(int id, HotkeyBinding binding)
        {
            if (binding == null || binding.Key == Keys.None) return false;

            uint mod = MOD_NOREPEAT;
            if (binding.Alt) mod |= MOD_ALT;
            if (binding.Ctrl) mod |= MOD_CONTROL;
            if (binding.Shift) mod |= MOD_SHIFT;
            if (binding.Win) mod |= MOD_WIN;

            bool ok = RegisterHotKey(currentHwnd, id, mod, (uint)binding.Key);
            if (!ok)
            {
                Debug.WriteLine(string.Format("Falha ao registrar HotKey ID {0} (Tecla {1}): erro {2}", 
                    id, binding.Key, Marshal.GetLastWin32Error()));
            }
            return ok;
        }

        public void UnregisterAll()
        {
            if (currentHwnd != IntPtr.Zero && isRegistered)
            {
                UnregisterHotKey(currentHwnd, ID_CYCLE_COLOR);
                UnregisterHotKey(currentHwnd, ID_TOGGLE_POWER);
                UnregisterHotKey(currentHwnd, ID_BRIGHTNESS_UP);
                UnregisterHotKey(currentHwnd, ID_BRIGHTNESS_DOWN);
                isRegistered = false;
            }
        }

        public bool ProcessMessage(ref Message m)
        {
            if (m.Msg == WM_HOTKEY)
            {
                int id = m.WParam.ToInt32();
                switch (id)
                {
                    case ID_CYCLE_COLOR:
                        if (OnCycleColor != null) OnCycleColor();
                        return true;

                    case ID_TOGGLE_POWER:
                        if (OnTogglePower != null) OnTogglePower();
                        return true;

                    case ID_BRIGHTNESS_UP:
                        if (OnBrightnessUp != null) OnBrightnessUp();
                        return true;

                    case ID_BRIGHTNESS_DOWN:
                        if (OnBrightnessDown != null) OnBrightnessDown();
                        return true;
                }
            }
            return false;
        }

        public void Dispose()
        {
            UnregisterAll();
            if (clevoListener != null)
            {
                clevoListener.Dispose();
                clevoListener = null;
            }
        }
    }

    /// <summary>
    /// Escuta eventos ACPI WMI em background para modelos Clevo que emitem CLEVO_EVENT
    /// </summary>
    public class ClevoEventListener : IDisposable
    {
        private ManagementEventWatcher watcher;
        public event Action OnCycleColor;
        public event Action OnTogglePower;
        public event Action OnBrightnessUp;
        public event Action OnBrightnessDown;

        public void Start()
        {
            try
            {
                ManagementScope scope = new ManagementScope(@"\\.\root\wmi");
                scope.Connect();
                EventQuery query = new EventQuery("SELECT * FROM CLEVO_EVENT");
                watcher = new ManagementEventWatcher(scope, query);
                watcher.EventArrived += Watcher_EventArrived;
                watcher.Start();
                Debug.WriteLine("ClevoEventListener iniciado com sucesso.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ClevoEventListener não iniciado (requer elevação ou hardware Clevo): " + ex.Message);
            }
        }

        private void Watcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            try
            {
                PropertyData prop = e.NewEvent.Properties["ULong"];
                if (prop != null && prop.Value != null)
                {
                    uint code = Convert.ToUInt32(prop.Value);
                    Debug.WriteLine(string.Format("Evento ACPI Clevo recebido: 0x{0:X}", code));

                    // Mapeamento de eventos comuns de teclado Clevo SCM/BIOS
                    // 0x45 / 0x46 / etc. dependendo do chassis
                    switch (code)
                    {
                        case 0x45: // Toggle Backlight em chassis específicos
                            if (OnTogglePower != null) OnTogglePower();
                            break;
                        case 0x46: // Cycle Color
                            if (OnCycleColor != null) OnCycleColor();
                            break;
                        case 0x47: // Brightness Up
                            if (OnBrightnessUp != null) OnBrightnessUp();
                            break;
                        case 0x48: // Brightness Down
                            if (OnBrightnessDown != null) OnBrightnessDown();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Erro ao processar CLEVO_EVENT: " + ex.Message);
            }
        }

        public void Dispose()
        {
            try
            {
                if (watcher != null)
                {
                    watcher.Stop();
                    watcher.Dispose();
                    watcher = null;
                }
            }
            catch {}
        }
    }
}

