namespace UnitTests.TestData
{
    /// <summary>
    /// Examples of creating spatial data 
    /// </summary>
    public static class CreatingData
    {
        private static readonly System.Random _rng = new System.Random();

        public static System.Random RandomNumberGenerator
        {
            get { return _rng; }
        }

        /// <summary>
        /// Creates an array of random values in the given range [<paramref name="min"/>, <paramref name="max"/>].
        /// </summary>
        /// <param name="size">The length of the array, aka the number of values</param>
        /// <param name="min">The lower bound.</param>
        /// <param name="max">The upper bound.</param>
        /// <returns>An array of random values</returns>
        public static System.Double[] GetRandomOrdinates(System.Int32 size, System.Double min, System.Double max)
        {
            System.Double[] arr = new System.Double[size];
            System.Double width = max - min;
            for (System.Int32 i = 0; i < size; i++)
            {
                System.Double randomValue = _rng.NextDouble();
                arr[i] = min + randomValue * width;
            }
            return arr;
        }

        
        /// <summary>
        /// Creates a FeatureDataTable from arrays of x, y and z components
        /// </summary>
        /// <param name="xcomponents">an array of doubles representing the x ordinate values</param>
        /// <param name="ycomponents">an array of doubles representing the y ordinate values</param>
        /// <param name="zcomponents">an array of doubles representing the z ordinate values</param>
        /// <returns></returns>
        public static SharpMap.Data.FeatureDataTable CreatePointFeatureDataTableFromArrays(double[] xcomponents, 
                                                                 double[] ycomponents,
                                                                 double[] zcomponents)
        {
            var factory = new NetTopologySuite.Geometries.GeometryFactory();
            var threedee = false;
            if (zcomponents != null)
            {
                if (!(zcomponents.Length == ycomponents.Length && zcomponents.Length == xcomponents.Length))
                    throw new System.ApplicationException("Mismatched Array Lengths");

                threedee = true;
            }
            else
            {
                if (ycomponents.Length != xcomponents.Length)
                    throw new System.ApplicationException("Mismatched Array Lengths");
            }

            var fdt = new SharpMap.Data.FeatureDataTable();
            fdt.Columns.Add("TimeStamp", typeof (System.DateTime)); // add example timestamp attribute
            for (var i = 0; i < xcomponents.Length; i++)
            {
                SharpMap.Data.FeatureDataRow fdr = fdt.NewRow();

                fdr.Geometry = factory.CreatePoint(threedee
                                   ? new GeoAPI.Geometries.Coordinate(xcomponents[i], ycomponents[i], zcomponents[i])
                                   : new GeoAPI.Geometries.Coordinate(xcomponents[i], ycomponents[i]));

                fdr["TimeStamp"] = System.DateTime.Now; //set the timestamp property
                fdt.AddRow(fdr);
            }

            return fdt;
        }
    }

    public static class ShapeFactory
    {
        public static GeoAPI.Geometries.ILinearRing CreateRectangle(GeoAPI.Geometries.IGeometryFactory factory, 
            GeoAPI.Geometries.Coordinate leftTop, GeoAPI.Geometries.Coordinate rightBottom)
        {
            var pts = new[]
                            {
                                leftTop, 
                                new GeoAPI.Geometries.Coordinate(rightBottom.X, leftTop.Y), 
                                rightBottom,
                                new GeoAPI.Geometries.Coordinate(leftTop.X, rightBottom.Y), 
                                leftTop
                            };
            return factory.CreateLinearRing(pts);
        }

        public static GeoAPI.Geometries.ILinearRing CreateRectangle(GeoAPI.Geometries.IGeometryFactory factory,
            GeoAPI.Geometries.Coordinate center, System.Drawing.SizeF size)
        {
            var wh = new System.Drawing.SizeF(size.Width * 0.5f, size.Height * 0.5f);
            var lt = new GeoAPI.Geometries.Coordinate(center.X - wh.Width, center.Y + wh.Height);
            var rb = new GeoAPI.Geometries.Coordinate(center.X + wh.Width, center.Y - wh.Height);

            return CreateRectangle(factory, lt, rb);
        }

        public static GeoAPI.Geometries.ILinearRing CreateEllipse(GeoAPI.Geometries.IGeometryFactory factory,
            GeoAPI.Geometries.Coordinate center, System.Drawing.SizeF size)
        {
            return CreateEllipse(factory, center, size, 12);
        }

        public static GeoAPI.Geometries.ILinearRing CreateEllipse(GeoAPI.Geometries.IGeometryFactory factory,
            GeoAPI.Geometries.Coordinate center, System.Drawing.SizeF size, int segmentsPerQuadrant)
        {
            const double piHalf = System.Math.PI * 0.5d;

            var step = piHalf / segmentsPerQuadrant;

            var pts = new GeoAPI.Geometries.Coordinate[4 * segmentsPerQuadrant + 1];
            var angle = 0d;
            for (var i = 0; i < 4 * segmentsPerQuadrant; i++)
            {
                pts[i] = new GeoAPI.Geometries.Coordinate(center.X + System.Math.Cos(angle) * size.Width,
                                                          center.Y + System.Math.Sin(angle) * size.Height);
                angle += step;
            }
            pts[pts.Length - 1] = pts[0];
            return factory.CreateLinearRing(pts);
        }
    }

    public class TestShapeFactory
    {
        public static readonly GeoAPI.Geometries.IGeometryFactory Factory =
            new NetTopologySuite.Geometries.GeometryFactory(new NetTopologySuite.Geometries.PrecisionModel(0.01));
        
        [NUnit.Framework.Test]
        public void TestRectangle()
        {
            var rect = ShapeFactory.CreateRectangle(Factory, new GeoAPI.Geometries.Coordinate(1, 2), new GeoAPI.Geometries.Coordinate(2, 1));
            NUnit.Framework.Assert.AreEqual(rect.StartPoint, rect.EndPoint);
            NUnit.Framework.Assert.AreEqual(4, rect.Length);

            var rect2 = ShapeFactory.CreateRectangle(Factory, new GeoAPI.Geometries.Coordinate(1.5, 1.5), new System.Drawing.SizeF(1f, 1f));
            NUnit.Framework.Assert.AreEqual(rect, rect2);

            NUnit.Framework.Assert.AreEqual("LINEARRING (1 2, 2 2, 2 1, 1 1, 1 2)", rect.ToString());
        }
        [NUnit.Framework.Test]
        public void TestEllipse()
        {
            var ell = ShapeFactory.CreateEllipse(Factory, new GeoAPI.Geometries.Coordinate(1, 1), new System.Drawing.SizeF(1, 1), 1);
            NUnit.Framework.Assert.AreEqual(ell.StartPoint, ell.EndPoint);
            NUnit.Framework.Assert.LessOrEqual(System.Math.Abs(4 * System.Math.Sqrt(2) - ell.Length), 0.00001d);


            NUnit.Framework.Assert.AreEqual("LINEARRING (2 1, 1 2, 0 1, 1 0, 2 1)", ell.ToString());
            var ell2 = ShapeFactory.CreateEllipse(Factory, new GeoAPI.Geometries.Coordinate(1, 1), new System.Drawing.SizeF(1, 1));
            System.Diagnostics.Trace.WriteLine(ell2.ToString());
        }

        [NUnit.Framework.Test]
        public void TestLayer()
        {
            var map = new SharpMap.Map(new System.Drawing.Size(500, 500));
            var gp = new SharpMap.Data.Providers.GeometryProvider(
                new[]
                    {
                        Factory.CreatePolygon(
                            ShapeFactory.CreateEllipse(Factory, new GeoAPI.Geometries.Coordinate(0, 0),
                                                       new System.Drawing.SizeF(40, 30)),
                            new[]
                                {
                                    ShapeFactory.CreateEllipse(Factory, new GeoAPI.Geometries.Coordinate(90, 55),
                                                               new System.Drawing.SizeF(40, 30)),
                                    ShapeFactory.CreateEllipse(Factory, new GeoAPI.Geometries.Coordinate(77, 24),
                                                               new System.Drawing.SizeF(40, 30)),
                                    ShapeFactory.CreateEllipse(Factory, new GeoAPI.Geometries.Coordinate(-80, 41),
                                                               new System.Drawing.SizeF(40, 30)),
                                    ShapeFactory.CreateEllipse(Factory, new GeoAPI.Geometries.Coordinate(-45, -36),
                                                               new System.Drawing.SizeF(40, 30)),
                                })
                    });
            var gl = new SharpMap.Layers.VectorLayer("GeometryLayer", gp);
            map.Layers.Add(gl);
            map.ZoomToExtents();
            using (var mapImage = map.GetMap())
                mapImage.Save(System.IO.Path.Combine(UnitTestsFixture.GetImageDirectory(this), "ellipse.png"), System.Drawing.Imaging.ImageFormat.Png);
        }
    }
}
