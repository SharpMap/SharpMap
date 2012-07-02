using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using GeoAPI.Geometries;

namespace SharpMap.Data.Providers.ODMatrix
{
    public abstract class MatrixProviderBase : IProvider
    {
        protected readonly IODMatrix Matrix;

        protected MatrixProviderBase(IODMatrix matrix)
        {
            Factory = new NetTopologySuite.Geometries.GeometryFactory();
            Matrix = matrix;
            MinValid = 0f;
            MaxValid = double.MaxValue;
        }

        public void Dispose()
        {
        }

        public abstract string ProviderName { get; }

        public string ConnectionID
        {
            get { return String.Format("Matrix{1}: {0}", ProviderName, Matrix.Name); }
        }

        public bool IsOpen
        {
            get { return true; }
        }

        public int SRID { get; set; }

        public abstract Collection<IGeometry> GetGeometriesInView(Envelope bbox);

        public abstract Collection<uint> GetObjectIDsInView(Envelope bbox);

        public abstract IGeometry GetGeometryByID(uint oid);

        public void ExecuteIntersectionQuery(IGeometry geom, FeatureDataSet ds)
        {
            throw new NotSupportedException();
        }

        public abstract void ExecuteIntersectionQuery(Envelope box, FeatureDataSet ds);

        public abstract int GetFeatureCount();

        public abstract FeatureDataRow GetFeature(uint rowId);

        private Envelope _extents;

        public Envelope GetExtents()
        {
            return _extents ?? (_extents = GetExtentsInternal());
        }

        private Envelope GetExtentsInternal()
        {
            var points = Matrix.Locations;
            var res = points.First().EnvelopeInternal;
            foreach (var point in points.Skip(1))
                res.ExpandToInclude(point.EnvelopeInternal);
            return res;
        }

        public void Open()
        {
            // nothing to do;
        }

        public void Close()
        {
            // nothing to do;
        }

        protected static FeatureDataTable CreateTable()
        {
            var res = new FeatureDataTable();
            res.Columns.Add(new DataColumn("Oid", typeof(ushort)));
            res.Columns.Add(new DataColumn("Value", typeof(double)));
            res.Constraints.Add("PK_OID", res.Columns[0], true);
            return res;
        }

        public double ScaleFactor { get; set; }

        public ScaleMethod ScaleMethod { get; set; }

        protected double Scale(double value)
        {
            switch (ScaleMethod)
            {
                //case ScaleMethod.None:
                default:
                    return value;
                case ScaleMethod.Linear:
                    return ScaleFactor * value;
                case ScaleMethod.Square:
                    return Math.Sqrt(value);
                case ScaleMethod.Qubic:
                    return Math.Pow(value, 1d / 3d);
                case ScaleMethod.Log10:
                    return Math.Log10(value);
                case ScaleMethod.Log:
                    return Math.Log(value, ScaleFactor);
            }
        }

        protected static IPolygon CreateCircle(IPoint point, double d)
        {
            return  Factory.CreatePolygon(CreateCircleRing(point, d * 0.5d, 12), null);
        }

        protected static IGeometryFactory Factory { get; private set; }

        private static ILinearRing CreateCircleRing(IPoint center,
                                                   double size,
                                                   int segmentsPerQuadrant)
        {
            const double piHalf = Math.PI * 0.5d;

            var step = piHalf / segmentsPerQuadrant;

            var pts = new Coordinate[4 * segmentsPerQuadrant + 1];
            var angle = 0d;
            for (var i = 0; i < 4 * segmentsPerQuadrant; i++)
            {
                pts[i] = new Coordinate(center.X + Math.Cos(angle) * size,
                                   center.Y + Math.Sin(angle) * size);
                angle += step;
            }
            pts[pts.Length - 1] = pts[0];
            return Factory.CreateLinearRing(pts);
        }

        public double MinValid { get; set; }

        public double MaxValid { get; set; }

        protected bool Valid(double d)
        {
            return (MinValid <= d && d <= MaxValid);
        }
    }
}