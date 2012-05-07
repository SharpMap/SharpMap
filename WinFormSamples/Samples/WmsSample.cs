using System.Drawing;
using System.Drawing.Drawing2D;
using SharpMap;
using SharpMap.Layers;
using Point=GeoAPI.Geometries.Coordinate;

namespace WinFormSamples.Samples
{
    public static class WmsSample
    {
        public static Map InitializeMap(float angle)
        {
            string wmsUrl = "http://dev:8080/geoserver/ows?service=wms&version=1.1.1&request=GetCapabilities";

            Map map = new Map();


            WmsLayer layWms = new WmsLayer("Demis Map", wmsUrl);

            layWms.AddLayer("sf:roads");
            //layWms.AddLayer("Topography");
            //layWms.AddLayer("Hillshading");

            layWms.SetImageFormat(layWms.OutputFormats[0]);
            layWms.ContinueOnError = true;
                //Skip rendering the WMS Map if the server couldn't be requested (if set to false such an event would crash the app)
            layWms.TimeOut = 5000; //Set timeout to 5 seconds
            layWms.SRID = 4326;
            map.Layers.Add(layWms);

            //limit the zoom to 360 degrees width
            map.MaximumZoom = 360;
            map.BackColor = Color.LightBlue;

            map.Zoom = 360;
            map.Center = new Point(0, 0);

            Matrix mat = new Matrix();
            mat.RotateAt(angle, map.WorldToImage(map.Center));
            map.MapTransform = mat;

            map.ZoomToExtents();
            return map;
        }
    }
}