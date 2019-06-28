using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap;
using SharpMap.Data;
using SharpMap.Data.Providers;
using SharpMap.Layers;
using System.Data;
using System.Linq;

namespace UnitTests.Layers
{
    class CoordinateTransformationTest
    {

        //private string _path = "C:\\temp\\";
        private string _baseFileName = "CoordTransTest";
        private double _gcsTolerance = 0.00000002; // 2mm (8dp dec degree = approx 1mm)
        private double _pcsTolerance = 0.002; // 2mm

        [Test]
        public void CoordinateTransformation_ChangingTargetSrid_PR101()
        {
            var m = InitialiseMapAndLayers();

            // force calc/cache of extents
            m.ZoomToExtents();

            TestNoCoordTrans(m);

            // visual proof - zoom to extents of Wgs84 and Ind75. Diagonal cross will not intersect North-South line (ie incorrect datum trans)
            var box = m.Layers[0].Envelope;
            box.ExpandToInclude(m.Layers[1].Envelope);
            m.ZoomToBox(box);

            using (var img = m.GetMap())
                img.Save(System.IO.Path.Combine(UnitTestsFixture.GetImageDirectory(this), _baseFileName + "NoTargetSrid.bmp"));

            // Test setting TargetSRID (should cause CoordinateTransformation to be updated) and generate maps with symmetrical 8-pointed star
            TestSrid(m, 4326, _gcsTolerance);
            TestSrid(m, 4240, _gcsTolerance);
            TestSrid(m, 24047, _pcsTolerance);
            TestSrid(m, 3857, _pcsTolerance);
        }

        [Test]
        public void CoordinateTransformation_ChangingCoordTrans_PR101()
        {
            var m = InitialiseMapAndLayers();

            // force calc/cache of extents
            m.ZoomToExtents();

            TestNoCoordTrans(m);

            // Test setting CoordinateTransformations (should cause TargetSRID to be updated) and generate maps with symmetrical 8-pointed star
            TestTrans(m, 4326, _gcsTolerance);
            TestTrans(m, 4240, _gcsTolerance);
            TestTrans(m, 24047, _pcsTolerance);
            TestTrans(m, 3857, _pcsTolerance);
        }

        private Map InitialiseMapAndLayers()
        {
            // plug in Web Mercator
            var pcs = ProjNet.CoordinateSystems.ProjectedCoordinateSystem.WebMercator;
            SharpMap.Session.Instance.CoordinateSystemRepository.AddCoordinateSystem(3857, pcs);

            Map m = new SharpMap.Map(new System.Drawing.Size(1440, 1080)) { BackColor = System.Drawing.Color.LightSkyBlue };
            m.SRID = 3857;

            // add 3 layers: WGS84 with 2 diagonal lines, Ind75 with North-South line, and Ind75Utm47N with East-West Line
            // Mid-point of all lines is the same point (to within a few mm), creating a symetrical 8-pointed star when correctly transformed.
            // Datum Shift from Wgs84 to Ind75 in study area is in vicinity of 350-400m, so any GCS mis-match is immediately apparent. 
            var vlWgs84 = CreateLayerWgs84();
            var envWgs84 = vlWgs84.DataSource.GetExtents();

            var vlInd75 = CreateLayerInd75();
            var envInd75 = vlInd75.DataSource.GetExtents();

            var vlInd75Utm47N = CreateLayerInd75Utm47N();
            var envInd75Utm47N = vlInd75Utm47N.DataSource.GetExtents();

            m.Layers.Add(vlWgs84);
            m.Layers.Add(vlInd75);
            m.Layers.Add(vlInd75Utm47N);

            return m;
        }

        private void TestNoCoordTrans(Map m)
        {
            for (int i = 0; i < m.Layers.Count(); i++)
            {
                var vl = (VectorLayer)m.Layers[i];
                Assert.IsNull(vl.CoordinateTransformation);
                Assert.IsNull(vl.ReverseCoordinateTransformation);
            }
        }

        private void TestSrid(Map m, int targetSrid, double tolerance)
        {
            // set Map SRID and TartgetSRID
            SetTargetSridAndZoomExtents(m, targetSrid);
            // check CoordTrans Source/Target Ids make sense
            ValidateCoordTransDef(m);
            // centroids of Map Extents and all layers envelopes should be the same
            ValidateCentroids(m, tolerance);
            // and appropriate env dimensions should also compare... need to increase tolerances here, remembering looking for gross errors only
            if (targetSrid == 24047 || targetSrid == 3857)
                // projected coord systems - allow for distortions
                ValidateEnvSizes(m, 2); // 2 metres
            else
                ValidateEnvSizes(m, tolerance * 5);

            // visual check: symmetrical 8-pointed star
            using (var img = m.GetMap())
                img.Save(System.IO.Path.Combine(UnitTestsFixture.GetImageDirectory(this), _baseFileName + "_SRID_" + targetSrid + ".bmp"));
        }

        private void TestTrans(Map m, int targetSrid, double tolerance)
        {
            // set Map SRID and TartgetSRID
            SetCoordTransAndZoomExtents(m, targetSrid);
            // check CoordTrans Source/Target Ids make sense
            ValidateCoordTransDef(m);
            // centroids of Map Extents and all layers envelopes should be the same
            ValidateCentroids(m, tolerance);
            // and appropriate env dimensions should also compare... need to increase tolerances here, remembering looking for gross errors only
            if (targetSrid == 24047 || targetSrid == 3857)
                // projected coord systems - allow for distortions
                ValidateEnvSizes(m, 2); // 2 metres
            else
                ValidateEnvSizes(m, tolerance * 5);

            // visual check: symmetrical 8-pointed star
            using (var img = m.GetMap())
                img.Save(System.IO.Path.Combine(UnitTestsFixture.GetImageDirectory(this), _baseFileName + "_Trans_" + targetSrid + ".bmp"));
        }

        private void SetTargetSridAndZoomExtents(Map m, int targetSrid)
        {
            m.SRID = targetSrid;
            for (int i = 0; i < m.Layers.Count(); i++)
            {
                var srid = ((VectorLayer)m.Layers[i]).SRID;
                ((VectorLayer)m.Layers[i]).SRID = srid;
                ((VectorLayer)m.Layers[i]).TargetSRID = targetSrid;
            }

            m.ZoomToExtents();
        }

        private void SetCoordTransAndZoomExtents(Map m, int targetSrid)
        {
            m.SRID = targetSrid;

            var css = Session.Instance.CoordinateSystemServices;
            var ctf = new ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory();

            for (int i = 0; i < m.Layers.Count(); i++)
            {
                var vl = (VectorLayer)m.Layers[i];
                if (vl.SRID == targetSrid)
                    vl.CoordinateTransformation = null;
                else
                    vl.CoordinateTransformation = ctf.CreateFromCoordinateSystems(
                            css.GetCoordinateSystem(vl.SRID),
                            css.GetCoordinateSystem(targetSrid));
            }

            m.ZoomToExtents();
        }
        private void ValidateCoordTransDef(Map m)
        {
            for (int i = 0; i < m.Layers.Count(); i++)
            {
                var vl = (VectorLayer)m.Layers[i];
                if (m.SRID == vl.SRID)
                {
                    Assert.AreEqual(m.SRID, vl.SRID);
                    Assert.AreEqual(vl.SRID, vl.TargetSRID);
                    Assert.IsNull(vl.CoordinateTransformation);
                    Assert.IsNull(vl.ReverseCoordinateTransformation);
                }
                else
                {
                    Assert.AreEqual(m.SRID, vl.TargetSRID);
                    Assert.IsNotNull(vl.CoordinateTransformation);
                    Assert.AreEqual(vl.SRID, vl.CoordinateTransformation.SourceCS.AuthorityCode);
                    Assert.AreEqual(vl.TargetSRID, vl.CoordinateTransformation.TargetCS.AuthorityCode);
                }
            }
        }

        private void ValidateCentroids(Map m, double tolerance)
        {
            var envMap = m.GetExtents();
            for (int i = 0; i < m.Layers.Count(); i++)
            {
                var vl = (VectorLayer)m.Layers[i];
                Assert.True(envMap.Centre.Equals2D(vl.Envelope.Centre, tolerance));
            }
        }
        private void ValidateEnvSizes(Map m, double tolerance)
        {
            var envMap = m.GetExtents();
            for (int i = 0; i < m.Layers.Count(); i++)
            {
                var vl = (VectorLayer)m.Layers[i];
                switch (vl.LayerName)
                {
                    case "Wgs84":
                        // map env Width and Height = lyr env Width and Height
                        Assert.AreEqual(envMap.Width, vl.Envelope.Width, tolerance);
                        Assert.AreEqual(envMap.Height, vl.Envelope.Height, tolerance);
                        break;

                    case "Ind75":
                        // map env Height = lyr env Height
                        Assert.AreEqual(envMap.Height, vl.Envelope.Height, tolerance);
                        break;

                    case "Ind75Utm47N":
                        // map env Width = lyr env Width
                        Assert.AreEqual(envMap.Width, vl.Envelope.Width, tolerance);
                        break;
                }
            }
        }

        private VectorLayer CreateLayerWgs84()
        {
            GeometryFeatureProvider gfp = new GeometryFeatureProvider(CreateFeatureDataTable());
            gfp.SRID = 4326;

            FeatureDataRow fdr = gfp.Features.NewRow();
            var coordsDiag1 = new[] { new Coordinate(100.498, 7.498), new Coordinate(100.502, 7.502) };
            fdr.Geometry = new LineString(coordsDiag1);
            gfp.Features.AddRow(fdr);

            fdr = gfp.Features.NewRow();
            var coordsDiag2 = new[] { new Coordinate(100.498, 7.502), new Coordinate(100.502, 7.498) };
            fdr.Geometry = new LineString(coordsDiag2);
            gfp.Features.AddRow(fdr);

            gfp.Features.AcceptChanges();

            VectorLayer vl = new VectorLayer("Wgs84");
            vl.DataSource = gfp;
            return vl;

        }

        private VectorLayer CreateLayerInd75()
        {
            GeometryFeatureProvider gfp = new GeometryFeatureProvider(CreateFeatureDataTable());
            gfp.SRID = 4240;

            FeatureDataRow fdr = gfp.Features.NewRow();
            var coordsNS = new[] { new Coordinate(100.503215154, 7.49987995484), new Coordinate(100.503215125, 7.4958796771) };
            fdr.Geometry = new LineString(coordsNS);
            gfp.Features.AddRow(fdr);

            gfp.Features.AcceptChanges();

            VectorLayer vl = new VectorLayer("Ind75");
            vl.DataSource = gfp;
            return vl;
        }

        private VectorLayer CreateLayerInd75Utm47N()
        {
            GeometryFeatureProvider gfp = new GeometryFeatureProvider(CreateFeatureDataTable());
            gfp.SRID = 24047;

            FeatureDataRow fdr = gfp.Features.NewRow();
            var coordsWE = new[] { new Coordinate(665624.762, 829006.460), new Coordinate(666066.223, 829007.968) };
            fdr.Geometry = new LineString(coordsWE);
            gfp.Features.AddRow(fdr);

            gfp.Features.AcceptChanges();

            VectorLayer vl = new VectorLayer("Ind75Utm47N");
            vl.DataSource = gfp;
            return vl;
        }

        private FeatureDataTable CreateFeatureDataTable()
        {
            FeatureDataTable fdt = new FeatureDataTable();

            fdt.Columns.Add("Oid", typeof(uint));
            UniqueConstraint con = new UniqueConstraint(fdt.Columns[0]);
            con.Columns[0].AutoIncrement = true;
            fdt.Constraints.Add(con);
            fdt.PrimaryKey = new DataColumn[1] { fdt.Columns[0] };

            return fdt;

        }

    }
}
