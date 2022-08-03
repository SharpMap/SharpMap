using NetTopologySuite;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using System;
using System.Windows.Forms;

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
            var gss = NtsGeometryServices.Instance;
            var css = new SharpMap.CoordinateSystems.CoordinateSystemServices(
                new CoordinateSystemFactory(),
                new CoordinateTransformationFactory(),
                SharpMap.Converters.WellKnownText.SpatialReference.GetAllReferenceSystems());

            // plug-in WebMercator so that correct spherical definition is directly available to Layer Transformations using SRID
            var pcs = (ProjectedCoordinateSystem)ProjectedCoordinateSystem.WebMercator;
            css.AddCoordinateSystem((int)pcs.AuthorityCode, pcs);

            SharpMap.Session.Instance
                .SetGeometryServices(gss)
                .SetCoordinateSystemServices(css)
                .SetCoordinateSystemRepository(css);

            // for SqlServerSample referencing SharpMap.SqlServerSpatialObjects
            //SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new DlgSamplesMenu());
        }
    }

}
