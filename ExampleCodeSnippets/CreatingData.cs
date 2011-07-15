namespace ExampleCodeSnippets
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
            bool threedee = false;
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

            SharpMap.Data.FeatureDataTable fdt = new SharpMap.Data.FeatureDataTable();
            fdt.Columns.Add("TimeStamp", typeof (System.DateTime)); // add example timestamp attribute
            for (int i = 0; i < xcomponents.Length; i++)
            {
                SharpMap.Data.FeatureDataRow fdr = fdt.NewRow();

                fdr.Geometry = threedee
                                   ? new SharpMap.Geometries.Point3D(xcomponents[i], ycomponents[i], zcomponents[i])
                                   : new SharpMap.Geometries.Point(xcomponents[i], ycomponents[i]);

                fdr["TimeStamp"] = System.DateTime.Now; //set the timestamp property
                fdt.AddRow(fdr);
            }

            return fdt;
        }
    }

    public static class ShapeFactory
    {
        public static SharpMap.Geometries.LinearRing CreateRectangle(SharpMap.Geometries.Point leftTop, SharpMap.Geometries.Point rightBottom)
        {
            var pts = new[]
                            {
                                leftTop, 
                                new SharpMap.Geometries.Point(rightBottom.X, leftTop.Y), 
                                rightBottom,
                                new SharpMap.Geometries.Point(leftTop.X, rightBottom.Y), 
                                leftTop
                            };
            return new SharpMap.Geometries.LinearRing(pts);
        }

        public static SharpMap.Geometries.LinearRing CreateRectangle(SharpMap.Geometries.Point center, System.Drawing.SizeF size)
        {
            var wh = new System.Drawing.SizeF(size.Width * 0.5f, size.Height * 0.5f);
            var lt = new SharpMap.Geometries.Point(center.X - wh.Width, center.Y + wh.Height);
            var rb = new SharpMap.Geometries.Point(center.X + wh.Width, center.Y - wh.Height);

            return CreateRectangle(lt, rb);
        }

        public static SharpMap.Geometries.LinearRing CreateEllipse(SharpMap.Geometries.Point center, System.Drawing.SizeF size)
        {
            return CreateEllipse(center, size, 12);
        }

        public static SharpMap.Geometries.LinearRing CreateEllipse(SharpMap.Geometries.Point center,
                                                                    System.Drawing.SizeF size,
                                                                    int segmentsPerQuadrant)
        {
            const double piHalf = System.Math.PI * 0.5d;

            var step = piHalf / segmentsPerQuadrant;

            var pts = new SharpMap.Geometries.Point[4 * segmentsPerQuadrant + 1];
            var angle = 0d;
            for (var i = 0; i < 4 * segmentsPerQuadrant; i++)
            {
                pts[i] = new SharpMap.Geometries.Point(center.X + System.Math.Cos(angle) * size.Width,
                                                       center.Y + System.Math.Sin(angle) * size.Height);
                angle += step;
            }
            pts[pts.Length - 1] = pts[0];
            return new SharpMap.Geometries.LinearRing(pts);
        }
    }

    public class TestShapeFactory
    {
        [NUnit.Framework.Test]
        public void TestRectangle()
        {
            var rect = ShapeFactory.CreateRectangle(new SharpMap.Geometries.Point(1, 2), new SharpMap.Geometries.Point(2, 1));
            NUnit.Framework.Assert.AreEqual(rect.StartPoint, rect.EndPoint);
            NUnit.Framework.Assert.AreEqual(4, rect.Length);

            var rect2 = ShapeFactory.CreateRectangle(new SharpMap.Geometries.Point(1.5, 1.5), new System.Drawing.SizeF(1f, 1f));
            NUnit.Framework.Assert.AreEqual(rect, rect2);

            NUnit.Framework.Assert.AreEqual("LINESTRING (1 2, 2 2, 2 1, 1 1, 1 2)", rect.ToString());
        }
        [NUnit.Framework.Test]
        public void TestEllipse()
        {
            var ell = ShapeFactory.CreateEllipse(new SharpMap.Geometries.Point(1, 1), new System.Drawing.SizeF(1, 1), 1);
            NUnit.Framework.Assert.AreEqual(ell.StartPoint, ell.EndPoint);
            NUnit.Framework.Assert.LessOrEqual(System.Math.Abs(4 * System.Math.Sqrt(2) - ell.Length), 0.00001d);


            NUnit.Framework.Assert.AreEqual("LINESTRING (2 1, 1 2, 0 1, 1 0, 2 1)", ell.ToString());
            var ell2 = ShapeFactory.CreateEllipse(new SharpMap.Geometries.Point(1, 1), new System.Drawing.SizeF(1, 1));
            System.Console.WriteLine(ell2.ToString());
        }

        [NUnit.Framework.Test]
        public void TestLayer()
        {
            var map = new SharpMap.Map(new System.Drawing.Size(500, 500));
            var gp = new SharpMap.Data.Providers.GeometryProvider(
                new SharpMap.Geometries.Geometry[]
                    {
                        new SharpMap.Geometries.Polygon(ShapeFactory.CreateEllipse(new SharpMap.Geometries.Point(0, 0), new System.Drawing.SizeF(40, 30))),
                        ShapeFactory.CreateEllipse(new SharpMap.Geometries.Point(90, 55), new System.Drawing.SizeF(40, 30)),
                        ShapeFactory.CreateEllipse(new SharpMap.Geometries.Point(77, 24), new System.Drawing.SizeF(40, 30)),
                        ShapeFactory.CreateEllipse(new SharpMap.Geometries.Point(-80, 41), new System.Drawing.SizeF(40, 30)),
                        ShapeFactory.CreateEllipse(new SharpMap.Geometries.Point(-45, -36), new System.Drawing.SizeF(40, 30)),
                    });
            var gl = new SharpMap.Layers.VectorLayer("GeometryLayer", gp);
            map.Layers.Add(gl);
            map.ZoomToExtents();
            var mapimage = map.GetMap();
            mapimage.Save("ellipse.png", System.Drawing.Imaging.ImageFormat.Png);
        }
    }
}