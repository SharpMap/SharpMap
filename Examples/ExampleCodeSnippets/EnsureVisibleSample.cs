
using System;
using System.Drawing;
using GeoAPI.Geometries;
using NUnit.Framework;

namespace ExampleCodeSnippets
{
    using cd = CreatingData;
        [NUnit.Framework.TestFixture]
    public class EnsureVisibleSample
    {
public static void EnsureVisible(SharpMap.Map map, GeoAPI.Geometries.Coordinate pt)
{
    const double ensureVisibleRatio = 0.1d;
            
    //Get current map envelope
    var bb = map.Envelope;
    System.Console.WriteLine(string.Format("Map envelope: {0}", bb));
            
    //Set valid envelope
    var evbb = bb.Grow(- ensureVisibleRatio * bb.Width, -ensureVisibleRatio * bb.Height );
    System.Console.WriteLine(string.Format("Valid envelope: {0}", evbb));
            
    //Test if Point is in valid envelope
    if (evbb.Contains(pt)) return;

    //It is not
    System.Console.WriteLine(string.Format("Valid envelope does not contain {0}", pt));

    //LineString from Map.Center -> to Point
    var ls = map.Factory.CreateLineString(new[] {evbb.Centre, pt});
    System.Console.WriteLine(string.Format("LineString Map.Center -> Point: {0}", ls));

    //Setup Linestring from BoundingBox
    var evbbpts = new [] {evbb.TopLeft(), evbb.TopRight(), evbb.BottomRight(), evbb.BottomLeft(), evbb.TopLeft() };
    var evbblinearring = map.Factory.CreateLineString(evbbpts);
    System.Console.WriteLine(string.Format("Linestring of valid envelope: {0}", evbblinearring));

    //// convert geometries to NTS
    //var ntsevbb = (NetTopologySuite.Geometries.LineString)
    //    SharpMap.Converters.NTS.GeometryConverter.ToNTSGeometry(evbblinearring, gf);
    //var ntsls = (NetTopologySuite.Geometries.LineString)
    //    SharpMap.Converters.NTS.GeometryConverter.ToNTSGeometry(ls, gf);

    // Get intersection point
    var intGeo = evbblinearring.Intersection(ls);
    var intPt = (NetTopologySuite.Geometries.Point)intGeo;
    System.Console.WriteLine(string.Format("Intersection point is: {0}", intPt));

    //Compute offset
    var dx = pt.X - intPt.X;
    var dy = pt.Y - intPt.Y;
    System.Console.WriteLine(string.Format("Map.Center needs to be shifted by: [{0}, {1}]", dx, dy));

    //Set new center Center
    map.Center = new GeoAPI.Geometries.Coordinate(map.Center.X + dx, map.Center.Y + dy);

}

        [NUnit.Framework.Test]
        public void TestEnsureVisible()
        {
            //Create a map
            SharpMap.Map map = new SharpMap.Map(new System.Drawing.Size(720,360));
            
            //Create some random sample data
            SharpMap.Data.FeatureDataTable fdt =
                cd.CreatePointFeatureDataTableFromArrays(cd.GetRandomOrdinates(80, -180, 180),
                                                         cd.GetRandomOrdinates(80, -90, 90), null);

            //Create layer and datasource
            SharpMap.Layers.VectorLayer vl = new SharpMap.Layers.VectorLayer("Points", new SharpMap.Data.Providers.GeometryFeatureProvider(fdt));
            
            //Create default style
            SharpMap.Styles.VectorStyle defaultStyle = new SharpMap.Styles.VectorStyle();
            defaultStyle.Symbol = new System.Drawing.Bitmap(@"..\..\..\DemoWinForm\Resources\flag.png");
            defaultStyle.SymbolScale = 0.5f;

            //Create theming class and apply to layer
            var srt = new SymbolRotationTheming("Rotation", defaultStyle);
            vl.Theme = new SharpMap.Rendering.Thematics.CustomTheme(srt.GetRotatedSymol);


            map.Layers.Add(vl);
            map.ZoomToExtents();
            map.Zoom = 60; //2*30
            map.Center = new GeoAPI.Geometries.Coordinate(0,0);

            System.Console.WriteLine(map.Center);
            EnsureVisible(map, new GeoAPI.Geometries.Coordinate(-30, 0));
            System.Console.WriteLine(map.Center);
            System.Console.WriteLine();
            EnsureVisible(map, new GeoAPI.Geometries.Coordinate(15, 20));
            System.Console.WriteLine(map.Center);
            System.Console.WriteLine();
            EnsureVisible(map, new GeoAPI.Geometries.Coordinate(15, -20));
            System.Console.WriteLine(map.Center);
        }

        [Test, Ignore("Utility, not a test")]
        public void CreateKnownColors()
        {
            Console.WriteLine("/// <summary>");
            Console.WriteLine("/// Attribute class to associate ARGB value with <see cref=\"KnownColor\" enum member");
            Console.WriteLine("/// </summary>");
            Console.WriteLine("[System.AttributeUsage(System.AttributeTargets.Field)]");
            Console.WriteLine("internal class ArgbValueAttribute : System.Attribute\n{");
            Console.WriteLine("    /// <summary>");
            Console.WriteLine("    /// Creates an instance of this class");
            Console.WriteLine("    /// </summary>");
            Console.WriteLine("    /// <param name=\"argb\">The ARGB value</param>");
            Console.WriteLine("    public ArgbValueAttribute(int argb)\n    {");
            Console.WriteLine("            Argb = argb;\n    }");
            Console.WriteLine("    /// <summary>");
            Console.WriteLine("    /// Gets a value indicating the ARGB value");
            Console.WriteLine("    /// </summary>");
            Console.WriteLine("    public int Argb { get; }");
            Console.WriteLine("}");

            Console.WriteLine("/// <summary>");
            Console.WriteLine("/// Straight copy of <see cref=\"System.Drawing.KnownColor\"/> names");
            Console.WriteLine("/// </summary>");
            Console.WriteLine("internal enum KnownColor\n{");
            foreach (System.Drawing.KnownColor knownColor in System.Enum.GetValues(typeof(System.Drawing.KnownColor)))
            {
                string knownColorName = Enum.GetName(typeof(System.Drawing.KnownColor), knownColor);
                Console.WriteLine("\t/// <summary>");
                Console.WriteLine("\t/// Color {0} (#{1:X})", knownColorName, 0x00FFFFFF & Color.FromKnownColor(knownColor).ToArgb());
                Console.WriteLine("\t/// </summary>");
                Console.WriteLine("\t[ArgbValue({0})]", Color.FromKnownColor(knownColor).ToArgb());
                Console.WriteLine("\t{0} = {1},", knownColorName, (int)knownColor);
            }
            Console.WriteLine("}");
        }

    }

}
