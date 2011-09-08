

namespace ExampleCodeSnippets
{
    [NUnit.Framework.TestFixture]
    public class LengthIndexedLineSample
    {
public SharpMap.Geometries.MultiLineString SplitLineString(
    SharpMap.Geometries.LineString lineString, 
    System.Double length)
{
    if (lineString == null || lineString.IsEmpty())
        throw new System.ArgumentException("Linestring is null or Empty", "lineString");

    var gf = new NetTopologySuite.Geometries.GeometryFactory();
    var ntsLine = (NetTopologySuite.Geometries.LineString)
                    SharpMap.Converters.NTS.GeometryConverter.ToNTSGeometry(lineString, gf);

    var ret = new SharpMap.Geometries.MultiLineString();
    var lil = new NetTopologySuite.LinearReferencing.LengthIndexedLine(ntsLine);

    double currentLength = 0d;
    while (currentLength  < ntsLine.Length)
    {
        var tmpLine = (NetTopologySuite.Geometries.LineString)
            lil.ExtractLine(currentLength, currentLength + length);
        ret.LineStrings.Add((SharpMap.Geometries.LineString)
            SharpMap.Converters.NTS.GeometryConverter.ToSharpMapGeometry(tmpLine));
        currentLength += length;
    }
    return ret;
}

        [NUnit.Framework.Test]
        public void TestLengthIndexedLine()
        {
            var l = new SharpMap.Geometries.LineString(
                new SharpMap.Geometries.Point[]
                    {   new SharpMap.Geometries.Point(0, 0), 
                        new SharpMap.Geometries.Point(300, 100),});

            System.Console.WriteLine(SplitLineString(l, 20d));
        }
    }
}