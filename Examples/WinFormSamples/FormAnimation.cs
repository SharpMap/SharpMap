using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GeoAPI.Geometries;
using SharpMap.Layers;
using SharpMap.Data;
using SharpMap.Styles;
using SharpMap.Rendering.Thematics;
using BruTile.Web;
using System.IO;
using System.Drawing.Drawing2D;
using SharpMap.Data.Providers;
using System.Threading;

namespace WinFormSamples
{
    public partial class FormAnimation : Form
    {

        public FormAnimation()
        {
   
            InitializeComponent();
            this.SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
        }

        private void FormAnimation_Load(object sender, EventArgs e)
        {

            //Set up the countries layer
            VectorLayer layCountries = new VectorLayer("Countries");
            //Set the datasource to a shapefile in the App_data folder
            layCountries.DataSource = new ShapeFile("GeoData/World/countries.shp", true);
            //Set fill-style to green
            layCountries.Style.Fill = new SolidBrush(Color.Green);
            //Set the polygons to have a black outline
            layCountries.Style.Outline = Pens.Black;
            layCountries.Style.EnableOutline = true;
            layCountries.SRID = 4326;

            this.mapBox1.Map.Layers.Add(layCountries);

            

            this.mapBox1.Map.ZoomToExtents(); 
            this.mapBox1.Refresh();


            
            
        }



        private void Form2_SizeChanged(object sender, EventArgs e)
        {
            this.mapBox1.Refresh();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (this.folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                var bb = this.mapBox1.Map.Envelope;

                for (int i = 0; i < 50; i=i+5)
                {

                    this.mapBox1.Map.ZoomToBox(new Envelope(bb.MinX - i, bb.MaxX + i, bb.MinY - i, bb.MaxY+ i));
                    this.mapBox1.Refresh();

                    Image image = mapBox1.Image;
                    image.Save(Path.Combine(this.folderBrowserDialog1.SelectedPath, "SaveSharpMapImage_" + i.ToString() + ".png"), System.Drawing.Imaging.ImageFormat.Png);
                }

                this.mapBox1.Map.ZoomToExtents();
                this.mapBox1.Refresh();

            }
            
        }

    }
}
