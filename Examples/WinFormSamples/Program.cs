using System;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using NetTopologySuite;

namespace WinFormSamples
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            GeoAPI.GeometryServiceProvider.Instance = new NtsGeometryServices();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new DlgSamplesMenu());
        }
    }
}