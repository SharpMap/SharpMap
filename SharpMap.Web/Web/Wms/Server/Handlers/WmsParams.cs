using System.Drawing;
using GeoAPI.Geometries;

namespace SharpMap.Web.Wms.Server.Handlers
{
    public class WmsParams
    {
        public string Service { get; set; }
        public string Version { get; set; }
        public string Layers { get; set; }
        public string Styles { get; set; }
        public string CRS { get; set; }
        public string QueryLayers { get; set; }
        public Envelope BBOX { get; set; }
        public short Width { get; set; }
        public short Height { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public string Format { get; set; }
        public string InfoFormat { get; set; }
        public int FeatureCount { get; set; }
        public string CqlFilter { get; set; }
        public Color BackColor { get; set; }
    }
}