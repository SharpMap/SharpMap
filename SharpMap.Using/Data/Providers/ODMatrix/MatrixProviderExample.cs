using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GeoAPI.Geometries;
using SharpMap.Layers;
using SharpMap.Styles;
using GeoPoint = GeoAPI.Geometries.IPoint;

namespace SharpMap.Data.Providers.ODMatrix
{
    public partial class MatrixProviderExample : Form
    {
        public MatrixProviderExample()
        {
            InitializeComponent();
            CreateMatrix();
            //CreateMatrixMdb();

            mapBox1.Map = CreateMap();
        }

        private ODMatrix _matrix;

        private void CreateMatrix()
        {
            var f = new NetTopologySuite.Geometries.GeometryFactory();
            _matrix = new ODMatrix(new[]
                                       {
                                           new KeyValuePair<ushort, GeoPoint>(0, f.CreatePoint(new Coordinate(-10, -10))),
                                           new KeyValuePair<ushort, GeoPoint>(1, f.CreatePoint(new Coordinate(-10, 10))),
                                           new KeyValuePair<ushort, GeoPoint>(2, f.CreatePoint(new Coordinate(10, 10))),
                                           new KeyValuePair<ushort, GeoPoint>(3, f.CreatePoint(new Coordinate(10, -10))),
                                           new KeyValuePair<ushort, GeoPoint>(4, f.CreatePoint(new Coordinate(0, 0))),
                                       });

            _matrix[0, 1] = 10;
            _matrix[0, 2] = 5;
            _matrix[0, 3] = 10;
            _matrix[1, (ushort)0] = 10;
            _matrix[1, 2] = 10;
            _matrix[1, 3] = 5;
            _matrix[2, (ushort)0] = 5;
            _matrix[2, 1] = 10;
            _matrix[2, 3] = 10;
            _matrix[3, (ushort)0] = 10;
            _matrix[3, 1] = 5;
            _matrix[3, 2] = 10;
            _matrix[4, 4] = 15;
        }

        private void CreateMatrixMdb()
        {
            using (
                var connPoints =
                    new Npgsql.NpgsqlConnection(
                        "server=127.0.0.1;port=5432;database=ivv-projekte;uid=postgres;password=1.Kennwort"))
            {
                connPoints.Open();
                using (
                    var pointsReader =
                        new Npgsql.NpgsqlCommand(
                            "WITH t as (SELECT zellnr, st_pointonsurface(wkb_geometry) as g FROM \"DFN2950\".\"fuerimport\") " +
                            "SELECT zellnr, st_X(g), st_Y(g) FROM t ORDER BY zellnr;",
                            connPoints).ExecuteReader())
                {
                    using (
                        var connMatrix =
                            new Npgsql.NpgsqlConnection(
                                "server=127.0.0.1;port=5432;database=ivv-projekte;uid=postgres;password=1.Kennwort"))
                    {
                        connMatrix.Open();
                        using (
                            var matrixReader =
                                new Npgsql.NpgsqlCommand(
                                    "SELECT origin, destin, val FROM \"DFN2950\".\"iv-kfz\" " +
                                    "WHERE val > 10 " +
                                    "ORDER BY origin, destin;",
                                    connMatrix).ExecuteReader())
                            _matrix = new ODMatrix(pointsReader, matrixReader);
                    }
                }
            }
        }

        private Map CreateMap()
        {
            var map = new Map(ClientSize);
            
            //var p1 = new MatrixRelationProvider(_matrix) { ScaleFactor = 0.1d, ScaleMethod = ScaleMethod.Linear };
            //map.Layers.Add(new VectorLayer(p1.ProviderName, p1)
            //{
            //    Style =
            //        new VectorStyle { Outline = new Pen(Brushes.Black, 1), Fill = new SolidBrush(Color.DarkSalmon), EnableOutline = true }
            //});

            var p2 = new MatrixRelationProvider(_matrix) { ScaleFactor = 0.1d, ScaleMethod = ScaleMethod.Linear, RestrictId = 1 };
            map.Layers.Add(new VectorLayer(p2.ProviderName, p2)
            {
                Style =
                    new VectorStyle { Outline = new Pen(Brushes.Black, 1), Fill = new SolidBrush(Color.BlueViolet), EnableOutline = true }
            });

            var p3 = new MatrixODSumProvider(_matrix, ODMatrixVector.Both) { ScaleFactor = 0.1d, ScaleMethod = ScaleMethod.Linear };
            map.Layers.Add(new VectorLayer(p3.ProviderName, p3)
                               {
                                   Style =
                                       new VectorStyle { Outline = new Pen(Brushes.Black, 1), Fill = new SolidBrush(Color.DarkSeaGreen), EnableOutline = true }
                               });

            var ll = new LabelLayer(string.Format("Label {0}", p3.ProviderName));
            ll.DataSource = p3;
            ll.LabelStringDelegate = d => string.Format("Oid:{0}\nVal:{1:n}", d[0], d[1]);
            ll.Style.Halo = Pens.AliceBlue;
            ll.Style.HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center;
            ll.PriorityColumn = "Value";
            ll.Style.CollisionBuffer = new SizeF(2, 2);
            ll.Style.CollisionDetection = true;
            map.Layers.Add(ll);

            //var extent = map.GetExtents();
            var center = _matrix[1];

            var box = center.EnvelopeInternal;
            box.ExpandBy(25);
            map.ZoomToBox(box);
            return map;
        }
    }
}