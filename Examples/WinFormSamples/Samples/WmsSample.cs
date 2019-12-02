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
            string wmsUrl = "http://resource.sgu.se/service/wms/130/brunnar";

            Map map = new Map();
            

            WmsLayer layWms = new WmsLayer("Brunnar", wmsUrl);

            layWms.AddLayer("SE.GOV.SGU.BRUNNAR.250K");
            //layWms.AddLayer("Topography");
            //layWms.AddLayer("Hillshading");

            layWms.SetImageFormat(layWms.OutputFormats[0]);
            layWms.ContinueOnError = true;
                //Skip rendering the WMS Map if the server couldn't be requested (if set to false such an event would crash the app)
            layWms.TimeOut = 20000; //Set timeout to 5 seconds
            layWms.SRID = 3006;

            //map.BackgroundLayer.Add(AsyncLayerProxyLayer.Create(layWms, new Size(256, 256)));
            map.BackgroundLayer.Add(layWms);
            map.MaximumExtents = layWms.Envelope;
            
            //limit the zoom to 360 degrees width
            map.ZoomToExtents();
            map.BackColor = Color.LightBlue;

            //map.Zoom = 360;
            //map.Center = new Point(0, 0);

            Matrix mat = new Matrix();
            mat.RotateAt(angle, map.WorldToImage(map.Center));
            map.MapTransform = mat;

            map.ZoomToExtents();
            map.Zoom = map.Envelope.Width/3;
            return map;
        }
    }
}