using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Management;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace OpenClevoRGB
{
    public enum KeyboardZone
    {
        Left = 0,
        Middle = 1,
        Right = 2
    }

    public enum KeyboardEffect
    {
        Static = 0,
        Breathe = 1,
        Cycle = 2,
        Wave = 3,
        Dance = 4,
        Flash = 5,
        Random = 6
    }

    public class ClevoController
    {
        private ManagementObject classInstance = null;
        private bool isInitialized = false;

        private const string WmiNamespace = "root\\WMI";
        private const string WmiInstanceName = "CLEVO_GET.InstanceName='ACPI\\PNP0C14\\0_0'";
        private const string WmiMethodName = "SetKBLED";

        // Códigos de controle de energia (Power)
        private const uint CmdDisableKeyboard = 3758096391u; // 0xE0000007
        private const uint CmdEnableKeyboard  = 3758616577u; // 0xE007F001

        // Níveis de Brilho
        private static readonly uint[] BrightnessCodes = new uint[]
        {
            0u,           // 0: Desligado
            4093640751u,  // 1: 25% (0xF400002F)
            4093640799u,  // 2: 50% (0xF400005F)
            4093640847u,  // 3: 75% (0xF400008F)
            4093640895u   // 4: 100% (0xF40000BF)
        };

        // Efeitos de Hardware pré-programados no BIOS
        private static readonly uint[] EffectCodes = new uint[]
        {
            0u,           // 0: Estático
            855703552u,   // 1: Respiração (Breathe)
            2952790016u,  // 2: Ciclo (Cycle)
            2147483648u,  // 3: Onda (Wave)
            2415919104u,  // 4: Dança (Dance)
            2684354560u,  // 5: Flash (Tempo)
            1879048192u   // 6: Aleatório (Random)
        };

        public string LastErrorMessage { get; private set; }
        public bool IsAccessDenied { get; private set; }

        public bool IsInitialized
        {
            get { return isInitialized; }
        }

        public bool TryInitialize()
        {
            LastErrorMessage = string.Empty;
            IsAccessDenied = false;

            try
            {
                // Tenta primeiro obter a instância dinâmica disponível
                using (ManagementClass mc = new ManagementClass(WmiNamespace, "CLEVO_GET", null))
                using (ManagementObjectCollection instances = mc.GetInstances())
                {
                    foreach (ManagementObject mo in instances)
                    {
                        this.classInstance = mo;
                        this.isInitialized = true;
                        Debug.WriteLine("Instância WMI conectada com sucesso: " + mo["InstanceName"]);
                        return true;
                    }
                }

                // Fallback para caminho direto padrão
                this.classInstance = new ManagementObject(WmiNamespace, WmiInstanceName, null);
                object dummy = this.classInstance["InstanceName"];
                this.isInitialized = (dummy != null);
                return this.isInitialized;
            }
            catch (ManagementException mex)
            {
                if (mex.ErrorCode == ManagementStatus.AccessDenied)
                {
                    IsAccessDenied = true;
                    LastErrorMessage = "Acesso Negado: O Windows requer privilégios de Administrador para controlar o hardware ACPI.";
                }
                else
                {
                    LastErrorMessage = "Erro WMI (" + mex.ErrorCode + "): " + mex.Message;
                }
                Debug.WriteLine(LastErrorMessage);
                this.isInitialized = false;
                this.classInstance = null;
                return false;
            }
            catch (UnauthorizedAccessException uex)
            {
                IsAccessDenied = true;
                LastErrorMessage = "Acesso Negado: " + uex.Message;
                Debug.WriteLine(LastErrorMessage);
                this.isInitialized = false;
                this.classInstance = null;
                return false;
            }
            catch (Exception ex)
            {
                LastErrorMessage = "Erro: " + ex.Message;
                Debug.WriteLine("Erro ao conectar no WMI Clevo: " + ex.Message);
                this.isInitialized = false;
                this.classInstance = null;
                return false;
            }
        }

        public bool IsDriverConfigured()
        {
            try
            {
                string syswow64 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SysWOW64", "clevomof.dll");
                string system32 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "clevomof.dll");
                
                bool fileExists = File.Exists(syswow64) || File.Exists(system32);
                if (!fileExists)
                {
                    return false;
                }

                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\WmiAcpi", false))
                {
                    if (key != null)
                    {
                        object val = key.GetValue("MofImagePath");
                        if (val != null && val.ToString().ToLower().Contains("clevomof.dll"))
                        {
                            return true;
                        }
                    }
                }
            }
            catch
            {
                // Ignorar erro de leitura de registro
            }
            return false;
        }

        public bool InstallDriverMetadata(string localDllPath)
        {
            try
            {
                string winDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                string destSysWOW64 = Path.Combine(winDir, "SysWOW64", "clevomof.dll");
                string destSystem32 = Path.Combine(winDir, "System32", "clevomof.dll");

                if (!File.Exists(localDllPath))
                {
                    throw new FileNotFoundException("Arquivo clevomof.dll não foi encontrado em: " + localDllPath);
                }

                // Copia para SysWOW64 (se for sistema 64-bit) ou System32
                if (Directory.Exists(Path.Combine(winDir, "SysWOW64")))
                {
                    File.Copy(localDllPath, destSysWOW64, true);
                }
                File.Copy(localDllPath, destSystem32, true);

                // Configura o registro do WmiAcpi
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\WmiAcpi", true))
                {
                    if (key != null)
                    {
                        key.SetValue("MofImagePath", "syswow64\\clevomof.dll", RegistryValueKind.String);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Erro ao instalar metadados Clevo: " + ex.Message);
                return false;
            }
        }

        public void SendRawWmiCode(uint code)
        {
            if (!this.isInitialized)
            {
                if (!TryInitialize())
                {
                    return;
                }
            }

            try
            {
                using (ManagementBaseObject inParams = this.classInstance.GetMethodParameters(WmiMethodName))
                {
                    inParams["Data"] = code;
                    this.classInstance.InvokeMethod(WmiMethodName, inParams, null);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Erro ao invocar SetKBLED: " + ex.Message);
            }
        }

        public void SetPower(bool enabled)
        {
            SendRawWmiCode(enabled ? CmdEnableKeyboard : CmdDisableKeyboard);
        }

        public void SetBrightness(int level)
        {
            if (level < 0) level = 0;
            if (level > 4) level = 4;

            if (level == 0)
            {
                SetPower(false);
            }
            else
            {
                SetPower(true);
                Thread.Sleep(30);
                SendRawWmiCode(BrightnessCodes[level]);
            }
        }

        public void SetZoneColor(KeyboardZone zone, Color color)
        {
            int r = color.R;
            int g = color.G;
            int b = color.B;

            // O firmware da Clevo aplica proporção no canal azul
            int bAdj = (int)Math.Round((double)b * 200.0 / 255.0);

            // Formato de cor: (Blue << 16) | (Red << 8) | Green
            long colorData = (long)((bAdj << 16) + (r << 8) + g);

            switch (zone)
            {
                case KeyboardZone.Left:
                    colorData |= 4026531840u; // 0xF0000000
                    break;
                case KeyboardZone.Middle:
                    colorData |= 4043309056u; // 0xF1000000
                    break;
                case KeyboardZone.Right:
                    colorData |= 4060086272u; // 0xF2000000
                    break;
            }

            SendRawWmiCode((uint)colorData);
        }

        public void SetAllZonesColor(Color color)
        {
            SetZoneColor(KeyboardZone.Left, color);
            Thread.Sleep(20);
            SetZoneColor(KeyboardZone.Middle, color);
            Thread.Sleep(20);
            SetZoneColor(KeyboardZone.Right, color);
        }

        public void SetEffect(KeyboardEffect effect)
        {
            int idx = (int)effect;
            if (idx >= 0 && idx < EffectCodes.Length && EffectCodes[idx] != 0u)
            {
                SendRawWmiCode(EffectCodes[idx]);
            }
        }
    }
}

