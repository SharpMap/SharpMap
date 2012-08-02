// code adapted from: https://github.com/awcoats/mapstache
namespace Mapstache
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Linq;

    using GeoAPI.Geometries;

    public class GraphicsPathBuilder
    {
        private readonly float metersToPixel;
        private readonly Point topLeft;
        private readonly Size size;

        public GraphicsPathBuilder(RectangleF bounds, Size size)
        {
            if (bounds.IsEmpty)
                throw new ArgumentException("Bounds is empty.", "bounds");
            if (size.IsEmpty)
                throw new ArgumentException("Size is empty.", "size");

            this.metersToPixel = bounds.Width/size.Width;
            this.topLeft = new Point((int) bounds.Left, (int) (bounds.Top));
            this.size = size;
        }

        public GraphicsPath Build(IGeometry geometry)
        {
            GraphicsPath graphicsPath = new GraphicsPath { FillMode = FillMode.Alternate };
            string geometryType = geometry.GeometryType;
            switch (geometryType)
            {
                case "Polygon":
                    this.AddPolygon((IPolygon)geometry, graphicsPath);
                    break;

                case "MultiPolygon":
                    this.AddMultiPolygon((IMultiPolygon)geometry, graphicsPath);
                    break;

                case "GeometryCollection":
                    for (int i = 0; i < geometry.NumGeometries; i++)
                    {
                        IGeometry geom = geometry.GetGeometryN(i + 1);
                        switch (geom.GeometryType)
                        {
                            case "Polygon":
                                this.AddPolygon((IPolygon)geom, graphicsPath);
                                break;
                            case "MultiPolygon":
                                this.AddMultiPolygon((IMultiPolygon)geom, graphicsPath);
                                break;
                        }
                    }
                    break;

                default:
                    string format = string.Format("The geometry type {0} is not supported.", geometryType);
                    throw new NotSupportedException(format);
            }
            return graphicsPath;
        }

        private void AddMultiPolygon(IMultiPolygon geometry, GraphicsPath graphicsPath)
        {
            if (geometry == null)
                throw new ArgumentNullException("geometry");

            foreach (var geom in geometry.Geometries)
                this.AddPolygon((IPolygon)geom, graphicsPath);
        }

        private void AddPolygon(IPolygon polygon, GraphicsPath graphicsPath)
        {
            if (polygon == null)
                throw new ArgumentNullException("polygon");

            ILineString exterior = polygon.ExteriorRing;
            IEnumerable<PointF> coords = this.GetCoords(exterior);
            graphicsPath.AddPolygon(coords.ToArray());

            foreach (ILineString ring in polygon.InteriorRings)
            {
                coords = this.GetCoords(ring);
                graphicsPath.AddPolygon(coords.ToArray());
            }
        }

        private IEnumerable<PointF> GetCoords(IGeometry lineString)
        {
            if (lineString == null)
                throw new ArgumentNullException("lineString");

            return lineString.Coordinates
                .Select(coord => new PointF((float)coord.X, (float)coord.Y))
                .Select(this.GetPixel);
        }

        private PointF GetPixel(PointF ll)
        {
            Point meters = SphericalMercator.FromLonLat(ll);
            float x = (meters.X - this.topLeft.X)/this.metersToPixel;
            float y = (meters.Y - this.topLeft.Y)/this.metersToPixel;
            y = this.size.Height - y;
            return new PointF(x, y);
        }
    }
}
