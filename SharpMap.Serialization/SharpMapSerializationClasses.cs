using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SharpMap.Serialization.Model
{
    [XmlInclude(typeof(WmsLayer))]
    [XmlInclude(typeof(SpatiaLiteLayer))]
    [XmlInclude(typeof(ShapefileLayer))]
    public partial class MapDefinition
    {
        public string BackGroundColor { get; set; }
        public Extent Extent { get; set; }
        public MapLayer[] Layers { get; set; }
        public int SRID { get; set; }
    }
    public class Extent
    {
        public double Xmin { get; set; }
        public double Ymin { get; set; }
        public double Xmax { get; set; }
        public double Ymax { get; set; }
    }
    
    public partial class MapLayer
    {
        public string Name { get; set; }
        public double MinVisible { get; set; }
        public double MaxVisible { get; set; }
    }

    public partial class StyledLayer : MapLayer
    {
        public Style[] Styles { get; set; }
    }


    [XmlInclude(typeof(SimpleLineStyle))]
    [XmlInclude(typeof(SimplePointStyle))]
    [XmlInclude(typeof(SimpleFillStyle))]
    [XmlInclude(typeof(SimpleMarkerStyle))]
    public class Style
    {
    }

    public class SimplePointStyle : Style
    {
        public string Color { get; set; }
        public double Opacity { get; set; }
        public double Size { get; set; }
        public double OutlineWidth { get; set; }
        public string OutlineColor { get; set; }
    }
    public class SimpleMarkerStyle : Style
    {
        public double? Opacity { get; set; }
        public double Size { get; set; }
        public byte[] MarkerPicture { get; set; }
        public double? SymbolOffsetX { get; set; }
        public double? SymbolOffsetY { get; set; }
    }
    public class SimpleLineStyle : Style
    {
        public string Color { get; set; }
        public double Opacity { get; set; }
        public string OutlineColor { get; set; }
        public double LineWidth { get; set; }
        public double OutlineWidth { get; set; }
    }
    public class SimpleFillStyle : Style
    {
        public string Color { get; set; }
        public double FillOpacity { get; set; }
        public string OutlineColor { get; set; }
        public double OutlineWidth { get; set; }
    }


    public class WmsLayer : MapLayer
    {
        public string OnlineURL { get; set; }
        public string WmsUser { get; set; }
        public string WmsPassword { get; set; }
        public string WmsLayers { get; set; }
    }

    public class SpatiaLiteLayer : StyledLayer 
    {
        public string FileName { get; set; }
        public string TableName { get; set; }
        public string GeometryColumn { get; set; }
    }

    public class ShapefileLayer : StyledLayer
    {
        public string FileName { get; set; }
    }

    public class OsmLayer : MapLayer
    {
    }
    public class GoogleLayer : MapLayer
    {
    }
    public class GoogleSatLayer : MapLayer
    {
    }
    public class GoogleTerrainLayer : MapLayer
    {
    }
    public class BingLayer : MapLayer
    {
    }

    public class CustomLayer : StyledLayer
    {
        public string ClassName { get; set; }
        public string CustomData { get; set; }
    }

}
