using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GeoAPI.Geometries;
using RoutingExample.RoutingSystem;
using System.Collections.ObjectModel;
using SharpMap.Layers;

namespace RoutingExample
{
    public partial class Main : Form
    {

        /// <summary>
        /// The routing system class provides a wrapper for the system.
        /// 1 Create Instance
        /// 2 Pass vector layer
        /// 3 Pass point for the source
        /// 4 Pass point for the destination
        /// 5 Call calcualte method. 2 MUST be done before 3,4,5 or the mayans will be proved right.
        /// </summary>
        RoutingSystem.RoutingSystem TheRoutingEngine;

        public static string BASEMAPLAYERNAME = "BASEMAP";
       
        public Main()
        {
            InitializeComponent();
            TheRoutingEngine = new RoutingSystem.RoutingSystem();
        }

        /// <summary>
        /// Opens a shape file. Hopefully.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOpenShapeFile_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog OfD = new OpenFileDialog();
                OfD.InitialDirectory = Application.StartupPath + "\\TestData\\";
                OfD.Filter = "Shape Files (*.shp)|*.shp";

                if (OfD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (OfD.CheckFileExists)
                    {
                        // The file should be there so we create a new layer with the name
                        if (!LoadTheBasicLayerIntoSharpMap(OfD.FileName))
                        {
                            MessageBox.Show("Something Went Very Very Wrong.", "Unable To Load The Shape File.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                }

            }
            catch (Exception)
            {
                MessageBox.Show("Danger, Will Robinson, Danger.", "Cant open the shape file", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
        }

        private bool LoadTheBasicLayerIntoSharpMap(string p)
        {
            try
            {
                SharpMap.Layers.VectorLayer temp = null;

                foreach (SharpMap.Layers.VectorLayer tLayer in mapBox1.Map.Layers)
                {
                    if (tLayer.LayerName == Main.BASEMAPLAYERNAME)
                        temp = tLayer;
                }

                mapBox1.Map.Layers.Remove(temp);

                SharpMap.Layers.VectorLayer TheLayer = new SharpMap.Layers.VectorLayer(Main.BASEMAPLAYERNAME);
                TheLayer.DataSource = new SharpMap.Data.Providers.ShapeFile(p);

                mapBox1.Map.Layers.Add(TheLayer);
                mapBox1.Map.ZoomToExtents();
                mapBox1.Refresh();
                mapBox1.Update();

                // The Routing Engine needs a copy of the layer. 
                TheRoutingEngine.AnalysisLayer = TheLayer;

                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var theShortestPath = TheRoutingEngine.PerformShortestPathAnalysis(false);

            if (theShortestPath == null)
            {
                label4.Text = "No Path Found.";
                System.Diagnostics.Debug.WriteLine("Either a problem occured or there was no connecting path.");
            }
            else
            {
                // THIS NEXT BIT ONLY DISPLAYS THE PATH ON THE MAP, AND SOME PATH LENGTH INFORMATION
                RemoveShortestPathLineFromMapIfPresent();      
                SharpMap.Layers.VectorLayer GraphicsLayer = GetGraphicsLayer(theShortestPath);
                mapBox1.Map.Layers.Add(GraphicsLayer);
                mapBox1.Refresh();
                mapBox1.Update();
                label4.Text = "Shortest Path Length: " + Math.Round(theShortestPath.Length, 2);
            }

            // END OF MAP DISPLAY BIT

        }


        /// <summary>
        /// This method just takes a linestring representing a shortest path and turns into a vector layer
        /// </summary>
        /// <param name="TheShortestPath">The Line string of the shortest path</param>
        /// <returns>The Vector Layer</returns>
        private static VectorLayer GetGraphicsLayer(ILineString TheShortestPath)
        {

            try
            {
                Collection<IGeometry> GeomCollection = new Collection<IGeometry>();
                GeomCollection.Add(TheShortestPath);

                VectorLayer VLayer = new VectorLayer("SPGL");
                VLayer.DataSource = new SharpMap.Data.Providers.GeometryProvider(GeomCollection);
                VLayer.Style.Line = new Pen(Color.Red, 3);
                return VLayer;
            }
            catch (Exception)
            {
                return null;
            }
        
        }

        /// <summary>
        /// removes the shortest path from the map if already present.
        /// </summary>
        private void RemoveShortestPathLineFromMapIfPresent()
        {
            var tempLayer = new VectorLayer("SPGL");
            foreach (VectorLayer VL in mapBox1.Map.Layers)
                if (VL.LayerName == "SPGL")
                {
                    tempLayer = VL;
                }

            mapBox1.Map.Layers.Remove(tempLayer);
        }

        private void mapBox1_MouseDown(Coordinate worldPos, MouseEventArgs imagePos)
        {

                if (imagePos.Button == System.Windows.Forms.MouseButtons.Left)
                    TheRoutingEngine.UserSource = worldPos;                    
               
                if (imagePos.Button == System.Windows.Forms.MouseButtons.Right)
                    TheRoutingEngine.UserDestination = worldPos;
        }
    }
}
