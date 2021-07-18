using Eto.Forms;
using System;
using System.Runtime.InteropServices;

namespace CheckersClient.Windows
{
    class MainClass
    {
        [DllImport("shcore.dll")]
        static extern int SetProcessDpiAwareness(int PROCESS_DPI_AWARENESS);

        [STAThread]
        public static void Main(string[] args)
        {
            SetProcessDpiAwareness(2);
            new Application(Eto.Platforms.WinForms).Run(new MainForm());
        }
    }
}
