using BruTile.Predefined;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using SharpMap;
using SharpMap.Data;
using SharpMap.Data.Providers;
using SharpMap.Layers;
using SharpMap.Rendering.Decoration;
using SharpMap.Rendering.Symbolizer;
using SharpMap.Styles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Windows.Forms;
using WinFormSamples.Properties;
using Point = NetTopologySuite.Geometries.Point;

namespace WinFormSamples
{
    public partial class FormLayerListImageGenerator : Form
    {
        private MovingObjects _fastBoats;
        private MovingObjects _mediumBoats;
        private MovingObjects _slowBoats;
        private static Image _boat;

        private Layer _contextLayer;
        
        // Context Menu actions
        private enum enumMenuItem
        {
            Refresh,             // MapBox.Refresh
            IncrementLabelSize,  // Label Layers
            DecrementLabelSize,  // Label Layers
            StartMoving,         // Variable Layer moving object   
            StopMoving,          // Variable Layer moving object
            RegularSymbolizer,   // Variable Layer and Map Point layer
            ThematicSymbolizer,  // Variable Layer and Map Point layer
            IncreaseSymbolSize,  // Basic Vector Style Point Size
            DecreaseSymbolSize,  // Basic Vector Style Point Size
            SymbolOffsetTL,      // Basic Vector Style Point Size
            SymbolOffsetTR,      // Basic Vector Style Point Size  
            SymbolOffsetNone,    // Basic Vector Style Point Size
            SymbolOffsetBL,      // Basic Vector Style Point Size
            SymbolOffsetBR,      // Basic Vector Style Point Size
            AlignHz,             // Regenerate Map Point layer data  
            AlignVt,             // Regenerate Map Point layer data
            AlignDiagonal ,      // Regenerate Map Point layer data
            IncrementLineWidth,  // Rectangle layers
            DecrementLineWidth   // Rectangle layers
        }
        
        public FormLayerListImageGenerator()
        {
            InitializeComponent();
        }

        private void FormLayerListImageGenerator_Load(object sender, System.EventArgs e)
        {
            this.SizeChanged += Form_SizeChanged;
            
            _boat = Resources.vessel_01;
            
            InitMap();
            InitBackground();
            InitLayers();
            InitVariableLayers();
            InitTreeView();
            InitRotations();

            using (var renderer = SharpMap.Forms.MapBox.MapImageGeneratorFunction(new SharpMap.Forms.MapBox(), null))
            {
                if (renderer is SharpMap.Forms.ImageGenerator.LegacyMapBoxImageGenerator)
                    this.txtImgGeneration.Text = (this.txtImgGeneration.Text + "\n.    LegacyMapImageRenderer");
                else
                    this.txtImgGeneration.Text = (this.txtImgGeneration.Text + "\n.    LayerListImageGenerator");
            };

            this.mb.Refresh();

            //_slowBoats?.Start();
            _mediumBoats?.Start();
            //_fastBoats?.Start();
        }



        private void FormLayerListImageGenerator_Closing(object sender, EventArgs e)
        {
            this.SizeChanged -= Form_SizeChanged;
            _fastBoats?.Dispose();
            _mediumBoats?.Dispose();
            _slowBoats?.Dispose();
        }

        private void Form_SizeChanged(object sender, EventArgs e)
        {
            this.mb.Refresh();
        }

        private void tv_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag != null)
            {
                var lyr = (Layer)e.Node.Tag;
                lyr.Enabled = e.Node.Checked;
                this.mb.Refresh();
            }
        }

        #region  Context Menu Stuff
        private void tv_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;

            var cm = new ContextMenu();
            var vlyr = e.Node.Tag as VectorLayer;
            _contextLayer = e.Node.Tag as Layer;

            if (vlyr != null && (e.Node.FullPath.StartsWith("Variable Layers", StringComparison.OrdinalIgnoreCase)))
            {
                if (cm.MenuItems.Count > 0) cm.MenuItems.Add(new MenuItem("-"));

                if (_contextLayer.LayerName.StartsWith("Fast"))
                    cm.MenuItems.Add(_fastBoats.IsRunning ?
                        CreateMenuItem(enumMenuItem.StopMoving, "Stop") :
                        CreateMenuItem(enumMenuItem.StartMoving, "Start"));
                else if (_contextLayer.LayerName.StartsWith("Slow"))
                    cm.MenuItems.Add(_slowBoats.IsRunning ?
                        CreateMenuItem(enumMenuItem.StopMoving, "Stop") :
                        CreateMenuItem(enumMenuItem.StartMoving, "Start"));
                else
                    cm.MenuItems.Add(_mediumBoats.IsRunning ?
                        CreateMenuItem(enumMenuItem.StopMoving, "Stop") :
                        CreateMenuItem(enumMenuItem.StartMoving, "Start"));

                //cm.MenuItems.Add(CreateMenuItem(enumMenuItem.StartMoving, "Start"));
                //cm.MenuItems.Add(CreateMenuItem(enumMenuItem.StopMoving, "Stop"));
                cm.MenuItems.Add(new MenuItem("-"));

                if (vlyr.Theme == null)
                {
                    cm.MenuItems.Add(CreateMenuItem(enumMenuItem.ThematicSymbolizer, "Thematic Symbolizer"));
                    cm.MenuItems.Add(new MenuItem("-"));
                    cm.MenuItems.Add(CreateMenuItem(enumMenuItem.IncreaseSymbolSize, "Increment symbol size"));
                    cm.MenuItems.Add(CreateMenuItem(enumMenuItem.DecreaseSymbolSize, "Decrement symbol size"));
                }
                else
                    cm.MenuItems.Add(CreateMenuItem(enumMenuItem.RegularSymbolizer, "Basic Symbolizer"));
            }
            else if (vlyr != null && (e.Node.FullPath.StartsWith("Map Layers", StringComparison.OrdinalIgnoreCase)))
            {
                if (vlyr.LayerName.StartsWith("Point", StringComparison.OrdinalIgnoreCase) )
                {
                    if (vlyr.Theme == null)
                    {
                        // default point style
                        if (cm.MenuItems.Count > 0) cm.MenuItems.Add(new MenuItem("-"));
                        cm.MenuItems.Add(CreateMenuItem(enumMenuItem.IncreaseSymbolSize, "Increment symbol size"));
                        cm.MenuItems.Add(CreateMenuItem(enumMenuItem.DecreaseSymbolSize, "Decrement symbol size"));
                        cm.MenuItems.Add(new MenuItem("-"));
                        cm.MenuItems.Add(CreateMenuItem(enumMenuItem.SymbolOffsetNone, "Remove symbol offset"));
                        cm.MenuItems.Add(CreateMenuItem(enumMenuItem.SymbolOffsetTL, "Offset step upper left"));
                        cm.MenuItems.Add(CreateMenuItem(enumMenuItem.SymbolOffsetTR, "Offset step upper right"));
                        cm.MenuItems.Add(CreateMenuItem(enumMenuItem.SymbolOffsetBL, "Offset Step lower left"));
                        cm.MenuItems.Add(CreateMenuItem(enumMenuItem.SymbolOffsetBR, "Offset step lower right"));
                    }
                    if (cm.MenuItems.Count > 0) cm.MenuItems.Add(new MenuItem("-"));
                    cm.MenuItems.Add(CreateMenuItem(enumMenuItem.AlignHz, "Align Pts Horizontal"));
                    cm.MenuItems.Add(CreateMenuItem(enumMenuItem.AlignVt, "Align Pts Vertical"));
                    cm.MenuItems.Add(CreateMenuItem(enumMenuItem.AlignDiagonal, "Align Pts Diagonal"));
                }

                if (vlyr.LayerName.Contains("Rect"))
                {
                    if (cm.MenuItems.Count > 0) cm.MenuItems.Add(new MenuItem("-"));
                    cm.MenuItems.Add(CreateMenuItem(enumMenuItem.IncrementLineWidth, "Increment line width"));
                    cm.MenuItems.Add(CreateMenuItem(enumMenuItem.DecrementLineWidth, "Decrement line width"));
                }
            }

            if (e.Node.Tag is LabelLayer)
            {
                if (cm.MenuItems.Count > 0) cm.MenuItems.Add(new MenuItem("-"));
                cm.MenuItems.Add(CreateMenuItem(enumMenuItem.IncrementLabelSize, "Increment Label Size"));
                cm.MenuItems.Add(CreateMenuItem(enumMenuItem.DecrementLabelSize, "Decrement Label Size"));
            }

            if (cm.MenuItems.Count > 0) cm.MenuItems.Add(new MenuItem("-"));
            cm.MenuItems.Add(CreateMenuItem(enumMenuItem.Refresh, "Refresh [clear cache]"));

            cm.Show(tv, new System.Drawing.Point(e.X, e.Y));
        }

        private MenuItem CreateMenuItem(enumMenuItem eMenuItem, string text)
        {
            var mi = new MenuItem(text)
            {
                Tag = eMenuItem,
            };
            mi.Click += MenuItemClick;
            return mi;
        }

        private void MenuItemClick (Object sender, EventArgs e)
        {
            var mi = sender as MenuItem;
            if (mi == null) return;

            var vectorLyr = _contextLayer as VectorLayer;
            var lblLyr = _contextLayer as LabelLayer;

            switch ((enumMenuItem)mi.Tag)
            {
                //case enumMenuItem.Refresh:
                //    this.mb.Refresh();
                //    break;
                case enumMenuItem.StartMoving:
                    if (_contextLayer.LayerName.StartsWith("Fast"))
                        _fastBoats?.Start();
                     else if (_contextLayer.LayerName.StartsWith("Slow"))
                        _slowBoats?.Start();
                    else
                        _mediumBoats?.Start();
                    break;

                case enumMenuItem.StopMoving:
                    if (_contextLayer.LayerName.StartsWith("Fast"))
                        _fastBoats?.Stop();
                    else if (_contextLayer.LayerName.StartsWith("Slow"))
                        _slowBoats?.Stop();
                    else
                        _mediumBoats?.Stop();
                    break;

                case enumMenuItem.IncrementLabelSize:
                    lblLyr.Style.Font = new Font(lblLyr.Style.Font.FontFamily, lblLyr.Style.Font.Size + 2);
                    break;
                case enumMenuItem.DecrementLabelSize:
                    lblLyr.Style.Font = new Font(lblLyr.Style.Font.FontFamily, lblLyr.Style.Font.Size - 2);
                    break;
                case enumMenuItem.RegularSymbolizer:
                    InitRasterPointSymbolizer(vectorLyr, 0);
                    break;
                case enumMenuItem.ThematicSymbolizer:
                    InitRasterPointSymbolizer(vectorLyr, 1);
                    break;
                case enumMenuItem.IncreaseSymbolSize:
                    vectorLyr.Style.PointSize += 2;
                    break;
                case enumMenuItem.DecreaseSymbolSize:
                    vectorLyr.Style.PointSize-= 2;
                    break;
                case enumMenuItem.SymbolOffsetTL:
                    vectorLyr.Style.SymbolOffset = new PointF(vectorLyr.Style.SymbolOffset.X - 10f, vectorLyr.Style.SymbolOffset.Y - 10f);
                    break;
                case enumMenuItem.SymbolOffsetTR:
                    vectorLyr.Style.SymbolOffset = new PointF(vectorLyr.Style.SymbolOffset.X + 10f, vectorLyr.Style.SymbolOffset.Y - 10f);
                    break;
                case enumMenuItem.SymbolOffsetNone:
                    vectorLyr.Style.SymbolOffset = new PointF(0, 0);
                    break;
                case enumMenuItem.SymbolOffsetBL:
                    vectorLyr.Style.SymbolOffset = new PointF(vectorLyr.Style.SymbolOffset.X - 10f, vectorLyr.Style.SymbolOffset.Y + 10f);
                    break;
                case enumMenuItem.SymbolOffsetBR:
                    vectorLyr.Style.SymbolOffset = new PointF(vectorLyr.Style.SymbolOffset.X + 10f, vectorLyr.Style.SymbolOffset.Y + 10f);
                    break;
                case enumMenuItem.AlignHz:
                    if (vectorLyr.Theme == null)
                        PopulateGeomFeatureLayer(vectorLyr, 0);
                    else
                        PopulateCharacterPointSymbolizerLayer((GeometryFeatureProvider)vectorLyr.DataSource, 0);
                    break;
                case enumMenuItem.AlignVt:
                    if (vectorLyr.Theme == null)
                        PopulateGeomFeatureLayer(vectorLyr, 1);
                    else
                        PopulateCharacterPointSymbolizerLayer((GeometryFeatureProvider)vectorLyr.DataSource, 1); 
                    break;
                case enumMenuItem.AlignDiagonal:
                    if (vectorLyr.Theme == null)
                        PopulateGeomFeatureLayer(vectorLyr, 2);
                    else
                        PopulateCharacterPointSymbolizerLayer((GeometryFeatureProvider)vectorLyr.DataSource, 2); 
                    break;
                case enumMenuItem.IncrementLineWidth:
                    vectorLyr.Style.Line.Width += 1;
                    break;
                case enumMenuItem.DecrementLineWidth:
                    vectorLyr.Style.Line.Width -= vectorLyr.Style.Line.Width <= 1 ? 0 : 1;
                    break;

                default:
                    break;
            }

            this.mb.Refresh();

        }
        #endregion

        private void InitTreeView()
        {
            var font = new System.Drawing.Font(tv.Font.FontFamily, tv.Font.Size, System.Drawing.FontStyle.Bold);

            tv.Nodes.Add(new TreeNode("Variable Layers") {NodeFont = font});
            tv.Nodes.Add(new TreeNode("Map Layers") { NodeFont = font });
            tv.Nodes.Add(new TreeNode("Background Layers") { NodeFont = font });

            // Populate Tree View
            TreeViewAddLayerNode(tv.Nodes[0], this.mb.Map.VariableLayers);
            TreeViewAddLayerNode(tv.Nodes[1], this.mb.Map.Layers);
            TreeViewAddLayerNode(tv.Nodes[2], this.mb.Map.BackgroundLayer);

            this.tv.CheckBoxes = true;
            this.tv.ShowRootLines = false;
            this.tv.ExpandAll();
        }

        private void TreeViewAddLayerNode(TreeNode parentNode, LayerCollection collection)
        {
            foreach (var lyr in collection)
                TreeViewAddLayerNode(parentNode, lyr);
        }

        private void TreeViewAddLayerNode(TreeNode parentNode, ILayer layer)
        {
            var node = new TreeNode(layer.LayerName) { Tag = layer, Checked = layer.Enabled };
            parentNode.Nodes.Add(node);

            var lyrGrp = layer as LayerGroup;
            if (lyrGrp != null)
                foreach (var lyr in lyrGrp.Layers)
                    TreeViewAddLayerNode(node, lyr);
        }

        private void InitRotations()
        {
            ddlRotation.Items.Clear();
            ddlRotation.DisplayMember = "Text";
            ddlRotation.ValueMember = "Value";

            ddlRotation.Items.Add(new {Text = "North up", Value = 0f});
            ddlRotation.Items.Add(new {Text = "30°", Value = 30f});
            ddlRotation.Items.Add(new {Text = "60°", Value = 60f});
            ddlRotation.Items.Add(new {Text = "90°", Value = 90f});
            ddlRotation.Items.Add(new {Text = "120°", Value = 120f});
            ddlRotation.Items.Add(new {Text = "150°", Value = 150f});
            ddlRotation.Items.Add(new {Text = "180°", Value = 180f});
            ddlRotation.Items.Add(new {Text = "210°", Value = 210f});
            ddlRotation.Items.Add(new {Text = "240°", Value = 240f});
            ddlRotation.Items.Add(new {Text = "270°", Value = 270f});
            ddlRotation.Items.Add(new {Text = "300°", Value = 300f});
            ddlRotation.Items.Add(new {Text = "330°", Value = 330f});

            ddlRotation.SelectedIndex = 0;
        }

        private void ddlRotation_SelectedIndexChanged(object sender, EventArgs e)
        {
            var deg = (ddlRotation.SelectedItem as dynamic).Value;
            var matrix = new System.Drawing.Drawing2D.Matrix();
            if (deg != 0f)
                matrix.RotateAt((float) deg, new PointF(this.mb.Width / 2f, this.mb.Height / 2f));
            this.mb.Map.MapTransform = matrix;
            this.mb.Refresh();
        }

        private void InitMap()
        {
            this.mb.Map = new SharpMap.Map()
            {
                SRID = 3857,
                BackColor = System.Drawing.Color.AliceBlue,
            };

            //Lisbon...
            var mathTransform = LayerTools.Wgs84toGoogleMercator.MathTransform;
            var geom = GeometryTransform.TransformBox(
                new Envelope(-9.205626, -9.123736, 38.690993, 38.740837),
                mathTransform);
            //geom.ExpandBy(2500);
            this.mb.Map.ZoomToBox(geom);

            this.mb.Map.Decorations.Add(new NorthArrow() {ForeColor = Color.DarkSlateBlue});
        }

        private void InitVariableLayers()
        {
            LayerGroup lyrGrp = null;
            VectorLayer lyr = null;

            // group layer with single target + labels
            lyrGrp = new LayerGroup("Fast Boats Group");
            lyr = CreateGeometryFeatureProviderLayer("Fast Boats", new[] {
                    new System.Data.DataColumn("Name",typeof(string)),
                    new System.Data.DataColumn("Heading",typeof(float)),
                    new System.Data.DataColumn("Scale",typeof(float)),
                    new System.Data.DataColumn("ARGB",typeof(int))
                });
            lyr.Style.PointColor = new SolidBrush(Color.Green);
            _fastBoats = new MovingObjects(50, 50, lyr, this.mb.Map, 0.8f, Color.Green);
            _fastBoats.AddObject("Fast 1", GetRectangleCenter(MapDecorationAnchor.LeftTop));
            InitRasterPointSymbolizer(lyr, 0);
            lyrGrp.Layers.Add(lyr);
            lyrGrp.Layers.Add(CreateLabelLayer(lyr, "Name", false));
            this.mb.Map.VariableLayers.Add(lyrGrp);
            
            // group layer with multiple targets + labels
            lyrGrp = new LayerGroup("Medium Boats Group");
            lyr = CreateGeometryFeatureProviderLayer("Medium Boats", new[] {
                new System.Data.DataColumn("Name",typeof(string)),
                new System.Data.DataColumn("Heading",typeof(float)),
                new System.Data.DataColumn("Scale",typeof(float)),
                new System.Data.DataColumn("ARGB",typeof(int))
            });
            lyr.Style.PointColor = new SolidBrush(Color.Yellow);
            _mediumBoats = new MovingObjects(500, 100, lyr, this.mb.Map, 1, Color.Yellow);
            _mediumBoats.AddObject("Boat 1", GetRectangleCenter(MapDecorationAnchor.RightTop));
            _mediumBoats.AddObject("Boat 2", GetRectangleCenter(MapDecorationAnchor.RightCenter));
            InitRasterPointSymbolizer(lyr, 1);
            lyrGrp.Layers.Add(lyr);
            lyrGrp.Layers.Add(CreateLabelLayer(lyr, "Name", false));
            this.mb.Map.VariableLayers.Add(lyrGrp);
            
            // group layer with multiple targets + labels
            lyrGrp = new LayerGroup("Slow Boats Group");
            lyr = CreateGeometryFeatureProviderLayer("Slow Boats", new[] {
                new System.Data.DataColumn("Name",typeof(string)),
                new System.Data.DataColumn("Heading",typeof(float)),
                new System.Data.DataColumn("Scale",typeof(float)),
                new System.Data.DataColumn("ARGB",typeof(int))
            });
            // raster point symbolizer
            lyr.Style.PointColor = new SolidBrush(Color.Red);
            _slowBoats = new MovingObjects(2000, 100, lyr, this.mb.Map, 1.2f, Color.Red);
            _slowBoats.AddObject("Slow 1", GetRectangleCenter(MapDecorationAnchor.LeftBottom));
            _slowBoats.AddObject("Slow 2", GetRectangleCenter(MapDecorationAnchor.CenterBottom));
            _slowBoats.AddObject("Slow 3", GetRectangleCenter(MapDecorationAnchor.RightBottom));
            InitRasterPointSymbolizer(lyr, 1);
            lyrGrp.Layers.Add(lyr);
            lyrGrp.Layers.Add(CreateLabelLayer(lyr, "Name", true));
            this.mb.Map.VariableLayers.Add(lyrGrp);
        }

        private ILayer CreateLabelLayer(VectorLayer lyr, string column, bool enabled)
        {
            var lblLayer = new LabelLayer( lyr.LayerName + " Labels");
            lblLayer.DataSource = lyr.DataSource;
            lblLayer.LabelColumn = column;
            //lblLayer.Style.BackColor = Brushes.LightPink;
            lblLayer.SRID = lblLayer.SRID;
            lblLayer.Enabled = enabled;
            return lblLayer;
        }

        private void InitLayers()
        {
            LayerGroup lyrGrp = null;
            VectorLayer lyr = null;
            Geometry[] geoms = null;

            // group layer with 2 child layers (Blue Rect, Red Rect)
            lyrGrp = new LayerGroup("Layer Group 1");

            geoms = new Geometry[] { new LineString(GetRectanglePoints(MapDecorationAnchor.LeftTop)) };
            lyrGrp.Layers.Add(CreateGeomLayer("Blue Rectangle", geoms, System.Drawing.Color.DodgerBlue));

            geoms = new Geometry[] { new LineString(GetRectanglePoints(MapDecorationAnchor.CenterTop)) };
            lyrGrp.Layers.Add(CreateGeomLayer("Red Rectangle", geoms, System.Drawing.Color.Red));
            this.mb.Map.Layers.Add(lyrGrp);

            // layer with Green Rect
            geoms = new Geometry[] { new LineString(GetRectanglePoints(MapDecorationAnchor.RightTop)) };
            lyr = CreateGeomLayer("Green Rectangle", geoms, System.Drawing.Color.Green);
            this.mb.Map.Layers.Add(lyr);

            // Point layer with basic Vector Style
            geoms = new Geometry[] {
                GetRectangleCenter(MapDecorationAnchor.LeftTop),
                GetRectangleCenter(MapDecorationAnchor.Center),
                GetRectangleCenter(MapDecorationAnchor.CenterBottom),
            };
            lyr = CreateGeomLayer("Points with Vector Style", geoms, System.Drawing.Color.Transparent);
            //lyr.Style.SymbolOffset =  new System.Drawing.PointF(20,20);
            lyr.Enabled = false;
            this.mb.Map.Layers.Add(lyr);

            // Char Symbol Layer with Thematic rendering
            lyr = CreateGeometryFeatureProviderLayer("Points with thematic CPS", new [] {
                new System.Data.DataColumn("CharIndex",typeof(int)),
                new System.Data.DataColumn("CharSize",typeof(float)),
                new System.Data.DataColumn("OffsetX",typeof(float)),
                new System.Data.DataColumn("OffsetY", typeof(float))
            });
            PopulateCharacterPointSymbolizerLayer((GeometryFeatureProvider)lyr.DataSource, 0);
            lyr.Theme = new SharpMap.Rendering.Thematics.CustomTheme(GetCharacterPointStyle);
            lyr.Enabled = false;
            this.mb.Map.Layers.Add(lyr);
        }

        private void InitBackground()
        {
            var lyr = new TileAsyncLayer(KnownTileSources.Create(KnownTileSource.BingRoads), "Async TileLayer [Bing]");
            lyr.SRID = 3857;
            this.mb.Map.BackgroundLayer.Add(lyr);
        }

        private void InitRasterPointSymbolizer(VectorLayer lyr, int style)
        {
            if (style == 1)
                lyr.Theme = new SharpMap.Rendering.Thematics.CustomTheme(GetRasterPointSymbolizerStyle);
            else
                lyr.Theme = null;
        }

        public static VectorStyle GetRasterPointSymbolizerStyle(FeatureDataRow row)
        {
            // NB - this is for testing only.
            var rps = new RasterPointSymbolizer()
            {
                Symbol = (Image)_boat.Clone(),
                Rotation = (float)row[2],
                RemapColor = Color.White,
                Scale = (float) row[3],
                SymbolColor = Color.FromArgb((int) row[4])
            };
            return new VectorStyle() {PointSymbolizer = rps};
        }

        private void PopulateGeomFeatureLayer(VectorLayer lyr, int direction)
        {
            var geoms = new Geometry[] {
                GetRectangleCenter(MapDecorationAnchor.LeftTop),
                GetRectangleCenter(direction == 0 ? MapDecorationAnchor.CenterTop : direction == 1 ? MapDecorationAnchor.LeftCenter  : MapDecorationAnchor.Center),
                GetRectangleCenter(direction == 0 ? MapDecorationAnchor.RightTop : direction == 1 ? MapDecorationAnchor.LeftBottom  : MapDecorationAnchor.RightBottom)
            };

            var gp = (GeometryProvider)lyr.DataSource;
            gp.Geometries.Clear();
            foreach (var geom in geoms)
                gp.Geometries.Add(geom);
            
        }
        private void PopulateCharacterPointSymbolizerLayer(GeometryFeatureProvider fp, int direction)
        {
            FeatureDataRow fdr = null;

            fp.Features.Clear();

            fdr = fp.Features.NewRow();
            fdr["CharIndex"] = 171;
            fdr["CharSize"] = 15f;
            fdr["OffsetX"] = -10f;
            fdr["OffsetY"] = 5f;
            fdr.Geometry = GetRectangleCenter(direction == 0 ? MapDecorationAnchor.LeftBottom : direction == 1 ? MapDecorationAnchor.RightBottom  : MapDecorationAnchor.LeftBottom);
            fp.Features.AddRow(fdr);

            fdr = fp.Features.NewRow();
            fdr["CharIndex"] = 176;
            fdr["CharSize"] = 20f;
            fdr["OffsetX"] = -10f;
            fdr["OffsetY"] = 5f;
            fdr.Geometry = GetRectangleCenter(direction == 0 ? MapDecorationAnchor.CenterBottom : direction == 1 ? MapDecorationAnchor.RightCenter  : MapDecorationAnchor.Center);
            fp.Features.AddRow(fdr);

            fdr = fp.Features.NewRow();
            fdr["CharIndex"] = 181;
            fdr["CharSize"] = 25f;
            fdr["OffsetX"] = -15f;
            fdr["OffsetY"] = -10f;
            fdr.Geometry = GetRectangleCenter(direction == 0 ? MapDecorationAnchor.RightBottom : MapDecorationAnchor.RightTop);
            fp.Features.AddRow(fdr);

            fp.Features.AcceptChanges();
        }
        private VectorLayer CreateGeometryFeatureProviderLayer(string name, System.Data.DataColumn[] columns)
        {
            var fdt = new FeatureDataTable();
            fdt.Columns.Add("Oid", typeof(uint));
            var con = new System.Data.UniqueConstraint(fdt.Columns[0]);
            con.Columns[0].AutoIncrement = true;
            con.Columns[0].AutoIncrementSeed = 1000;
            fdt.Constraints.Add(con);
            fdt.PrimaryKey = new System.Data.DataColumn[]{fdt.Columns[0]};

            fdt.Columns.AddRange(columns);

            return new VectorLayer(name, new GeometryFeatureProvider(fdt));
        }
               
        private VectorLayer CreateGeomLayer(string name, IGeometry[] geometries, System.Drawing.Color lineColor)
        {
            var lyr = new VectorLayer(name)
            {
                DataSource = new GeometryProvider(geometries),
                SRID = 3857
            };
            lyr.Style.Line.Color = lineColor;
            lyr.Style.Line.Width = 2f;

            return lyr;
        }
        
        private Coordinate[] GetRectanglePoints(MapDecorationAnchor anchor)
        {
            var env = this.mb.Map.Envelope;
            env.ExpandBy(-env.Width * 0.05);

            var coords = new Coordinate[5];

            Coordinate tl = null;
            switch (anchor)
            {
                case MapDecorationAnchor.LeftTop:
                    tl = env.TopLeft();
                    break;
                case MapDecorationAnchor.LeftCenter:
                    tl = new Coordinate(env.MinX, env.MaxY - env.Height * 0.375);
                    break;
                case MapDecorationAnchor.LeftBottom:
                    tl = new Coordinate(env.MinX, env.MinY + env.Height * 0.25);
                    break;
                case MapDecorationAnchor.CenterTop:
                    tl = new Coordinate(env.Centre.X - env.Width * 0.125, env.MaxY);
                    break;
                case MapDecorationAnchor.CenterBottom:
                    tl = new Coordinate(env.Centre.X - env.Width * 0.125, env.MinY + env.Height * 0.25);
                    break;
                case MapDecorationAnchor.RightTop:
                    tl = new Coordinate(env.MaxX - env.Width * 0.25, env.MaxY);
                    break;
                case MapDecorationAnchor.RightCenter:
                    tl = new Coordinate(env.MaxX - env.Width * 0.25, env.MaxY - env.Height * 0.375);
                    break;
                case MapDecorationAnchor.RightBottom:
                    tl = new Coordinate(env.MaxX - env.Width * 0.25, env.MinY + env.Height * 0.25);
                    break;
                default:
                    tl = new Coordinate(env.Centre.X - env.Width * 0.125, env.Centre.Y + env.Height * 0.125);
                    break;
            }

            coords[0] = tl;
            coords[1] = new Coordinate(tl.X + env.Width * 0.25, coords[0].Y);
            coords[2] = new Coordinate(coords[1].X, tl.Y - env.Height * 0.25);
            coords[3] = new Coordinate(coords[0].X, coords[2].Y);
            coords[4] = tl;

            return coords;
        }

        private Point GetRectangleCenter(MapDecorationAnchor anchor)
        {
            var coords = GetRectanglePoints(anchor);
            return new Point((coords[0].X + coords[1].X) / 2.0,
                             (coords[0].Y + coords[2].Y) / 2.0);
        }

        public static VectorStyle GetCharacterPointStyle(FeatureDataRow row)
         {
            var cps = new CharacterPointSymbolizer();
            cps.CharacterIndex = (int)row[1];
            cps.Font = new System.Drawing.Font("Wingdings", (float)row[2]);
            cps.Offset = new System.Drawing.PointF((float)row[3], (float)row[4]);
            return new VectorStyle() {PointSymbolizer = cps};
         }

    }

    public class MovingObjects : IDisposable
    {
        private List<MovingObject> _movingObjects = new List<MovingObject>();
        
        private readonly Timer _timer;
        private readonly VectorLayer _lyr;
        private readonly Map _map;
        public int StepSize { get; set; }
        private float _scale;
        private Color _color;

        public MovingObjects(int interval, int stepSize, VectorLayer lyr, Map map, float scale, Color color)
        {
            _timer = new Timer();
            _timer.Tick += new EventHandler(Timer_Tick);
            _timer.Interval = interval;

            StepSize = stepSize;
            _lyr = lyr;
            _map = map;
            _scale = scale;
            _color = color;
        }

        public void Start() => _timer.Enabled = true;
        public void Stop() => _timer.Enabled = false;
        public bool IsRunning => _timer.Enabled;

        public void AddObject(string name, Point startAt)
        {
            lock (((ICollection) _movingObjects).SyncRoot)
            {
                var fp = (GeometryFeatureProvider) _lyr.DataSource;
                var fdr = fp.Features.NewRow();
                fdr[1] = name;
                fdr[2] = 0;
                fdr[3] = _scale;
                fdr[4] = _color.ToArgb();
                fdr.Geometry = startAt;
                fp.Features.AddRow(fdr);
                fp.Features.AcceptChanges();

                var obj = new MovingObject(Convert.ToUInt32(fdr[0]), startAt);
                _movingObjects.Add(obj);
            }
        }

        public bool DeleteObject(uint oid)
        {
            lock (((ICollection) _movingObjects).SyncRoot)
            {
                var obj = _movingObjects.Find(p => p.Oid == oid);
                if (obj == null) return false;
                
                var fp = (GeometryFeatureProvider) _lyr.DataSource;
                var fdr = fp.Features.Rows.Find(oid);
                fp.Features.Rows.Remove(fdr);
                fp.Features.AcceptChanges();

                _movingObjects.Remove(obj);
                return true;
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            lock (((ICollection)_movingObjects).SyncRoot)
            {
                foreach (var obj in _movingObjects)
                {
                    obj.Step(_map.Envelope, StepSize);
                    var fp = (GeometryFeatureProvider) _lyr.DataSource;
                    var fdr = (FeatureDataRow) fp.Features.Rows.Find(obj.Oid);
//                    //fdr.Geometry = obj.Position;
                    fdr[2] = obj.Heading;
                    fdr.AcceptChanges();
                }
            }
            if (_lyr.Enabled) _map.VariableLayers.TouchTimer();
        }

        public void Dispose()
        {
            _timer.Tick -= new EventHandler(Timer_Tick);
            _timer?.Dispose();
        }
    }
    
    public class MovingObject
    {
        public uint Oid { get; }
        private bool _movingLeft;
        private bool _movingUp;
        public Point Position { get; private set; }
        public float Heading { get; private set; }

        public MovingObject(uint oid, Point startAt)
        {
            Oid = oid;
            Position = startAt;
            _movingUp = true;
        }
        
        public void Step(Envelope currentExtent, double stepSize)
        {
            var dx = _movingLeft ? -stepSize : stepSize;
            var dy = _movingUp ? stepSize : -stepSize;

            Position.X += dx;
            Position.Y += dy;
            Position.GeometryChanged();

            if (Position.X < currentExtent.MinX)
                _movingLeft = false;
            else if (Position.X > currentExtent.MaxX)
                _movingLeft = true;

            if (Position.Y < currentExtent.MinY)
                _movingUp = true;
            else if (Position.Y >  currentExtent.MaxY)
                _movingUp = false;

            var deg = 90 - Math.Atan2(dy, dx) * 180 / Math.PI;
            Heading = (float)(deg + 360) % 360;
        }
    }
}

