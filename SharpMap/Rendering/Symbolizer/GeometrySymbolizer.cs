using System;
using System.Drawing;
using GeoAPI.Geometries;
using SharpMap.Data;
using SharpMap.Rendering.Thematics;

namespace SharpMap.Rendering.Symbolizer
{
    /// <summary>
    /// Multi geometry symbolizer class
    /// </summary>
    [Serializable]
    public class GeometrySymbolizer : ISymbolizer<IGeometry>
    {
        private IPointSymbolizer _pointSymbolizer;
        private ILineSymbolizer _lineSymbolizer;
        private IPolygonSymbolizer _polygonSymbolizer;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        public GeometrySymbolizer()
        {
            _pointSymbolizer = new RasterPointSymbolizer();
            _lineSymbolizer = new BasicLineSymbolizer();
            _polygonSymbolizer = new BasicPolygonSymbolizer();
        }

        /// <inheritdoc/>
        public object Clone()
        {
            return new GeometrySymbolizer
                       {
                           _pointSymbolizer = (IPointSymbolizer) _pointSymbolizer.Clone(),
                           _lineSymbolizer = (ILineSymbolizer) _lineSymbolizer.Clone(),
                           _polygonSymbolizer = (IPolygonSymbolizer) _polygonSymbolizer.Clone()
                       };

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
        public void Render(Map map, IGeometry geometry, Graphics graphics)
        {
            switch (geometry.OgcGeometryType)
            {
                case OgcGeometryType.Point:
                case OgcGeometryType.MultiPoint:
                    _pointSymbolizer.Render(map, (IPuntal)geometry, graphics);
                    return;

                case OgcGeometryType.LineString:
                case OgcGeometryType.MultiLineString:
                    _lineSymbolizer.Render(map, (ILineal)geometry, graphics);
                    return;

                case OgcGeometryType.Polygon:
                case OgcGeometryType.MultiPolygon:
                    _polygonSymbolizer.Render(map, (IPolygonal)geometry, graphics);
                    return;

                case OgcGeometryType.GeometryCollection:
                    foreach (var g in ((IGeometryCollection)geometry))
                    {
                        Render(map, g, graphics);
                    }
                    return;

            }

            throw new Exception("Unknown geometry type");
        }

        /// <summary>
        /// Method to indicate that the symbolizer has to be prepared.
        /// </summary>
        /// <param name="g">The graphics object</param>
        /// <param name="map">The map</param>
        /// <param name="aproximateNumberOfGeometries">The approximate number of geometries</param>
        public void Begin(Graphics g, Map map, int aproximateNumberOfGeometries)
        {
            _lineSymbolizer.Begin(g, map, aproximateNumberOfGeometries);
            _pointSymbolizer.Begin(g, map, aproximateNumberOfGeometries);
            _polygonSymbolizer.Begin(g, map, aproximateNumberOfGeometries);
        }

        /// <summary>
        /// Method to indicate that the symbolizer should do its symbolizer work.
        /// </summary>
        /// <param name="g">The graphics object</param>
        /// <param name="map">The map</param>
        public void Symbolize(Graphics g, Map map)
        {
            _polygonSymbolizer.Symbolize(g, map);
            _lineSymbolizer.Symbolize(g, map);
            _pointSymbolizer.Symbolize(g, map);
        }

        /// <summary>
        /// Method to indicate that the symbolizers work is done and it can clean up.
        /// </summary>
        /// <param name="g">The graphics object</param>
        /// <param name="map">The map</param>
        public void End(Graphics g, Map map)
        {
            _polygonSymbolizer.End(g, map);
            _lineSymbolizer.End(g, map);
            _pointSymbolizer.End(g, map);
        }

        /// <summary>
        /// Gets or sets a value indicating which <see cref="ISymbolizer.SmoothingMode"/> is to be used for rendering
        /// </summary>
        public System.Drawing.Drawing2D.SmoothingMode SmoothingMode
        {
            get { return _pointSymbolizer.SmoothingMode; }
            set
            {
                _pointSymbolizer.SmoothingMode =
                    _lineSymbolizer.SmoothingMode =
                        _polygonSymbolizer.SmoothingMode = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating which <see cref="ISymbolizer.PixelOffsetMode"/> is to be used for rendering
        /// </summary>
        public System.Drawing.Drawing2D.PixelOffsetMode PixelOffsetMode
        {
            get { return _pointSymbolizer.PixelOffsetMode; }
            set
            {
                _pointSymbolizer.PixelOffsetMode =
                    _lineSymbolizer.PixelOffsetMode =
                        _polygonSymbolizer.PixelOffsetMode = value;
            }
        }
    }
}