using System;
using System.Windows.Forms;

namespace CurePlease.App
{
    internal static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new CurePleaseForm());
        }
    }
}

namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit {}
}