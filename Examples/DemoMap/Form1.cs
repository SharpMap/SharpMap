using SharpMap.Data.Providers;
using SharpMap.Layers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DemoMap
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            FormAnimation_Load(this, EventArgs.Empty);

        }


        private void FormAnimation_Load(object sender, EventArgs e)
        {

            


            //Set up the countries layer
            VectorLayer layCountries = new VectorLayer("Countries");
            //Set the datasource to a shapefile in the App_data folder
            layCountries.DataSource = new ShapeFile("App_Data\\countries.shp", true);
            //Set fill-style to green
            layCountries.Style.Fill = new SolidBrush(Color.BlueViolet);
            //Set the polygons to have a black outline
            layCountries.Style.Outline = Pens.Black;
            layCountries.Style.EnableOutline = true;
            layCountries.SRID = 4326;

            this.mapBox1.Map.Layers.Add(layCountries);



            this.mapBox1.Map.ZoomToExtents();
            this.mapBox1.Refresh();




        }

    }
}
