using System;
using System.Drawing;
using SharpMap.Data;
using SharpMap.Geometries;
using SharpMap.Rendering.Thematics;

namespace SharpMap.Rendering.Symbolizer
{
    /// <summary>
    /// Multi geometry symbolizer class
    /// </summary>
    [Serializable]
    public class GeometrySymbolizer : ISymbolizer<Geometry>
    {
        private IPointSymbolizer _pointSymbolizer;
        private ILineSymbolizer _lineSymbolizer;
        private IPolygonSymbolizer _polygonSymbolizer;

        public GeometrySymbolizer()
        {
            _pointSymbolizer = new RasterPointSymbolizer();
            _lineSymbolizer = new BasicLineSymbolizer();
            _polygonSymbolizer = new BasicPolygonSymbolizer();
        }
        
        /// <summary>
        /// Gets or sets the point symbolizer
        /// </summary>
        public IPointSymbolizer PointSymbolizer
        {
            get { return _pointSymbolizer; }
            set { _pointSymbolizer = value; }
            
        }

        /// <summary>
        /// Gets or sets the line symbolizer
        /// </summary>
        public ILineSymbolizer LineSymbolizer
        {
            get { return _lineSymbolizer; }
            set { _lineSymbolizer = value; }

        }

        /// <summary>
        /// Gets or sets the polygon symbolizer
        /// </summary>
        public IPolygonSymbolizer PolygonSymbolizer
        {
            get { return _polygonSymbolizer; }
            set { _polygonSymbolizer = value; }

        }

        /// <summary>
        /// Function to render the geometry
        /// </summary>
        /// <param name="map">The map object, mainly needed for transformation purposes.</param>
        /// <param name="geometry">The geometry to symbolize.</param>
        /// <param name="graphics">The graphics object to use.</param>
        public void Render(Map map, Geometry geometry, Graphics graphics)
        {
            switch (geometry.GeometryType)
            {
                case GeometryType2.Point:
                case GeometryType2.MultiPoint:
                    _pointSymbolizer.Render(map, (IPuntal)geometry, graphics);
                    return;

                case GeometryType2.LineString:
                case GeometryType2.MultiLineString:
                    _lineSymbolizer.Render(map, (ILineal)geometry, graphics);
                    return;

                case GeometryType2.Polygon:
                case GeometryType2.MultiPolygon:
                    _polygonSymbolizer.Render(map, (IPolygonal)geometry, graphics);
                    return;

                case GeometryType2.GeometryCollection:
                    foreach (Geometry g in ((GeometryCollection)geometry).Collection)
                    {
                        Render(map, g, graphics);
                    }
                    return;

            }

            throw new Exception("Unknown geometry type");
        }

        public void Begin(Graphics g, Map map, int aproximateNumberOfGeometries)
        {
            _lineSymbolizer.Begin(g, map, aproximateNumberOfGeometries);
            _pointSymbolizer.Begin(g, map, aproximateNumberOfGeometries);
            _polygonSymbolizer.Begin(g, map, aproximateNumberOfGeometries);
        }

        public void Symbolize(Graphics g, Map map)
        {
            _polygonSymbolizer.Symbolize(g, map);
            _lineSymbolizer.Symbolize(g, map);
            _pointSymbolizer.Symbolize(g, map);
        }

        public void End(Graphics g, Map map)
        {
            _polygonSymbolizer.End(g, map);
            _lineSymbolizer.End(g, map);
            _pointSymbolizer.End(g, map);
        }
    }
}