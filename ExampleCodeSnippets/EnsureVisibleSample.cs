namespace ExampleCodeSnippets
{
    using cd = CreatingData;
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
    var ls = new SharpMap.Geometries.LineString(new[] {evbb.GetCentroid(), pt});
    System.Console.WriteLine(string.Format("LineString Map.Center -> Point: {0}", ls));

    //Setup Linestring from BoundingBox
    var gf = new NetTopologySuite.Geometries.GeometryFactory();
    var evbbpts = new System.Collections.Generic.List<SharpMap.Geometries.Point>(
        new[] {evbb.TopLeft, evbb.TopRight, evbb.BottomRight, evbb.BottomLeft, evbb.TopLeft });
    var evbblinearring = new SharpMap.Geometries.LineString(evbbpts);
    System.Console.WriteLine(string.Format("Linestring of valid envelope: {0}", evbblinearring));

    // convert geometries to NTS
    var ntsevbb = (NetTopologySuite.Geometries.LineString)
        SharpMap.Converters.NTS.GeometryConverter.ToNTSGeometry(evbblinearring, gf);
    var ntsls = (NetTopologySuite.Geometries.LineString)
        SharpMap.Converters.NTS.GeometryConverter.ToNTSGeometry(ls, gf);

    // Get intersection point
    var intGeo = ntsevbb.Intersection(ntsls);
    var intPt = (NetTopologySuite.Geometries.Point)intGeo;
    System.Console.WriteLine(string.Format("Intersection point is: {0}", intPt));

    //Compute offset
    var dx = pt.X - intPt.X;
    var dy = pt.Y - intPt.Y;
    System.Console.WriteLine(string.Format("Map.Center needs to be shifted by: [{0}, {1}]", dx, dy));

    //Set new center Center
    map.Center = new SharpMap.Geometries.Point(map.Center.X + dx, map.Center.Y + dy);

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