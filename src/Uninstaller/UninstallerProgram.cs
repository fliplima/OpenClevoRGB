using System;
using System.Windows.Forms;

namespace OpenClevoRGBUninstaller
{
    static class UninstallerProgram
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new UninstallerForm());
        }
    }
}

