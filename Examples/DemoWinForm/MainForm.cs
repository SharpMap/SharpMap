// Copyright 2007 - Rory Plaire (codekaizen@gmail.com)
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using DemoWinForm.Properties;
using NetTopologySuite.Geometries;
using SharpMap.Data;
using SharpMap.Data.Providers;
using SharpMap.Forms;
using GeoAPI.Geometries;
using SharpMap.Layers;
using GeoPoint = GeoAPI.Geometries.Coordinate;

namespace DemoWinForm
{
    public partial class MainForm : Form
    {
        private readonly Dictionary<string, Color> _colorTable = new Dictionary<string, Color>();

        private readonly Dictionary<string, ILayerFactory> _layerFactoryCatalog =
            new Dictionary<string, ILayerFactory>();

        private readonly Dictionary<string, Bitmap> _symbolTable = new Dictionary<string, Bitmap>();

        public MainForm()
        {
            InitializeComponent();

            registerSymbols();

            registerKnownColors(_colorTable);

            registerLayerFactories();

            MainMapImage.MapQueried += MainMapImage_MapQueried;
        }

        private void registerSymbols()
        {
            _symbolTable["Notices"] = Resources.Chat;
            _symbolTable["Radioactive Fuel Rods"] = Resources.DATABASE;
            _symbolTable["Bases"] = Resources.Flag;
            _symbolTable["Houses"] = Resources.Home;
            _symbolTable["Measures"] = Resources.PIE_DIAGRAM;
            _symbolTable["Contacts"] = Resources.Women;
            _symbolTable["Prospects"] = Resources.Women_1;
        }

        private static void registerKnownColors(Dictionary<string, Color> colorTable)
        {
            foreach (string colorName in Enum.GetNames(typeof (KnownColor)))
            {
                KnownColor color = (KnownColor) Enum.Parse(typeof (KnownColor), colorName);
                colorTable[colorName] = Color.FromKnownColor(color);
            }
        }

        private void registerLayerFactories()
        {
//			ConfigurationManager.GetSection("LayerFactories");
            _layerFactoryCatalog[".shp"] = new ShapeFileLayerFactory();
        }

        private void addLayer(ILayer layer)
        {
            MainMapImage.Map.Layers.Add(layer);

            LayersDataGridView.Rows.Insert(0, true, getLayerTypeIcon(layer.GetType()), layer.LayerName);
        }

        private void addNewRandomGeometryLayer()
        {
            Random rndGen = new Random();
            Collection<IGeometry> geometry = new Collection<IGeometry>();

            VectorLayer layer = new VectorLayer(String.Empty);
            var gf = new GeometryFactory();
            switch (rndGen.Next(3))
            {
                case 0:
                    {
                        GeneratePoints(gf, geometry, rndGen);
                        KeyValuePair<string, Bitmap> symbolEntry = getSymbolEntry(rndGen.Next(_symbolTable.Count));
                        layer.Style.Symbol = symbolEntry.Value;
                        layer.LayerName = symbolEntry.Key;
                    }
                    break;
                case 1:
                    {
                        GenerateLines(gf, geometry, rndGen);
                        KeyValuePair<string, Color> colorEntry = getColorEntry(rndGen.Next(_colorTable.Count));
                        layer.Style.Line = new Pen(colorEntry.Value);
                        layer.LayerName = String.Format("{0} lines", colorEntry.Key);
                    }
                    break;
                case 2:
                    {
                        GeneratePolygons(gf, geometry, rndGen);
                        KeyValuePair<string, Color> colorEntry = getColorEntry(rndGen.Next(_colorTable.Count));
                        layer.Style.Fill = new SolidBrush(colorEntry.Value);
                        layer.LayerName = String.Format("{0} squares", colorEntry.Key);
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }

            var provider = new GeometryProvider(geometry);
            layer.DataSource = provider;

            addLayer(layer);
        }

        private KeyValuePair<string, Bitmap> getSymbolEntry(int index)
        {
            foreach (KeyValuePair<string, Bitmap> entry in _symbolTable)
            {
                if (index-- == 0)
                    return entry;
            }

            throw new InvalidOperationException();
        }

        private KeyValuePair<string, Color> getColorEntry(int index)
        {
            foreach (KeyValuePair<string, Color> entry in _colorTable)
            {
                if (index-- == 0)
                    return entry;
            }

            throw new InvalidOperationException();
        }

        private Color GetRandomColor(Random rndGen)
        {
            return Color.FromArgb(rndGen.Next(255), rndGen.Next(255), rndGen.Next(255));
        }

        private static void GeneratePolygons(IGeometryFactory factory, ICollection<IGeometry> geometry, Random rndGen)
        {
            int numPolygons = rndGen.Next(10, 100);
            for (var polyIndex = 0; polyIndex < numPolygons; polyIndex++)
            {
                var vertices = new GeoPoint[5];
                var upperLeft = new GeoPoint(rndGen.NextDouble()*1000, rndGen.NextDouble()*1000);
                var sideLength = rndGen.NextDouble()*50;

                // Make a square
                vertices[0] = new GeoPoint(upperLeft);
                vertices[1] = new GeoPoint(upperLeft.X + sideLength, upperLeft.Y);
                vertices[2] = new GeoPoint(upperLeft.X + sideLength, upperLeft.Y - sideLength);
                vertices[3] = new GeoPoint(upperLeft.X, upperLeft.Y - sideLength);
                vertices[4] = upperLeft;
                
                geometry.Add(factory.CreatePolygon(factory.CreateLinearRing(vertices), null));
            }
        }

        private static void GenerateLines(IGeometryFactory factory, ICollection<IGeometry> geometry, Random rndGen)
        {
            var numLines = rndGen.Next(10, 100);
            for (var lineIndex = 0; lineIndex < numLines; lineIndex++)
            {
                var numVerticies = rndGen.Next(4, 15);
                var vertices = new GeoPoint[numVerticies];

                var lastPoint = new GeoPoint(rndGen.NextDouble()*1000, rndGen.NextDouble()*1000);
                vertices[0] = lastPoint;

                for (var vertexIndex = 1; vertexIndex < numVerticies; vertexIndex++)
                {
                    var nextPoint = new GeoPoint(lastPoint.X + rndGen.Next(-50, 50),
                                                 lastPoint.Y + rndGen.Next(-50, 50));
                    vertices[vertexIndex] = nextPoint;

                    lastPoint = nextPoint;
                }
                geometry.Add(factory.CreateLineString(vertices));
            }
        }

        private static void GeneratePoints(IGeometryFactory factory, ICollection<IGeometry> geometry, Random rndGen)
        {
            var numPoints = rndGen.Next(10, 100);
            for (var pointIndex = 0; pointIndex < numPoints; pointIndex++)
            {
                var point = new GeoPoint(rndGen.NextDouble()*1000, rndGen.NextDouble()*1000);
                geometry.Add(factory.CreatePoint(point));
            }
        }

        private void LoadLayer()
        {
            var result = AddLayerDialog.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                foreach (var fileName in AddLayerDialog.FileNames)
                {
                    var extension = Path.GetExtension(fileName);
                    ILayerFactory layerFactory = null;

                    if (!_layerFactoryCatalog.TryGetValue(extension, out layerFactory))
                        continue;

                    ILayer layer = layerFactory.Create(Path.GetFileNameWithoutExtension(fileName), fileName);

                    addLayer(layer);
                }

                changeUIOnLayerSelectionChange();

                MainMapImage.Refresh();
            }
        }

        private void RemoveLayer()
        {
            if (LayersDataGridView.SelectedRows.Count == 0)
                return;

            string layerName = LayersDataGridView.SelectedRows[0].Cells[2].Value as string;

            ILayer layerToRemove = null;

            foreach (ILayer layer in MainMapImage.Map.Layers)
                if (layer.LayerName == layerName)
                {
                    layerToRemove = layer;
                    break;
                }

            MainMapImage.Map.Layers.Remove(layerToRemove);
            LayersDataGridView.Rows.Remove(LayersDataGridView.SelectedRows[0]);
        }

        private void zoomToExtents()
        {
            MainMapImage.Map.ZoomToExtents();
            MainMapImage.Refresh();
        }

        private void changeMode(MapImage.Tools tool)
        {
            MainMapImage.ActiveTool = tool;

            ZoomInModeToolStripButton.Checked = (tool == MapImage.Tools.ZoomIn);
            ZoomOutModeToolStripButton.Checked = (tool == MapImage.Tools.ZoomOut);
            PanToolStripButton.Checked = (tool == MapImage.Tools.Pan);
            QueryModeToolStripButton.Checked = (tool == MapImage.Tools.Query);
        }

        private object getLayerTypeIcon(Type type)
        {
            if (type == typeof (VectorLayer))
            {
                return Resources.polygon;
            }

            return Resources.Raster;
        }

        private void changeUIOnLayerSelectionChange()
        {
            bool isLayerSelected = false;
            int layerIndex = -1;

            if (LayersDataGridView.SelectedRows.Count > 0)
            {
                isLayerSelected = true;
                layerIndex = LayersDataGridView.SelectedRows[0].Index;
            }

            RemoveLayerToolStripButton.Enabled = isLayerSelected;
            RemoveLayerToolStripMenuItem.Enabled = isLayerSelected;

            if (layerIndex < 0)
            {
                MoveUpToolStripMenuItem.Visible = false;
                MoveDownToolStripMenuItem.Visible = false;
                LayerContextMenuSeparator.Visible = false;
                return;
            }
            else
            {
                MoveUpToolStripMenuItem.Visible = true;
                MoveDownToolStripMenuItem.Visible = true;
                LayerContextMenuSeparator.Visible = true;
            }

            if (layerIndex == 0)
            {
                MoveUpToolStripMenuItem.Enabled = false;
            }
            else
            {
                MoveUpToolStripMenuItem.Enabled = true;
            }

            if (layerIndex == LayersDataGridView.Rows.Count - 1)
            {
                MoveDownToolStripMenuItem.Enabled = false;
            }
            else
            {
                MoveDownToolStripMenuItem.Enabled = true;
            }
        }

        private void MainMapImage_MapQueried(FeatureDataTable data)
        {
            FeaturesDataGridView.DataSource = data;
        }

        private void AddLayerToolStripButton_Click(object sender, EventArgs e)
        {
            BeginInvoke((MethodInvoker) delegate { LoadLayer(); });
        }

        private void RemoveLayerToolStripButton_Click(object sender, EventArgs e)
        {
            BeginInvoke((MethodInvoker) delegate { RemoveLayer(); });
        }

        private void AddLayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BeginInvoke((MethodInvoker) delegate { LoadLayer(); });
        }

        private void RemoveLayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BeginInvoke((MethodInvoker) delegate { RemoveLayer(); });
        }

        private void ZoomToExtentsToolStripButton_Click(object sender, EventArgs e)
        {
            BeginInvoke((MethodInvoker) delegate { zoomToExtents(); });
        }

        private void PanToolStripButton_Click(object sender, EventArgs e)
        {
            BeginInvoke((MethodInvoker) delegate { changeMode(MapImage.Tools.Pan); });
        }

        private void QueryModeToolStripButton_Click(object sender, EventArgs e)
        {
            BeginInvoke((MethodInvoker) delegate { changeMode(MapImage.Tools.Query); });
        }

        private void ZoomInModeToolStripButton_Click(object sender, EventArgs e)
        {
            BeginInvoke((MethodInvoker) delegate { changeMode(MapImage.Tools.ZoomIn); });
        }

        private void ZoomOutModeToolStripButton_Click(object sender, EventArgs e)
        {
            BeginInvoke((MethodInvoker) delegate { changeMode(MapImage.Tools.ZoomOut); });
        }

        private void MoveUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (LayersDataGridView.SelectedRows.Count == 0)
                return;

            if (LayersDataGridView.SelectedRows[0].Index == 0)
                return;

            int rowIndex = LayersDataGridView.SelectedRows[0].Index;
            DataGridViewRow row = LayersDataGridView.Rows[rowIndex];
            LayersDataGridView.Rows.RemoveAt(rowIndex);
            LayersDataGridView.Rows.Insert(rowIndex - 1, row);

            int layerIndex = MainMapImage.Map.Layers.Count - rowIndex - 1;
            ILayer layer = MainMapImage.Map.Layers[layerIndex];
            MainMapImage.Map.Layers.RemoveAt(layerIndex);
            MainMapImage.Map.Layers.Insert(layerIndex + 1, layer);
        }

        private void MoveDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (LayersDataGridView.SelectedRows.Count == 0)
                return;

            if (LayersDataGridView.SelectedRows[0].Index == LayersDataGridView.Rows.Count - 1)
                return;

            int rowIndex = LayersDataGridView.SelectedRows[0].Index;
            DataGridViewRow row = LayersDataGridView.Rows[rowIndex];
            LayersDataGridView.Rows.RemoveAt(rowIndex);
            LayersDataGridView.Rows.Insert(rowIndex + 1, row);

            int layerIndex = MainMapImage.Map.Layers.Count - rowIndex - 1;
            ILayer layer = MainMapImage.Map.Layers[layerIndex];
            MainMapImage.Map.Layers.RemoveAt(layerIndex);
            MainMapImage.Map.Layers.Insert(layerIndex - 1, layer);
        }

        private void LayersDataGridView_SelectionChanged(object sender, EventArgs e)
        {
            BeginInvoke((MethodInvoker) delegate
                                            {
                                                changeUIOnLayerSelectionChange();

                                                if (LayersDataGridView.SelectedRows.Count > 0)
                                                {
                                                    MainMapImage.QueryLayerIndex =
                                                        LayersDataGridView.SelectedRows[0].Index;
                                                }
                                            });
        }

        private void MainMapImage_MouseMove(GeoPoint WorldPos, MouseEventArgs ImagePos)
        {
            CoordinatesLabel.Text = String.Format("Coordinates: {0:N5}, {1:N5}", WorldPos.X, WorldPos.Y);
        }

        private void AddNewRandomGeometryLayer_Click(object sender, EventArgs e)
        {
            BeginInvoke((MethodInvoker) delegate { addNewRandomGeometryLayer(); });
        }
    }
}