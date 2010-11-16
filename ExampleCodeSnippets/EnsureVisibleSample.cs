namespace ExampleCodeSnippets
{
        [NUnit.Framework.TestFixture]
    public class EnsureVisibleSample
    {
public static void EnsureVisible(SharpMap.Map map, SharpMap.Geometries.Point pt)
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
    var ls = new SharpMap.Geometries.LineString(
        new SharpMap.Geometries.Point[] {evbb.GetCentroid(), pt});
    System.Console.WriteLine(string.Format("LineString Map.Center -> Point: {0}", ls));

    //Setup Linestring from BoundingBox
    var gf = new GisSharpBlog.NetTopologySuite.Geometries.GeometryFactory();
    var evbbpts = new System.Collections.Generic.List<SharpMap.Geometries.Point>(
        new SharpMap.Geometries.Point[] {evbb.TopLeft, evbb.TopRight, 
            evbb.BottomRight, evbb.BottomLeft, evbb.TopLeft });
    var evbblinearring = new SharpMap.Geometries.LineString(evbbpts);
    System.Console.WriteLine(string.Format("Linestring of valid envelope: {0}", evbblinearring));

    // convert geometries to NTS
    var ntsevbb = (GisSharpBlog.NetTopologySuite.Geometries.LineString)
        SharpMap.Converters.NTS.GeometryConverter.ToNTSGeometry(evbblinearring, gf);
    var ntsls = (GisSharpBlog.NetTopologySuite.Geometries.LineString)
        SharpMap.Converters.NTS.GeometryConverter.ToNTSGeometry(ls, gf);

    // Get intersection point
    var intGeo = ntsevbb.Intersection(ntsls);
    var intPt = (GisSharpBlog.NetTopologySuite.Geometries.Point)intGeo;
    System.Console.WriteLine(string.Format("Intersection point is: {0}", intPt));

    //Compute offset
    var dx = pt.X - intPt.X;
    var dy = pt.Y - intPt.Y;
    System.Console.WriteLine(string.Format("Map.Center needs to be shifted by: [{0}, {1}]", dx, dy));

    //Set new center Center
    map.Center = new SharpMap.Geometries.Point(map.Center.X + dx, map.Center.Y + dy);

}

        private static readonly System.Random _randomNumberGenerator = new System.Random();
        static System.Double[] GetRandomOrdinates(System.Int32 size, System.Double min, System.Double max)
        {
            System.Double[] arr = new System.Double[size];
            System.Double width = max - min;
            for(System.Int32 i = 0; i < size; i++)
            {
                System.Double randomValue = _randomNumberGenerator.NextDouble();
                arr[i] = min + randomValue*width;
            }
            return arr;
        }
        
        [NUnit.Framework.Test]
        public void TestEnsureVisible()
        {
            //Create a map
            SharpMap.Map map = new SharpMap.Map(new System.Drawing.Size(720,360));
            
            //Create some random sample data
            CreatingData cd = new CreatingData();
            SharpMap.Data.FeatureDataTable fdt =
                cd.CreatePointFeatureDataTableFromArrays(GetRandomOrdinates(80, -180, 180),
                                                         GetRandomOrdinates(80, -90, 90), null);

            //Create layer and datasource
            SharpMap.Layers.VectorLayer vl = new SharpMap.Layers.VectorLayer("Points", new SharpMap.Data.Providers.GeometryFeatureProvider(fdt));
            
            //Create default style
            SharpMap.Styles.VectorStyle defaultStyle = new SharpMap.Styles.VectorStyle();
            defaultStyle.Symbol = new System.Drawing.Bitmap(@"..\..\..\DemoWinForm\Resources\flag.png");
            defaultStyle.SymbolScale = 0.5f;

            //Create theming class and apply to layer
            SymbolRotationTheming srt = new SymbolRotationTheming("Rotation", defaultStyle);
            vl.Theme = new SharpMap.Rendering.Thematics.CustomTheme(srt.GetRotatedSymol);


            map.Layers.Add(vl);
            map.ZoomToExtents();
            map.Zoom = 60; //2*30
            map.Center = new SharpMap.Geometries.Point(0,0);

            System.Console.WriteLine(map.Center);
            EnsureVisible(map, new SharpMap.Geometries.Point(-30, 0));
            System.Console.WriteLine(map.Center);
            System.Console.WriteLine();
            EnsureVisible(map, new SharpMap.Geometries.Point(15, 20));
            System.Console.WriteLine(map.Center);
            System.Console.WriteLine();
            EnsureVisible(map, new SharpMap.Geometries.Point(15, -20));
            System.Console.WriteLine(map.Center);
        }
    }




}