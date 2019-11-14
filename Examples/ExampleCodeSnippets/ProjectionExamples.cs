//#define alglib

namespace ExampleCodeSnippets
{
    
    [NUnit.Framework.TestFixture]
    public class ProjectionExamples
    {
        private const string Osgb36 =
            "COMPD_CS[\"OSGB36 / British National Grid + ODN\",PROJCS[\"OSGB 1936 / British National Grid\",GEOGCS[\"OSGB 1936\",DATUM[\"OSGB 1936\",SPHEROID[\"Airy 1830\",6377563.396,299.3249646,AUTHORITY[\"EPSG\",\"7001\"]],TOWGS84[446.448,-125.157,542.06,0.15,0.247,0.842,-4.2261596151967575],AUTHORITY[\"EPSG\",\"6277\"]],PRIMEM[\"Greenwich\",0.0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.017453292519943295],AXIS[\"Geodetic latitude\",NORTH],AXIS[\"Geodetic longitude\",EAST],AUTHORITY[\"EPSG\",\"4277\"]],PROJECTION[\"Transverse Mercator\",AUTHORITY[\"EPSG\",\"9807\"]],PARAMETER[\"central_meridian\",-2.0],PARAMETER[\"latitude_of_origin\",49.0],PARAMETER[\"scale_factor\",0.9996012717],PARAMETER[\"false_easting\",400000.0],PARAMETER[\"false_northing\",-100000.0],UNIT[\"m\",1.0],AXIS[\"Easting\",EAST],AXIS[\"Northing\",NORTH],AUTHORITY[\"EPSG\",\"27700\"]],VERT_CS[\"Newlyn\",VERT_DATUM[\"Ordnance Datum Newlyn\",2005,AUTHORITY[\"EPSG\",\"5101\"]],UNIT[\"m\",1.0],AXIS[\"Gravity-related height\",UP],AUTHORITY[\"EPSG\",\"5701\"]],AUTHORITY[\"EPSG\",\"7405\"]]";

        private const string WGS84 =
            "GEOGCS[\"WGS 84\",DATUM[\"WGS_1984\",SPHEROID[\"WGS 84\",6378137,298.257223563,AUTHORITY[\"EPSG\",\"7030\"]],AUTHORITY[\"EPSG\",\"6326\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.01745329251994328,AUTHORITY[\"EPSG\",\"9122\"]],AUTHORITY[\"EPSG\",\"4326\"]]";


        [NUnit.Framework.Test]
        [NUnit.Framework.Ignore("Nothing to see here")]
        public void TestConversionProjNet()
        {
            var csf = new ProjNet.CoordinateSystems.CoordinateSystemFactory();
            var cs1 = csf.CreateFromWkt(Osgb36);
            var cs2 = csf.CreateFromWkt(WGS84);

            var ctf = new ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory();
            var ct = ctf.CreateFromCoordinateSystems(cs1, cs2);

            System.Diagnostics.Debug.Assert(ct != null);
        }

#if alglib
    /// <summary>
    /// Performs an affine 2D coordinate transfromation
    /// X' = _a*X + _b*Y + _c
    /// Y' = _d*X + _e*Y + _f
    /// </summary>
    public class AffineCoordinateTransformation2D : ProjNet.CoordinateSystems.Transformations.MathTransform, GeoAPI.CoordinateSystems.Transformations.ICoordinateTransformation
    {
        private readonly double _a, _b, _c, _d, _e, _f;
        private readonly double _ainv, _binv, _cinv, _dinv, _einv, _finv;
        //private   Matrix matrix;
        //private readonly Matrix _inverse;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="matrix"><see cref="T:System.Drawing.Drawing2D.Matrix"/> that holds the coefficients and for the coordinate transformation</param>
        public AffineCoordinateTransformation2D(System.Drawing.Drawing2D.Matrix matrix)
            : this(
                matrix.Elements[0], matrix.Elements[1], matrix.OffsetX,
                matrix.Elements[2], matrix.Elements[3], matrix.OffsetY)
        {
            
        }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="affineTransformVector"></param>
        public AffineCoordinateTransformation2D(params double[] affineTransformVector)
        {
            _a = affineTransformVector[0];
            _b = affineTransformVector[1];
            _c = affineTransformVector[2];

            _d = affineTransformVector[3];
            _e = affineTransformVector[4];
            _f = affineTransformVector[5];

            //var mat = new System.Drawing.Drawing2D.Matrix((float)_a, (float)_b, 
            //    (float)_d, (float)_e, (float)_c, (float)_f);
            //var matInv = mat.Clone();
            //matInv.Invert();

            ////
            //var m = ToMatrix();
            var minv = ToMatrix();
            var rep = new alglib.matinv.matinvreport();
            var info = 0;
            alglib.matinv.rmatrixinverse(ref minv, 3, ref info, rep);
            _ainv = minv[0, 0];
            _binv = minv[0, 1];
            _cinv = minv[2, 0];
            _dinv = minv[1, 0];
            _einv = minv[1, 1];
            _finv = minv[2, 1];

            
        }

        public AffineCoordinateTransformation2D(double[,] matrix)
            :this(matrix[0,0], matrix[0,1], matrix[2,0], 
                  matrix[1,0], matrix[1,1], matrix[2,1])
        {
        }

        private double[,] ToMatrix()
        {
            var elem = new double[3,3];
            elem[0, 0] = _a;
            elem[0, 1] = _b;
            //elem[0, 2] = 1d;
            elem[1, 0] = _d;
            elem[1, 1] = _e;
            //elem[1, 2] = 1d;
            elem[2, 0] = _c;
            elem[2, 1] = _f;
            elem[2, 2] = 1d;
            return elem;
        }

        #region Overrides of MathTransform

        public override GeoAPI.CoordinateSystems.Transformations.IMathTransform Inverse()
        {
            //#warning(System.Drawing.Drawing2D.Matrix uses single precision floating point numbers. This involves reduction of precision, not at all accurate!)

            //var m = ToMatrix();
            //var rep = new alglib.matinv.matinvreport();
            //var info = 0;
            //alglib.matinv.rmatrixinverse(ref m, 3, ref info, rep);
            return new AffineCoordinateTransformation2D(_ainv, _binv, _cinv, _dinv, _einv, _finv);
        }

        public override double[] Transform(double[] point)
        {
            /*
            Converting from input (X,Y) to output coordinate system (X',Y') is done by:
            X' = a*X + b*Y + c, 
            Y' = d*X + e*Y + f
             */
            System.Diagnostics .Debug.Assert(point.Length == 2);
            if (IsInverse)
            {
                return new []
                           {
                               _ainv*point[0] + _binv*point[1] + _cinv,
                               _dinv*point[0] + _einv*point[1] + _finv
                           };
            }
            return new []
                       {
                           _a*point[0] + _b*point[1] + _c,
                           _d*point[0] + _e*point[1] + _f
                       };
        }

        public override System.Collections.Generic.IList<double[]> TransformList(System.Collections.Generic.IList<double[]> points)
        {
            System.Collections.Generic.List<System.Double[]> ret = new System.Collections.Generic.List<System.Double[]>(points.Count);
            foreach (var d in points)
                ret.Add(Transform(d));
            return ret;
        }

        /// <summary>
        /// Gets a value indicating whether this transfrom is in inverse mode or not
        /// </summary>
        public bool IsInverse { get; private set;}

        public override void Invert()
        {
            IsInverse = !IsInverse;
        }

        public override int DimSource
        {
            get { throw new System.NotImplementedException(); }
        }

        public override int DimTarget
        {
            get { throw new System.NotImplementedException(); }
        }

        public override string WKT
        {
            get { throw new System.NotImplementedException(); }
        }

        public override string XML
        {
            get { throw new System.NotImplementedException(); }
        }

        #endregion

        #region Implementation of ICoordinateTransformation

        public string AreaOfUse
        {
            get { throw new System.NotImplementedException(); }
        }

        public string Authority
        {
            get { throw new System.NotImplementedException(); }
        }

        public long AuthorityCode
        {
            get { throw new System.NotImplementedException(); }
        }

        public GeoAPI.CoordinateSystems.Transformations.IMathTransform MathTransform
        {
            get { return this; }
        }

        public string Name
        {
            get { return "AffineCoordinateTransformation2D"; }
        }

        public string Remarks
        {
            get { return ""; }
        }

        public GeoAPI.CoordinateSystems.ICoordinateSystem SourceCS
        {
            get { return null; }
        }

        public GeoAPI.CoordinateSystems.ICoordinateSystem TargetCS
        {
            get { return null; }
        }

        public GeoAPI.CoordinateSystems.Transformations.TransformType TransformType
        {
            get { return GeoAPI.CoordinateSystems.Transformations.TransformType.Transformation; }
        }

        #endregion
    }

    [NUnit.Framework.TestFixture]
    public class AffineTransformSample
    {

        private static readonly System.Random RandomNumberGenerator = new System.Random(778564);
        static System.Double[] GetRandomOrdinates(System.Int32 size, System.Double min, System.Double max)
        {
            System.Double[] arr = new System.Double[size];
            System.Double width = max - min;
            for (System.Int32 i = 0; i < size; i++)
            {
                System.Double randomValue = RandomNumberGenerator.NextDouble();
                arr[i] = min + randomValue * width;
            }
            return arr;
        }

        private static SharpMap.Data.FeatureDataTable TransformedFeatureDataTable(
            System.Drawing.Drawing2D.Matrix matrix, SharpMap.Data.FeatureDataTable fdt)
        {
            SharpMap.Data.FeatureDataTable fdtClone = new SharpMap.Data.FeatureDataTable(fdt);
            fdtClone.Clear();
            foreach (SharpMap.Data.FeatureDataRow row in fdt)
            {
                SharpMap.Data.FeatureDataRow newRow = fdtClone.NewRow();
                for (System.Int32 i = 0; i < fdtClone.Columns.Count; i++)
                    newRow[i] = row[i];

                GeoAPI.Geometries.IPoint smpt = (GeoAPI.Geometries.IPoint)row.Geometry;
                System.Drawing.PointF[] pts = new System.Drawing.PointF[] 
                    { new System.Drawing.PointF((float)smpt.X, (float)smpt.Y) };
                matrix.TransformPoints(pts);
                newRow.Geometry = new NetTopologySuite.Geometries.Point(pts[0].X, pts[0].Y);

                fdtClone.AddRow(newRow);
            }

            return fdtClone;
        }

        [NUnit.Framework.Test]
        public void TestMatrix()
        {
            var p = new NetTopologySuite.Geometries.Point(10, 10);
            var b = p.AsBinary();
            
            System.Drawing.Drawing2D.Matrix mat = new System.Drawing.Drawing2D.Matrix();
            mat.Rotate(30);
            mat.Translate(-20, 20);
            System.Drawing.PointF[] pts = new System.Drawing.PointF[] { new System.Drawing.PointF(50, 50) };

            mat.TransformPoints(pts);
            System.Diagnostics.Debug.WriteLine(string.Format("POINT ({0} {1})", pts[0].X, pts[0].Y));
            System.Drawing.PointF ptt = pts[0];
            System.Drawing.PointF[] ptts = new System.Drawing.PointF[] { new System.Drawing.PointF(ptt.X, ptt.Y) };
            System.Drawing.Drawing2D.Matrix inv = mat.Clone();
            inv.Invert();
            inv.TransformPoints(ptts);
            NUnit.Framework.Assert.LessOrEqual(System.Math.Abs(ptts[0].X - 50f), 0.01);
            NUnit.Framework.Assert.LessOrEqual(System.Math.Abs(ptts[0].Y - 50f), 0.01);
        
        }

        [NUnit.Framework.Test]
        public void TestMatrix2()
        {
            System.Drawing.Drawing2D.Matrix mat = new System.Drawing.Drawing2D.Matrix();
            mat.Rotate(30);
            mat.Translate(-20, 20);

            var at = new AffineCoordinateTransformation2D(mat);
            var atInv = at.Inverse();

            var p0 = new double[] { 50d, 50d };
            var pt = at.Transform(p0);
            at.Invert();
            var p1 = at.Transform(pt);
            NUnit.Framework.Assert.LessOrEqual(System.Math.Abs(p1[0] - p0[0]), 0.01d);
            NUnit.Framework.Assert.LessOrEqual(System.Math.Abs(p1[1] - p0[1]), 0.01d);
            var p2 = atInv.Transform(pt);
            NUnit.Framework.Assert.LessOrEqual(System.Math.Abs(p2[0] - p0[0]), 0.01d);
            NUnit.Framework.Assert.LessOrEqual(System.Math.Abs(p2[1] - p0[1]), 0.01d);

            
            System.Drawing.PointF[] pts = new System.Drawing.PointF[] { new System.Drawing.PointF(50, 50) };

            mat.TransformPoints(pts);
            System.Diagnostics.Debug.WriteLine(string.Format("POINT ({0} {1})", pts[0].X, pts[0].Y));
            System.Drawing.PointF ptt = pts[0];
            System.Drawing.PointF[] ptts = new System.Drawing.PointF[] { new System.Drawing.PointF(ptt.X, ptt.Y) };
            System.Drawing.Drawing2D.Matrix inv = mat.Clone();
            inv.Invert();
            inv.TransformPoints(ptts);
            NUnit.Framework.Assert.LessOrEqual(System.Math.Abs(ptts[0].X - 50f), 0.01);
            NUnit.Framework.Assert.LessOrEqual(System.Math.Abs(ptts[0].Y - 50f), 0.01);

        }

        [NUnit.Framework.Test]
        public void TestAffineTransform2D()
        {
            //Setup some affine transformation
            System.Drawing.Drawing2D.Matrix matrix = new System.Drawing.Drawing2D.Matrix();
            matrix.RotateAt(30, new System.Drawing.PointF(0, 0));
            matrix.Translate(-20, -20, System.Drawing.Drawing2D.MatrixOrder.Append);
            matrix.Shear(0.95f, -0.2f, System.Drawing.Drawing2D.MatrixOrder.Append);
            
            //Create some random sample data
            SharpMap.Data.FeatureDataTable fdt1 =
                CreatingData.CreatePointFeatureDataTableFromArrays(GetRandomOrdinates(80, -180, 180),
                                                                   GetRandomOrdinates(80, -90, 90), null);

            //Clone random sample data and apply affine transformation on it
            SharpMap.Data.FeatureDataTable fdt2 = TransformedFeatureDataTable(matrix, fdt1);

            //Get affine transformation with LeastSquaresTransform
            SharpMap.Utilities.LeastSquaresTransform lst = new SharpMap.Utilities.LeastSquaresTransform();

            //Add at least three corresponding points
            lst.AddInputOutputPoint(
                ((SharpMap.Data.FeatureDataRow)fdt1.Rows[0]).Geometry.Coordinate,
                ((SharpMap.Data.FeatureDataRow)fdt2.Rows[0]).Geometry.Coordinate);

            lst.AddInputOutputPoint(
                ((SharpMap.Data.FeatureDataRow)fdt1.Rows[39]).Geometry.Coordinate,
                ((SharpMap.Data.FeatureDataRow)fdt2.Rows[39]).Geometry.Coordinate);

            lst.AddInputOutputPoint(
                ((SharpMap.Data.FeatureDataRow)fdt1.Rows[79]).Geometry.Coordinate,
                ((SharpMap.Data.FeatureDataRow)fdt2.Rows[79]).Geometry.Coordinate);

            /*
            //Get affine transformation calculates mean points to improve accuaracy
            //Unfortunately the result is not very good, so, since I know better I manually set these
            //mean points.
            lst.SetMeanPoints(new GeoAPI.Geometries.IPoint(0, 0), 
                              new GeoAPI.Geometries.IPoint(matrix.OffsetX, matrix.OffsetY));
             */

            //Create Affine
            AffineCoordinateTransformation2D at2 = new AffineCoordinateTransformation2D(lst.GetAffineTransformation());

            //Create Map
            SharpMap.Map map = new SharpMap.Map(new System.Drawing.Size(720, 360));

            //Add not transformed layer
            map.Layers.Add(new SharpMap.Layers.VectorLayer("L1",
                                                           new SharpMap.Data.Providers.GeometryFeatureProvider(fdt1)));
            ((SharpMap.Layers.VectorLayer) map.Layers[0]).Style.Symbol =
                new System.Drawing.Bitmap(@"..\..\..\DemoWinForm\Resources\flag.png");

            //Add transformed layer
            map.Layers.Add(new SharpMap.Layers.VectorLayer("L2",
                                                           new SharpMap.Data.Providers.GeometryFeatureProvider(fdt2)));
            ((SharpMap.Layers.VectorLayer) map.Layers[1]).Style.Symbol =
                new System.Drawing.Bitmap(@"..\..\..\DemoWinForm\Resources\women.png");

            //Render map
            map.ZoomToExtents();

            //Get map and save to file
            var bmp = (System.Drawing.Bitmap)map.GetMap();
            bmp.Save("affinetransform1.bmp");

            //we want to reverse the previously applied transformation.
            ((SharpMap.Layers.VectorLayer) map.Layers[1]).CoordinateTransformation = (AffineCoordinateTransformation2D)at2.Inverse();

            //Render map
            map.ZoomToExtents();

            //Get map and save to file
            bmp = (System.Drawing.Bitmap)map.GetMap();
            bmp.Save("affinetransform2.bmp");
            //Hopefully women cover flags ;-).

        }
    }
    
    #endif

        [NUnit.Framework.Test]
        public void TestGdalRasterLayer()
        {
            if (!System.IO.File.Exists("D:\\Daten\\zone49_mga.ecw"))
                NUnit.Framework.Assert.Ignore("Adjust file path");
            if (!System.IO.File.Exists("D:\\Daten\\zone50_mga.ecw"))
                NUnit.Framework.Assert.Ignore("Adjust file path");

            var ecw1 = new SharpMap.Layers.GdalRasterLayer("zone49", "D:\\Daten\\zone49_mga.ecw");
            var ecw2 = new SharpMap.Layers.GdalRasterLayer("zone50", "D:\\Daten\\zone50_mga.ecw");

            var p1 = ecw1.GetProjection();
            ecw2.ReprojectToCoordinateSystem(p1);

            var m = new SharpMap.Map(new System.Drawing.Size( 1024, 768));
            m.Layers.Add(ecw1);
            m.Layers.Add(ecw2);

            m.ZoomToExtents();
            using (var img = m.GetMap())
            {
                img.Save("ecw.png");
            }
        }

public static void ReprojectFeatureDataSet(SharpMap.Data.FeatureDataSet fds,
    GeoAPI.CoordinateSystems.ICoordinateSystem target)
{
    for (var i = 0; i < fds.Tables.Count; i ++)
    {
        var fdt = fds.Tables[i];
        ReprojectFeatureDataTable(fdt, target);
    }

}

public static void ReprojectFeatureDataTable(SharpMap.Data.FeatureDataTable fdt,
    GeoAPI.CoordinateSystems.ICoordinateSystem target)
{
    var source = SharpMap.CoordinateSystems.CoordinateSystemExtensions.GetCoordinateSystem(fdt[0].Geometry);

    var ctFactory = new ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory();
    var ct = ctFactory.CreateFromCoordinateSystems(source, target);
            
    var geomFactory = GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory((int)target.AuthorityCode);

    for (var i = 0; i < fdt.Rows.Count; i++)
    {
        var fdr = fdt[i];
        fdr.Geometry =
            GeoAPI.CoordinateSystems.Transformations.GeometryTransform.TransformGeometry(fdr.Geometry,
                ct.MathTransform, geomFactory);
    }
}
    }
}
