using System;
using System.Windows.Forms;

namespace SharpMap
{
    internal static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Data.Providers.ODMatrix.MatrixProviderExample());
        }
    }
}