using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using GeoAPI.CoordinateSystems.Transformations;
using SharpMap.Data.Providers;
using SharpMap.Layers;
using SharpMap.Styles;

namespace SharpMap.Demo.Wms.Helpers
{
    public static class DatabaseHelper
    {
        public static Map SqlServer()
        {
            const string connstr = "Data Source=.\\SQL2008R2;Initial Catalog=SampleData;Integrated Security=SSPI;";
            Map map = new Map(new Size(1, 1));            

            VectorStyle style0 = new VectorStyle { Line = new Pen(Color.DarkGray, 2) };
            VectorLayer layer0 = CreateLayer(connstr, "osm_boundaries", style0);
            map.Layers.Add(layer0);
            
            VectorStyle style1 = new VectorStyle { Line = new Pen(Color.DarkRed, 2.5f) };
            VectorLayer layer1 = CreateLayer(connstr, "osm_roads_major", style1);
            map.Layers.Add(layer1);

            VectorStyle style2 = new VectorStyle { Line = new Pen(Color.DarkBlue, 1.5f) };
            VectorLayer layer2 = CreateLayer(connstr, "osm_roads_minor", style2);
            map.Layers.Add(layer2);
            
            return map;
        }

        private static VectorLayer CreateLayer(string connstr, string name, VectorStyle style)
        {
            if (connstr == null)
                throw new ArgumentNullException("connstr");
            if (name == null)
                throw new ArgumentNullException("name");

            
            SqlServer2008 source = new SqlServer2008(connstr, name, "geom", "ID") { ValidateGeometries = true };
            ICoordinateTransformation transformation = ProjHelper.LatLonToGoogle();
            VectorLayer item = new VectorLayer(name, source)
            {
                SRID = 4326,
                TargetSRID = 900913,
                CoordinateTransformation = transformation,
                Style = style,
                SmoothingMode = SmoothingMode.AntiAlias,
            };
            return item;
        }
    }
}