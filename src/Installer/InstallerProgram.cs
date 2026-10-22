using System;
using System.Windows.Forms;

namespace OpenClevoRGBInstaller
{
    static class InstallerProgram
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new InstallerForm());
        }
    }
}

