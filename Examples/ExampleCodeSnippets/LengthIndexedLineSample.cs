

namespace ExampleCodeSnippets
{
    [NUnit.Framework.TestFixture]
    public class LengthIndexedLineSample
    {
public GeoAPI.Geometries.IMultiLineString SplitLineString(
    GeoAPI.Geometries.ILineString lineString, 
    System.Double length)
{
    if (lineString == null || lineString.IsEmpty)
        throw new System.ArgumentException("Linestring is null or Empty", "lineString");

    //var gf = new NetTopologySuite.Geometries.GeometryFactory();
    //var ntsLine = (NetTopologySuite.Geometries.LineString)
    //                SharpMap.Converters.NTS.GeometryConverter.ToNTSGeometry(lineString, gf);

    var ret = new System.Collections.Generic.List<GeoAPI.Geometries.ILineString>();
    var lil = new NetTopologySuite.LinearReferencing.LengthIndexedLine(lineString);

    double currentLength = 0d;
    while (currentLength  < lineString.Length)
    {
        var tmpLine = (GeoAPI.Geometries.ILineString)
            lil.ExtractLine(currentLength, currentLength + length);
        ret.Add(tmpLine);
        currentLength += length;
    }
    return lineString.Factory.CreateMultiLineString(ret.ToArray());
}

        [NUnit.Framework.Test]
        public void TestLengthIndexedLine()
        {
            var gf = new NetTopologySuite.Geometries.GeometryFactory();
            var l = gf.CreateLineString(
                new []
                    {   new GeoAPI.Geometries.Coordinate(0, 0), 
                        new GeoAPI.Geometries.Coordinate(300, 100),});

            System.Console.WriteLine(SplitLineString(l, 20d));
        }
    }
}