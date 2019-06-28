using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnitTests.Rendering.Symbolizer
{
    public class RasterPointSymbolizerScaleRotnTest
    {

        [Test]
        public void TestRasterPointSymbolizerScaleRotn()
        {
            //plugin Webmercator
            var pcs = ProjNet.CoordinateSystems.ProjectedCoordinateSystem.WebMercator;
            SharpMap.Session.Instance.CoordinateSystemRepository.AddCoordinateSystem(3857, pcs);

            var m = CreateMap();
            m.ZoomToExtents();
            var img = m.GetMap();
            img.Save(System.IO.Path.Combine(UnitTestsFixture.GetImageDirectory(this),"RasterPointSymbolizer.png"), System.Drawing.Imaging.ImageFormat.Png);
        }

        private SharpMap.Map CreateMap()
        {
            var m = new SharpMap.Map(new System.Drawing.Size(1440, 1080)) { BackColor = System.Drawing.Color.LightSkyBlue };
            m.SRID = 3857;

            var originX = 100.0;
            var originY = 7.0;
            var gap = 0.002;

            //create 4 layers varying scale only
            m.Layers.Add(CreateRpsLayer(originX + 0 * gap, originY + 2 * gap, 1, 0));
            m.Layers.Add(CreateRpsLayer(originX + 1 * gap, originY + 2 * gap, 0.5f, 0));
            m.Layers.Add(CreateRpsLayer(originX + 2 * gap, originY + 2 * gap, 0.25f, 0));
            m.Layers.Add(CreateRpsLayer(originX + 3 * gap, originY + 2 * gap, 2, 0));

            //create 4 layers varying rotn  only
            m.Layers.Add(CreateRpsLayer(originX + 0 * gap, originY + 1 * gap, 0, 45));
            m.Layers.Add(CreateRpsLayer(originX + 1 * gap, originY + 1 * gap, 0, 135));
            m.Layers.Add(CreateRpsLayer(originX + 2 * gap, originY + 1 * gap, 0, 225));
            m.Layers.Add(CreateRpsLayer(originX + 3 * gap, originY + 1 * gap, 0, 315));

            //create 4 layers varying scale + rotn
            m.Layers.Add(CreateRpsLayer(originX + 0 * gap, originY + 0 * gap, 1, 45));
            m.Layers.Add(CreateRpsLayer(originX + 1 * gap, originY + 0 * gap, 0.5f, 135));
            m.Layers.Add(CreateRpsLayer(originX + 2 * gap, originY + 0 * gap, 0.25f, 225));
            m.Layers.Add(CreateRpsLayer(originX + 3 * gap, originY + 0 * gap, 2, 315));

            //create a pseudo-graticule for visual reference
            LineString[] graticule = new LineString[7];
            graticule[0] = new LineString(new Coordinate[] { new Coordinate(originX - gap, originY + 0 * gap), new Coordinate(originX + 4 * gap, originY + 0 * gap) });
            graticule[1] = new LineString(new Coordinate[] { new Coordinate(originX - gap, originY + 1 * gap), new Coordinate(originX + 4 * gap, originY + 1 * gap) });
            graticule[2] = new LineString(new Coordinate[] { new Coordinate(originX - gap, originY + 2 * gap), new Coordinate(originX + 4 * gap, originY + 2 * gap) });

            graticule[3] = new LineString(new Coordinate[] { new Coordinate(originX + 0 * gap, originY - gap), new Coordinate(originX + 0 * gap, originY + 3 * gap) });
            graticule[4] = new LineString(new Coordinate[] { new Coordinate(originX + 1 * gap, originY - gap), new Coordinate(originX + 1 * gap, originY + 3 * gap) });
            graticule[5] = new LineString(new Coordinate[] { new Coordinate(originX + 2 * gap, originY - gap), new Coordinate(originX + 2 * gap, originY + 3 * gap) });
            graticule[6] = new LineString(new Coordinate[] { new Coordinate(originX + 3 * gap, originY - gap), new Coordinate(originX + 3 * gap, originY + 3 * gap) });

            var pr = new SharpMap.Data.Providers.GeometryFeatureProvider(graticule);
            pr.SRID = 4326;
            var vl = new SharpMap.Layers.VectorLayer("Graticule", pr);
            vl.TargetSRID = 3857;
            m.Layers.Add(vl);


            return m;
        }

        private SharpMap.Layers.VectorLayer CreateRpsLayer(double x, double y, float scale, float rot)
        {

            NetTopologySuite.Geometries.Point[] pts = new NetTopologySuite.Geometries.Point[1];
            pts[0] = new NetTopologySuite.Geometries.Point(x, y);
            var pr = new SharpMap.Data.Providers.GeometryFeatureProvider(pts);
            pr.SRID = 4326;
            var vl = new SharpMap.Layers.VectorLayer(string.Format("{0} {1} {2} {3}", x, y, scale, rot), pr);
            vl.TargetSRID = 3857;

            var rps = new SharpMap.Rendering.Symbolizer.RasterPointSymbolizer();
            rps.Scale = scale;
            rps.Rotation = rot;
            //rps.Symbol = GetRasterSymbol();
            rps.Symbol = GetRasterSymbol();
            vl.Style.PointSymbolizer = rps;

            return vl;

        }

        private System.Drawing.Image GetRasterSymbol()
        {
            var str = "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAAOwQAADsEBuJFr7QAAABl0RVh0U29mdHdhcmUAcGFpbnQubmV0IDQuMC4yMfEgaZUAAAE8SURBVHhe7dpRDoIwEIThHpWbcHRkW2osJQouswTyTzLFWG3d79k0pTRd0bSqvXdF7fZpji1hzQOvsrzXfE7eKwC2hq8JR4gG+DZ8TShCJMCe4WvCEKIAjgxfE4IQAWCDjOM4vzwW+44cQQ3w7/A1cgQAhADe4WukCACIAM4avkaGEAkwDMOhfuYxAHtza4Ct4S0eAIsEAQAAytJtOLr8yC5eAAsAAOSzm/tcBQCAsnQbjgIAgD36AAAAAMur33kswJFu5dYAZwQAAPLZzX2uAgBAWboNRwEAwB6aAABAPru5z1UAAChLt+EoAADYQxMAAMhnN/e5CgAAZek2HAUAAHtoAgAA+ezmPlcBAKAs3YajAABgD00AACCf3dznKgAigK0/S3tzmz9LW89GkAxvfQMIaj/4zM6LoGl6AWGcInMnlc2ZAAAAAElFTkSuQmCC";
            byte[] imageBytes = Convert.FromBase64String(str);
            System.IO.MemoryStream ms = new System.IO.MemoryStream(imageBytes, 0, imageBytes.Length);
            ms.Write(imageBytes, 0, imageBytes.Length);
            System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);
            return image;
        }
    }
}
