using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace OpenClevoRGB.Tests
{
    internal static class Assert
    {
        public static void IsTrue(bool condition, string message)
        {
            if (!condition) throw new Exception("Assert.IsTrue falhou: " + message);
        }

        public static void IsFalse(bool condition, string message)
        {
            if (condition) throw new Exception("Assert.IsFalse falhou: " + message);
        }

        public static void IsNotNull(object obj, string message)
        {
            if (obj == null) throw new Exception("Assert.IsNotNull falhou: " + message);
        }

        public static void AreEqual<T>(T expected, T actual, string message)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
            {
                throw new Exception(string.Format(
                    "Assert.AreEqual falhou ({0}): esperado <{1}>, obtido <{2}>",
                    message, expected, actual));
            }
        }
    }

    internal static class Win32TestHelper
    {
        public const uint MOD_ALT       = 0x0001;
        public const uint MOD_CONTROL   = 0x0002;
        public const uint MOD_SHIFT     = 0x0004;
        public const uint MOD_NOREPEAT  = 0x4000;
        public const int ERROR_HOTKEY_ALREADY_REGISTERED = 1409;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// Verifica se uma combinação já está registrada no Windows tentando registrá-la temporariamente.
        /// Retorna true se o Windows responder ERROR_HOTKEY_ALREADY_REGISTERED (1409).
        /// </summary>
        public static bool IsHotkeyRegisteredInWindows(uint modifiers, Keys key)
        {
            const int probeId = 0x7F01;
            bool registeredByProbe = RegisterHotKey(IntPtr.Zero, probeId, modifiers | MOD_NOREPEAT, (uint)key);
            if (registeredByProbe)
            {
                UnregisterHotKey(IntPtr.Zero, probeId);
                return false;
            }
            int err = Marshal.GetLastWin32Error();
            return err == ERROR_HOTKEY_ALREADY_REGISTERED;
        }
    }

    internal static class TestRunner
    {
        private static string tempTestDir;
        private static string originalSettingsPath;

        [STAThread]
        static int Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MainForm.SuppressAutoUpdateCheckForTests = true;

            tempTestDir = Path.Combine(Path.GetTempPath(), "OpenClevoRGB_Tests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempTestDir);
            originalSettingsPath = AppSettings.FilePath;

            var tests = new List<KeyValuePair<string, Action>>
            {
                new KeyValuePair<string, Action>("SettingsData_DefaultValues", Test_SettingsData_DefaultValues),
                new KeyValuePair<string, Action>("HotkeyBinding_DisplayAndFriendlyNames", Test_HotkeyBinding_DisplayAndFriendlyNames),
                new KeyValuePair<string, Action>("AppSettings_SaveAndLoad_RoundTrip", Test_AppSettings_SaveAndLoad_RoundTrip),
                new KeyValuePair<string, Action>("AppSettings_LoadCorruptedXml_FallsBackToDefaults", Test_AppSettings_LoadCorruptedXml_FallsBackToDefaults),
                new KeyValuePair<string, Action>("HotkeyManager_ProcessMessage_DispatchesAllEvents", Test_HotkeyManager_ProcessMessage_DispatchesAllEvents),
                new KeyValuePair<string, Action>("HotkeyManager_Win32Register_Unregister_And_ReRegister", Test_HotkeyManager_Win32Register_Unregister_And_ReRegister),
                new KeyValuePair<string, Action>("HotkeyManager_Disabled_DoesNotRegister", Test_HotkeyManager_Disabled_DoesNotRegister),
                new KeyValuePair<string, Action>("MainForm_SilentStart_DoesNotShowWindowOrCreateHandleEarly", Test_MainForm_SilentStart_DoesNotShowWindowOrCreateHandleEarly),
                new KeyValuePair<string, Action>("MainForm_SilentStart_HotkeysRemainRegisteredAndWork", Test_MainForm_SilentStart_HotkeysRemainRegisteredAndWork),
                new KeyValuePair<string, Action>("MainForm_SilentStart_ShowWindow_RendersControlsCorrectly", Test_MainForm_SilentStart_ShowWindow_RendersControlsCorrectly),
                new KeyValuePair<string, Action>("MainForm_NormalStart_ShowsWindowVisible", Test_MainForm_NormalStart_ShowsWindowVisible),
                new KeyValuePair<string, Action>("MainForm_CloseWithMinimizeToTray_HidesInsteadOfClosing", Test_MainForm_CloseWithMinimizeToTray_HidesInsteadOfClosing),
                new KeyValuePair<string, Action>("HotkeySettingsForm_InitializesWithCurrentSettings", Test_HotkeySettingsForm_InitializesWithCurrentSettings),
                new KeyValuePair<string, Action>("ClevoController_SafeBehaviorWithoutHardware", Test_ClevoController_SafeBehaviorWithoutHardware),
                new KeyValuePair<string, Action>("UpdateChecker_ParseVersion_And_Comparison", Test_UpdateChecker_ParseVersion_And_Comparison),
                new KeyValuePair<string, Action>("UpdateChecker_ParseGitHubReleaseJson_ExtractsInstallerAndNotes", Test_UpdateChecker_ParseGitHubReleaseJson_ExtractsInstallerAndNotes),
                new KeyValuePair<string, Action>("UpdateChecker_IsTrustedDownloadUrl_ValidatesGitHubHttpsOnly", Test_UpdateChecker_IsTrustedDownloadUrl_ValidatesGitHubHttpsOnly),
                new KeyValuePair<string, Action>("UpdateChecker_CheckForUpdate_MockHttp_DetectsNewAndUpToDate", Test_UpdateChecker_CheckForUpdate_MockHttp_DetectsNewAndUpToDate)
            };

            Console.WriteLine("========================================================");
            Console.WriteLine("  OpenClevoRGB - Suite de Testes Automatizados");
            Console.WriteLine("========================================================");
            Console.WriteLine();

            int passed = 0;
            int failed = 0;
            Stopwatch totalWatch = Stopwatch.StartNew();

            try
            {
                foreach (var test in tests)
                {
                    // Isola settings.xml em um arquivo novo por teste
                    AppSettings.FilePath = Path.Combine(tempTestDir, test.Key + "_settings.xml");
                    AppSettings.Current = new SettingsData();

                    Stopwatch sw = Stopwatch.StartNew();
                    try
                    {
                        test.Value();
                        sw.Stop();
                        passed++;
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("[PASS] ");
                        Console.ResetColor();
                        Console.WriteLine("{0} ({1} ms)", test.Key, sw.ElapsedMilliseconds);
                    }
                    catch (Exception ex)
                    {
                        sw.Stop();
                        failed++;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("[FAIL] ");
                        Console.ResetColor();
                        Console.WriteLine("{0} ({1} ms)", test.Key, sw.ElapsedMilliseconds);
                        Console.WriteLine("       " + ex.Message);
                    }
                }
            }
            finally
            {
                AppSettings.FilePath = originalSettingsPath;
                try
                {
                    if (Directory.Exists(tempTestDir))
                    {
                        Directory.Delete(tempTestDir, true);
                    }
                }
                catch { }
            }

            totalWatch.Stop();
            Console.WriteLine();
            Console.WriteLine("========================================================");
            Console.WriteLine("  Resultado: {0} passaram, {1} falharam (Total: {2} em {3} ms)",
                passed, failed, passed + failed, totalWatch.ElapsedMilliseconds);
            Console.WriteLine("========================================================");

            return failed == 0 ? 0 : 1;
        }

        private static SettingsData CreateSafeTestHotkeySettings()
        {
            // Usa Ctrl + Alt + Shift + F20..F23 para nunca conflitar com o app real ou atalhos do Windows
            SettingsData s = new SettingsData();
            s.EnableHotkeys = true;
            s.Brightness = 3;
            s.HotkeyCycleColor     = new HotkeyBinding(true, true, true, false, Keys.F20);
            s.HotkeyTogglePower    = new HotkeyBinding(true, true, true, false, Keys.F21);
            s.HotkeyBrightnessUp   = new HotkeyBinding(true, true, true, false, Keys.F22);
            s.HotkeyBrightnessDown = new HotkeyBinding(true, true, true, false, Keys.F23);
            return s;
        }

        // ============================================================
        // 1. Testes de AppSettings & HotkeyBinding
        // ============================================================

        private static void Test_SettingsData_DefaultValues()
        {
            SettingsData s = new SettingsData();
            Assert.AreEqual(4, s.Brightness, "Brilho padrao");
            Assert.AreEqual(0, s.Effect, "Efeito padrao");
            Assert.IsTrue(s.SyncZones, "SyncZones padrao");
            Assert.IsFalse(s.StartWithWindows, "StartWithWindows padrao");
            Assert.IsTrue(s.MinimizeToTray, "MinimizeToTray padrao");
            Assert.IsTrue(s.AutoCheckUpdates, "AutoCheckUpdates padrao");
            Assert.IsTrue(s.EnableHotkeys, "EnableHotkeys padrao");

            Assert.AreEqual(Keys.Divide, s.HotkeyCycleColor.Key, "HotkeyCycleColor");
            Assert.IsTrue(s.HotkeyCycleColor.Ctrl, "HotkeyCycleColor Ctrl");
            Assert.AreEqual(Keys.Multiply, s.HotkeyTogglePower.Key, "HotkeyTogglePower");
            Assert.AreEqual(Keys.Add, s.HotkeyBrightnessUp.Key, "HotkeyBrightnessUp");
            Assert.AreEqual(Keys.Subtract, s.HotkeyBrightnessDown.Key, "HotkeyBrightnessDown");
        }

        private static void Test_HotkeyBinding_DisplayAndFriendlyNames()
        {
            var b1 = new HotkeyBinding(true, false, false, false, Keys.Divide);
            Assert.AreEqual("Ctrl + Num /", b1.ToDisplayString(), "Ctrl + Num /");

            var b2 = new HotkeyBinding(true, true, true, true, Keys.F12);
            Assert.AreEqual("Ctrl + Alt + Shift + Win + F12", b2.ToDisplayString(), "Todos modificadores");

            var b3 = new HotkeyBinding(false, false, false, false, Keys.Space);
            Assert.AreEqual("Espaço", b3.ToDisplayString(), "Espaco sem modificadores");

            var b4 = new HotkeyBinding();
            Assert.AreEqual("(Nenhum)", b4.ToDisplayString(), "Binding vazio");

            Assert.AreEqual("Num *", HotkeyBinding.GetKeyFriendlyName(Keys.Multiply), "Multiply");
            Assert.AreEqual("Num +", HotkeyBinding.GetKeyFriendlyName(Keys.Add), "Add");
            Assert.AreEqual("Num -", HotkeyBinding.GetKeyFriendlyName(Keys.Subtract), "Subtract");
            Assert.AreEqual("Num .", HotkeyBinding.GetKeyFriendlyName(Keys.Decimal), "Decimal");
            Assert.AreEqual("Num 0", HotkeyBinding.GetKeyFriendlyName(Keys.NumPad0), "NumPad0");
            Assert.AreEqual("Num 9", HotkeyBinding.GetKeyFriendlyName(Keys.NumPad9), "NumPad9");
            Assert.AreEqual("Seta Cima", HotkeyBinding.GetKeyFriendlyName(Keys.Up), "Up");
            Assert.AreEqual("Seta Direita", HotkeyBinding.GetKeyFriendlyName(Keys.Right), "Right");
        }

        private static void Test_AppSettings_SaveAndLoad_RoundTrip()
        {
            AppSettings.Current = new SettingsData
            {
                ColorLeftArgb = Color.FromArgb(255, 10, 20).ToArgb(),
                ColorMidArgb = Color.FromArgb(30, 200, 40).ToArgb(),
                ColorRightArgb = Color.FromArgb(50, 60, 240).ToArgb(),
                Brightness = 2,
                Effect = 3,
                SyncZones = false,
                StartWithWindows = true,
                MinimizeToTray = false,
                AutoCheckUpdates = false,
                EnableHotkeys = true,
                HotkeyCycleColor = new HotkeyBinding(false, true, true, false, Keys.F9),
                HotkeyTogglePower = new HotkeyBinding(true, false, false, false, Keys.F10),
                HotkeyBrightnessUp = new HotkeyBinding(true, true, false, false, Keys.Up),
                HotkeyBrightnessDown = new HotkeyBinding(true, true, false, false, Keys.Down)
            };

            AppSettings.Save();
            Assert.IsTrue(File.Exists(AppSettings.FilePath), "Arquivo settings.xml deve ter sido salvo");

            AppSettings.Current = new SettingsData();
            AppSettings.Load();

            Assert.AreEqual(Color.FromArgb(255, 10, 20).ToArgb(), AppSettings.Current.ColorLeftArgb, "ColorLeftArgb");
            Assert.AreEqual(Color.FromArgb(30, 200, 40).ToArgb(), AppSettings.Current.ColorMidArgb, "ColorMidArgb");
            Assert.AreEqual(Color.FromArgb(50, 60, 240).ToArgb(), AppSettings.Current.ColorRightArgb, "ColorRightArgb");
            Assert.AreEqual(2, AppSettings.Current.Brightness, "Brightness");
            Assert.AreEqual(3, AppSettings.Current.Effect, "Effect");
            Assert.IsFalse(AppSettings.Current.SyncZones, "SyncZones");
            Assert.IsTrue(AppSettings.Current.StartWithWindows, "StartWithWindows");
            Assert.IsFalse(AppSettings.Current.MinimizeToTray, "MinimizeToTray");
            Assert.IsFalse(AppSettings.Current.AutoCheckUpdates, "AutoCheckUpdates desabilitado deve persistir");
            Assert.AreEqual("Alt + Shift + F9", AppSettings.Current.HotkeyCycleColor.ToDisplayString(), "HotkeyCycleColor");
            Assert.AreEqual("Ctrl + Alt + Seta Baixo", AppSettings.Current.HotkeyBrightnessDown.ToDisplayString(), "HotkeyBrightnessDown");
        }

        private static void Test_AppSettings_LoadCorruptedXml_FallsBackToDefaults()
        {
            File.WriteAllText(AppSettings.FilePath, "<SettingsData><BrokenXml");
            AppSettings.Current.Brightness = 1;

            AppSettings.Load();

            Assert.AreEqual(4, AppSettings.Current.Brightness, "XML corrompido deve restaurar brilho padrao");
            Assert.IsTrue(AppSettings.Current.EnableHotkeys, "XML corrompido deve restaurar EnableHotkeys padrao");
        }

        // ============================================================
        // 2. Testes de HotkeyManager
        // ============================================================

        private static void Test_HotkeyManager_ProcessMessage_DispatchesAllEvents()
        {
            int cycleCount = 0, powerCount = 0, upCount = 0, downCount = 0;

            using (HotkeyManager hm = new HotkeyManager())
            {
                hm.OnCycleColor += () => cycleCount++;
                hm.OnTogglePower += () => powerCount++;
                hm.OnBrightnessUp += () => upCount++;
                hm.OnBrightnessDown += () => downCount++;

                Message mCycle = Message.Create(IntPtr.Zero, HotkeyManager.WM_HOTKEY, (IntPtr)HotkeyManager.ID_CYCLE_COLOR, IntPtr.Zero);
                Message mPower = Message.Create(IntPtr.Zero, HotkeyManager.WM_HOTKEY, (IntPtr)HotkeyManager.ID_TOGGLE_POWER, IntPtr.Zero);
                Message mUp    = Message.Create(IntPtr.Zero, HotkeyManager.WM_HOTKEY, (IntPtr)HotkeyManager.ID_BRIGHTNESS_UP, IntPtr.Zero);
                Message mDown  = Message.Create(IntPtr.Zero, HotkeyManager.WM_HOTKEY, (IntPtr)HotkeyManager.ID_BRIGHTNESS_DOWN, IntPtr.Zero);
                Message mOther = Message.Create(IntPtr.Zero, 0x000F, IntPtr.Zero, IntPtr.Zero); // WM_PAINT
                Message mUnkId = Message.Create(IntPtr.Zero, HotkeyManager.WM_HOTKEY, (IntPtr)9999, IntPtr.Zero);

                Assert.IsTrue(hm.ProcessMessage(ref mCycle), "ProcessMessage CycleColor");
                Assert.IsTrue(hm.ProcessMessage(ref mPower), "ProcessMessage TogglePower");
                Assert.IsTrue(hm.ProcessMessage(ref mUp), "ProcessMessage BrightnessUp");
                Assert.IsTrue(hm.ProcessMessage(ref mDown), "ProcessMessage BrightnessDown");
                Assert.IsFalse(hm.ProcessMessage(ref mOther), "ProcessMessage deve ignorar WM_PAINT");
                Assert.IsFalse(hm.ProcessMessage(ref mUnkId), "ProcessMessage deve ignorar ID desconhecido");
            }

            Assert.AreEqual(1, cycleCount, "cycleCount");
            Assert.AreEqual(1, powerCount, "powerCount");
            Assert.AreEqual(1, upCount, "upCount");
            Assert.AreEqual(1, downCount, "downCount");
        }

        private static void Test_HotkeyManager_Win32Register_Unregister_And_ReRegister()
        {
            SettingsData settings = CreateSafeTestHotkeySettings();
            uint mods = Win32TestHelper.MOD_CONTROL | Win32TestHelper.MOD_ALT | Win32TestHelper.MOD_SHIFT;

            using (Form dummy1 = new Form())
            using (Form dummy2 = new Form())
            using (HotkeyManager hm = new HotkeyManager())
            {
                IntPtr h1 = dummy1.Handle;
                hm.Start(h1, settings);

                Assert.IsTrue(Win32TestHelper.IsHotkeyRegisteredInWindows(mods, Keys.F20), "F20 deve estar registrada no Windows");
                Assert.IsTrue(Win32TestHelper.IsHotkeyRegisteredInWindows(mods, Keys.F21), "F21 deve estar registrada no Windows");
                Assert.IsTrue(Win32TestHelper.IsHotkeyRegisteredInWindows(mods, Keys.F22), "F22 deve estar registrada no Windows");
                Assert.IsTrue(Win32TestHelper.IsHotkeyRegisteredInWindows(mods, Keys.F23), "F23 deve estar registrada no Windows");

                // Simula recriacao de handle (Unregister + novo Start em outro HWND)
                hm.UnregisterAll();
                Assert.IsFalse(Win32TestHelper.IsHotkeyRegisteredInWindows(mods, Keys.F20), "F20 deve ser liberada apos UnregisterAll");

                IntPtr h2 = dummy2.Handle;
                hm.Start(h2, settings);
                Assert.IsTrue(Win32TestHelper.IsHotkeyRegisteredInWindows(mods, Keys.F20), "F20 deve ser re-registrada no novo HWND");

                // Dispose idempotente
                hm.Dispose();
                hm.Dispose();
                Assert.IsFalse(Win32TestHelper.IsHotkeyRegisteredInWindows(mods, Keys.F20), "F20 deve ser liberada apos Dispose");
                Assert.IsFalse(Win32TestHelper.IsHotkeyRegisteredInWindows(mods, Keys.F23), "F23 deve ser liberada apos Dispose");
            }
        }

        private static void Test_HotkeyManager_Disabled_DoesNotRegister()
        {
            SettingsData settings = CreateSafeTestHotkeySettings();
            settings.EnableHotkeys = false;
            uint mods = Win32TestHelper.MOD_CONTROL | Win32TestHelper.MOD_ALT | Win32TestHelper.MOD_SHIFT;

            using (Form dummy = new Form())
            using (HotkeyManager hm = new HotkeyManager())
            {
                hm.Start(dummy.Handle, settings);
                Assert.IsFalse(Win32TestHelper.IsHotkeyRegisteredInWindows(mods, Keys.F20), "Com EnableHotkeys=false nao deve registrar F20");
                Assert.IsFalse(Win32TestHelper.IsHotkeyRegisteredInWindows(mods, Keys.F21), "Com EnableHotkeys=false nao deve registrar F21");
            }
        }

        // ============================================================
        // 3. Testes de Regressão do MainForm (-silent, UI, Hotkeys, Tray)
        // ============================================================

        private static void Test_MainForm_SilentStart_DoesNotShowWindowOrCreateHandleEarly()
        {
            AppSettings.Current = CreateSafeTestHotkeySettings();
            AppSettings.Save();

            using (MainForm form = new MainForm(true))
            {
                // Regressao: o construtor NAO pode forcar a criacao do handle antes de SetVisibleCore
                Assert.IsFalse(form.IsHandleCreated, "Construtor nao deve criar HWND prematuramente");

                // Simula o que Application.Run(mainForm) faz: chama Show() -> SetVisibleCore(true)
                form.Show();
                Application.DoEvents();

                Assert.IsTrue(form.IsHandleCreated, "SetVisibleCore deve criar o HWND oculto");
                Assert.IsFalse(form.Visible, "Form deve permanecer Visible=false no modo -silent");
                Assert.IsFalse(Win32TestHelper.IsWindowVisible(form.Handle), "Win32 IsWindowVisible deve ser false (sem janela preta!)");
            }
        }

        private static void Test_MainForm_SilentStart_HotkeysRemainRegisteredAndWork()
        {
            AppSettings.Current = CreateSafeTestHotkeySettings();
            AppSettings.Save();
            uint mods = Win32TestHelper.MOD_CONTROL | Win32TestHelper.MOD_ALT | Win32TestHelper.MOD_SHIFT;

            using (MainForm form = new MainForm(true))
            {
                form.Show();
                Application.DoEvents();

                // Regressao: todas as 4 hotkeys devem permanecer registradas no handle oculto
                Assert.IsTrue(Win32TestHelper.IsHotkeyRegisteredInWindows(mods, Keys.F20), "Hotkey CycleColor deve estar registrada em -silent");
                Assert.IsTrue(Win32TestHelper.IsHotkeyRegisteredInWindows(mods, Keys.F21), "Hotkey TogglePower deve estar registrada em -silent");
                Assert.IsTrue(Win32TestHelper.IsHotkeyRegisteredInWindows(mods, Keys.F22), "Hotkey BrightnessUp deve estar registrada em -silent");
                Assert.IsTrue(Win32TestHelper.IsHotkeyRegisteredInWindows(mods, Keys.F23), "Hotkey BrightnessDown deve estar registrada em -silent");

                // Envia WM_HOTKEY diretamente para o HWND do MainForm (como o Windows faz)
                int initialBrightness = AppSettings.Current.Brightness; // 3
                Win32TestHelper.SendMessage(form.Handle, HotkeyManager.WM_HOTKEY, (IntPtr)HotkeyManager.ID_BRIGHTNESS_DOWN, IntPtr.Zero);
                Application.DoEvents();
                Assert.AreEqual(initialBrightness - 1, AppSettings.Current.Brightness, "WM_HOTKEY BrightnessDown deve reduzir brilho");

                Win32TestHelper.SendMessage(form.Handle, HotkeyManager.WM_HOTKEY, (IntPtr)HotkeyManager.ID_BRIGHTNESS_UP, IntPtr.Zero);
                Application.DoEvents();
                Assert.AreEqual(initialBrightness, AppSettings.Current.Brightness, "WM_HOTKEY BrightnessUp deve aumentar brilho");

                int colorBefore = AppSettings.Current.ColorLeftArgb;
                Win32TestHelper.SendMessage(form.Handle, HotkeyManager.WM_HOTKEY, (IntPtr)HotkeyManager.ID_CYCLE_COLOR, IntPtr.Zero);
                Application.DoEvents();
                Assert.IsTrue(AppSettings.Current.ColorLeftArgb != colorBefore, "WM_HOTKEY CycleColor deve alterar a cor");
            }

            // Apos Dispose, nenhuma hotkey deve vazar no Windows
            Assert.IsFalse(Win32TestHelper.IsHotkeyRegisteredInWindows(mods, Keys.F20), "F20 deve ser desregistrada apos Dispose do MainForm");
            Assert.IsFalse(Win32TestHelper.IsHotkeyRegisteredInWindows(mods, Keys.F23), "F23 deve ser desregistrada apos Dispose do MainForm");
        }

        private static void Test_MainForm_SilentStart_ShowWindow_RendersControlsCorrectly()
        {
            AppSettings.Current = CreateSafeTestHotkeySettings();
            AppSettings.Save();

            using (MainForm form = new MainForm(true))
            {
                form.Show();
                Application.DoEvents();
                Assert.IsFalse(form.Visible, "Inicialmente oculto em -silent");

                // Simula o usuario clicar duas vezes no icone da bandeja
                form.ShowWindow();
                Application.DoEvents();

                Assert.IsTrue(form.Visible, "ShowWindow deve tornar o form visivel");
                Assert.IsTrue(Win32TestHelper.IsWindowVisible(form.Handle), "Win32 IsWindowVisible deve ser true apos ShowWindow");
                Assert.AreEqual(FormWindowState.Normal, form.WindowState, "WindowState deve ser Normal");
                Assert.IsTrue(form.ShowInTaskbar, "ShowInTaskbar deve ser true apos abrir");

                // Verifica se os paineis/cards principais existem e estao visiveis com dimensoes validas
                Assert.IsTrue(form.Controls.Count >= 6, "MainForm deve ter pelo menos 6 containers principais");
                foreach (Control c in form.Controls)
                {
                    Assert.IsTrue(c.Visible, "Controle raiz deve estar visivel: " + c.GetType().Name);
                    Assert.IsTrue(c.Width > 0 && c.Height > 0, "Controle raiz deve ter dimensoes > 0");
                }
            }
        }

        private static void Test_MainForm_NormalStart_ShowsWindowVisible()
        {
            AppSettings.Current = CreateSafeTestHotkeySettings();
            AppSettings.Save();

            using (MainForm form = new MainForm(false))
            {
                form.Show();
                Application.DoEvents();

                Assert.IsTrue(form.Visible, "Modo normal deve exibir a janela");
                Assert.IsTrue(Win32TestHelper.IsWindowVisible(form.Handle), "Win32 IsWindowVisible deve ser true no modo normal");
                Assert.AreEqual(FormWindowState.Normal, form.WindowState, "WindowState deve ser Normal");
            }
        }

        private static void Test_MainForm_CloseWithMinimizeToTray_HidesInsteadOfClosing()
        {
            AppSettings.Current = CreateSafeTestHotkeySettings();
            AppSettings.Current.MinimizeToTray = true;
            AppSettings.Save();

            using (MainForm form = new MainForm(false))
            {
                form.Show();
                Application.DoEvents();
                Assert.IsTrue(form.Visible, "Janela inicialmente visivel");

                // Simula clique real no botao X (WM_SYSCOMMAND + SC_CLOSE -> CloseReason.UserClosing)
                const int WM_SYSCOMMAND = 0x0112;
                const int SC_CLOSE = 0xF060;
                Win32TestHelper.SendMessage(form.Handle, WM_SYSCOMMAND, (IntPtr)SC_CLOSE, IntPtr.Zero);
                Application.DoEvents();

                Assert.IsFalse(form.IsDisposed, "Form nao deve ser descartado quando MinimizeToTray=true");
                Assert.IsFalse(form.Visible, "Form deve ficar oculto na bandeja ao fechar");
            }
        }

        // ============================================================
        // 4. Testes de HotkeySettingsForm e ClevoController
        // ============================================================

        private static void Test_HotkeySettingsForm_InitializesWithCurrentSettings()
        {
            AppSettings.Current = CreateSafeTestHotkeySettings();

            using (HotkeySettingsForm dlg = new HotkeySettingsForm())
            {
                Assert.IsTrue(dlg.Controls.Count > 0, "HotkeySettingsForm deve conter controles");
                Assert.IsTrue(dlg.Size.Width >= 700 && dlg.Size.Height >= 500, "Dimensoes da tela de atalhos");
            }
        }

        private static void Test_ClevoController_SafeBehaviorWithoutHardware()
        {
            ClevoController ctrl = new ClevoController();
            Assert.IsFalse(ctrl.IsInitialized, "Novo controller comeca nao inicializado");

            bool installed = ctrl.InstallDriverMetadata(Path.Combine(tempTestDir, "arquivo_inexistente.dll"));
            Assert.IsFalse(installed, "InstallDriverMetadata com arquivo inexistente deve retornar false");

            // Nao deve lancar excecao para valores fora da faixa
            ctrl.SetBrightness(-5);
            ctrl.SetBrightness(99);
            ctrl.SetEffect((KeyboardEffect)99);
        }

        // ============================================================
        // 5. Testes de UpdateChecker (Atualizacao Automatica)
        // ============================================================

        private static void Test_UpdateChecker_ParseVersion_And_Comparison()
        {
            Version v140 = UpdateChecker.ParseVersion("v1.4.0");
            Version v1400 = new Version(1, 4, 0, 0);
            Version v150 = UpdateChecker.ParseVersion("v1.5.0");
            Version v150Beta = UpdateChecker.ParseVersion("v1.5.0-beta.1");
            Version v139 = UpdateChecker.ParseVersion("1.3.9");

            Assert.AreEqual(new Version(1, 4, 0, 0), v140, "v1.4.0 normalizado");
            Assert.AreEqual(new Version(1, 5, 0, 0), v150Beta, "v1.5.0-beta.1 normalizado");
            Assert.IsFalse(UpdateChecker.IsNewerVersion(v140, v1400), "1.4.0 nao e mais nova que 1.4.0.0");
            Assert.IsTrue(UpdateChecker.IsNewerVersion(v150, v1400), "1.5.0 e mais nova que 1.4.0.0");
            Assert.IsFalse(UpdateChecker.IsNewerVersion(v139, v1400), "1.3.9 nao e mais nova que 1.4.0.0");
            Assert.AreEqual("v1.4.0", UpdateChecker.FormatVersion(v1400), "FormatVersion 1.4.0.0");
        }

        private static void Test_UpdateChecker_ParseGitHubReleaseJson_ExtractsInstallerAndNotes()
        {
            string sampleJson =
                "{" +
                "  \"author\": { \"html_url\": \"https://github.com/fliplima\" }," +
                "  \"html_url\": \"https://github.com/fliplima/OpenClevoRGB/releases/tag/v1.5.0\"," +
                "  \"tag_name\": \"v1.5.0\"," +
                "  \"body\": \"Corre\\u00e7\\u00e3o do modo \\\"-silent\\\"\\r\\n- Busca autom\\u00e1tica\"," +
                "  \"assets\": [" +
                "    { \"name\": \"OpenClevoRGB.exe\", \"browser_download_url\": \"https://github.com/fliplima/OpenClevoRGB/releases/download/v1.5.0/OpenClevoRGB.exe\" }," +
                "    { \"name\": \"Instalador_OpenClevoRGB.exe\", \"browser_download_url\": \"https://github.com/fliplima/OpenClevoRGB/releases/download/v1.5.0/Instalador_OpenClevoRGB.exe\" }" +
                "  ]" +
                "}";

            ReleaseInfo info = UpdateChecker.ParseGitHubReleaseJson(sampleJson);
            Assert.IsNotNull(info, "ReleaseInfo nao deve ser nulo");
            Assert.AreEqual("v1.5.0", info.TagName, "TagName");
            Assert.AreEqual(new Version(1, 5, 0, 0), info.ParsedVersion, "ParsedVersion");
            Assert.AreEqual("https://github.com/fliplima/OpenClevoRGB/releases/tag/v1.5.0", info.HtmlUrl, "HtmlUrl da release");
            Assert.AreEqual("Instalador_OpenClevoRGB.exe", info.InstallerFileName, "Deve priorizar o Instalador");
            Assert.AreEqual("https://github.com/fliplima/OpenClevoRGB/releases/download/v1.5.0/Instalador_OpenClevoRGB.exe", info.InstallerDownloadUrl, "InstallerDownloadUrl");
            Assert.IsTrue(info.ReleaseNotes.Contains("Correção do modo \"-silent\""), "Unescape de unicode e aspas em ReleaseNotes");
        }

        private static void Test_UpdateChecker_IsTrustedDownloadUrl_ValidatesGitHubHttpsOnly()
        {
            Assert.IsTrue(UpdateChecker.IsTrustedDownloadUrl("https://github.com/fliplima/OpenClevoRGB/releases/download/v1.5.0/Instalador_OpenClevoRGB.exe"), "github.com https");
            Assert.IsTrue(UpdateChecker.IsTrustedDownloadUrl("https://objects.githubusercontent.com/github-production-release-asset/123/Instalador.exe"), "objects.githubusercontent.com https");
            Assert.IsFalse(UpdateChecker.IsTrustedDownloadUrl("http://github.com/fliplima/OpenClevoRGB/releases/download/v1.5.0/Instalador_OpenClevoRGB.exe"), "http inseguro deve ser rejeitado");
            Assert.IsFalse(UpdateChecker.IsTrustedDownloadUrl("https://evil.com/Instalador_OpenClevoRGB.exe"), "dominio externo deve ser rejeitado");
            Assert.IsFalse(UpdateChecker.IsTrustedDownloadUrl("https://github.com.evil.com/Instalador_OpenClevoRGB.exe"), "spoof de subdominio deve ser rejeitado");
            Assert.IsFalse(UpdateChecker.IsTrustedDownloadUrl(""), "string vazia deve ser rejeitada");
        }

        private static void Test_UpdateChecker_CheckForUpdate_MockHttp_DetectsNewAndUpToDate()
        {
            string jsonV150 = "{ \"tag_name\": \"v1.5.0\", \"html_url\": \"https://github.com/fliplima/OpenClevoRGB/releases/tag/v1.5.0\", \"body\": \"Nova versao\", \"assets\": [{ \"name\": \"Instalador_OpenClevoRGB.exe\", \"browser_download_url\": \"https://github.com/fliplima/OpenClevoRGB/releases/download/v1.5.0/Instalador_OpenClevoRGB.exe\" }] }";

            UpdateCheckResult resNew = UpdateChecker.CheckForUpdate(new Version(1, 4, 0, 0), url => jsonV150);
            Assert.IsTrue(resNew.Success, "Check com nova versao deve ter Success=true");
            Assert.IsTrue(resNew.UpdateAvailable, "1.5.0 deve ser detectada como disponivel para 1.4.0.0");

            UpdateCheckResult resSame = UpdateChecker.CheckForUpdate(new Version(1, 5, 0, 0), url => jsonV150);
            Assert.IsTrue(resSame.Success, "Check com mesma versao deve ter Success=true");
            Assert.IsFalse(resSame.UpdateAvailable, "1.5.0 nao deve ser disponivel para 1.5.0.0");

            UpdateCheckResult resEmpty = UpdateChecker.CheckForUpdate(new Version(1, 4, 0, 0), url => "");
            Assert.IsFalse(resEmpty.Success, "Resposta vazia deve retornar Success=false");
        }
    }
}
