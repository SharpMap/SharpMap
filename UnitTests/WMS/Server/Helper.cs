using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using SharpMap;
using SharpMap.Data.Providers;
using SharpMap.Layers;
using SharpMap.Styles;
using SharpMap.Web.Wms;

namespace UnitTests.WMS.Server
{
    public class LayerData
    {
        public string LabelColumn { get; set; }
        public IStyle Style { get; set; }
    }

    internal static class Helper
    {
        private static readonly IDictionary<string, LayerData> Nyc;

        static Helper()
        {
            LayerData landmarks = new LayerData
            {
                LabelColumn = "LANAME",
                Style = new VectorStyle
                {
                    EnableOutline = true,
                    Fill = new SolidBrush(Color.FromArgb(192, Color.LightBlue))
                }
            };
            LayerData roads = new LayerData
            {
                LabelColumn = "NAME",
                Style = new VectorStyle
                {
                    EnableOutline = false,
                    Fill = new SolidBrush(Color.FromArgb(192, Color.LightGray)),
                    Line = new Pen(Color.FromArgb(200, Color.DarkBlue), 0.5f)
                }
            };
            LayerData pois = new LayerData
            {
                LabelColumn = "NAME",
                Style = new VectorStyle
                {
                    PointColor = new SolidBrush(Color.FromArgb(200, Color.DarkGreen)),
                    PointSize = 10
                }
            };
            Nyc = new Dictionary<string, LayerData>
            {
                { "nyc/poly_landmarks.shp", landmarks },
                { "nyc/tiger_roads.shp", roads },
                { "nyc/poi.shp", pois }
            };
        }

        internal static Capabilities.WmsServiceDescription Description()
        {
            Capabilities.WmsServiceDescription desc = new Capabilities.WmsServiceDescription("Test Server", "http://localhost/sharpmap");
            desc.MaxWidth = 500;
            desc.MaxHeight = 500;
            desc.Abstract = "SharpMap Test Server";
            desc.Keywords = new[] { "sharpmap", "test", "server" };
            desc.ContactInformation.PersonPrimary.Person = "John Doe";
            desc.ContactInformation.PersonPrimary.Organisation = "SharpMap Inc";
            desc.ContactInformation.Address.AddressType = "postal";
            desc.ContactInformation.Address.Country = "Neverland";
            desc.ContactInformation.VoiceTelephone = "1-800-WE DO MAPS";
            return desc;
        }

        internal static Map Default()
        {       
            string currdir = Directory.GetCurrentDirectory();
            Trace.WriteLine(String.Format("Current directory: {0}", currdir));

            Map map = new Map(new Size(1, 1));
            IDictionary<string, LayerData> dict = Nyc;
            foreach (string layer in dict.Keys)
            {
                string format = String.Format(@"WMS\Server\data\{0}", layer);                
                string path = Path.Combine(currdir, format);
                if (!File.Exists(path))
                    throw new FileNotFoundException("file not found", path);

                string name = Path.GetFileNameWithoutExtension(layer);
                LayerData data = dict[layer];
                ShapeFile source = new ShapeFile(path, true);
                VectorLayer item = new VectorLayer(name, source)
                {
                    SRID = 4326,
                    Style = (VectorStyle)data.Style,
                    SmoothingMode = SmoothingMode.AntiAlias
                };
                map.Layers.Add(item);
            }
            return map;
        }
    }
}
