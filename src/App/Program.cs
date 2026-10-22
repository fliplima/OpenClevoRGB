using System;
using System.Threading;
using System.Windows.Forms;

namespace OpenClevoRGB
{
    static class Program
    {
        private const string AppMutexName = "OpenClevoRGB_SingleInstance_Mutex";

        [STAThread]
        static void Main(string[] args)
        {
            bool createdNew;
            using (Mutex mutex = new Mutex(true, AppMutexName, out createdNew))
            {
                if (!createdNew)
                {
                    // Já existe uma instância em execução!
                    // Poderia enviar mensagem para restaurar, ou apenas encerrar silenciosamente
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                bool startSilent = false;
                if (args != null && args.Length > 0)
                {
                    foreach (string arg in args)
                    {
                        if (arg.Equals("-silent", StringComparison.OrdinalIgnoreCase) ||
                            arg.Equals("-minimized", StringComparison.OrdinalIgnoreCase))
                        {
                            startSilent = true;
                            break;
                        }
                    }
                }

                MainForm mainForm = new MainForm();

                if (startSilent)
                {
                    // Inicia sem mostrar a janela principal, mantendo apenas o NotifyIcon na bandeja
                    mainForm.WindowState = FormWindowState.Minimized;
                    mainForm.ShowInTaskbar = false;
                    Application.Run(); // roda o message loop
                }
                else
                {
                    Application.Run(mainForm);
                }
            }
        }
    }
}

