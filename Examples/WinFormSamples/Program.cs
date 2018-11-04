using System;
using System.Windows.Forms;
using System.Text;
using NetTopologySuite;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

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
            var gss = new NtsGeometryServices();
            var css = new SharpMap.CoordinateSystems.CoordinateSystemServices(
                new CoordinateSystemFactory(), 
                new CoordinateTransformationFactory(), 
                SharpMap.Converters.WellKnownText.SpatialReference.GetAllReferenceSystems());

            // plug-in WebMercator so that correct spherical definition is directly available to Layer Transformations using SRID
            var pcs = (ProjectedCoordinateSystem)ProjectedCoordinateSystem.WebMercator;
            css.AddCoordinateSystem((int)pcs.AuthorityCode, pcs);

            GeoAPI.GeometryServiceProvider.Instance = gss;
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
