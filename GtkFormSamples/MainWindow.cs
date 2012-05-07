using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using NetTopologySuite;

//using Gtk;

namespace TestGtk
{
    public partial class MainWindow : Gtk.Window
    {
        private SharpMap.Map myMap;

        public MainWindow()
            : base(Gtk.WindowType.Toplevel)
        {
            try
            {
                GeoAPI.GeometryServiceProvider.Instance = new NtsGeometryServices();

                Build();

                Size mapSize = new Size(800, 500);
                myMap = new SharpMap.Map(mapSize);

                SharpMap.Styles.VectorStyle style = new SharpMap.Styles.VectorStyle();
                style.Outline = new Pen(Color.Green, 1);
                style.EnableOutline = true;
                SharpMap.Layers.VectorLayer layWorld = new SharpMap.Layers.VectorLayer("States");
                layWorld.DataSource =
                    new SharpMap.Data.Providers.ShapeFile(
                        @"data" + System.IO.Path.DirectorySeparatorChar + @"states.shp", true);
                layWorld.Style = style;

                myMap.Layers.Add(layWorld);
                myMap.MaximumZoom = 360;
                myMap.BackColor = Color.LightBlue;
                myMap.Center = new GeoAPI.Geometries.Coordinate(0, 0);
                myMap.Zoom = 360;

                Bitmap img = (Bitmap)myMap.GetMap();
                image3.Pixbuf = ImageToPixbuf(img);
            }
            catch (Exception ex)
            {
                label1.Text = ex.Message;
            }
        }

        protected void OnDeleteEvent(object sender, Gtk.DeleteEventArgs a)
        {
            Gtk.Application.Quit();
            a.RetVal = true;
        }

        private static Gdk.Pixbuf ImageToPixbuf(System.Drawing.Image image)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                image.Save("test.png", ImageFormat.Png);
                image.Save(stream, ImageFormat.Png);
                stream.Position = 0;
                Gdk.Pixbuf pixbuf = new Gdk.Pixbuf(stream);
                return pixbuf;
            }
        }

        protected virtual void EventBoxButtonPress(object o, Gtk.ButtonPressEventArgs args)
        {
            double dX = args.Event.X;
            double dY = args.Event.Y;
            System.Drawing.PointF oPointF = new System.Drawing.PointF((float)dX, (float)dY);

            label1.Text = dX.ToString() + " , " + dY.ToString();

            myMap.Center = myMap.ImageToWorld(oPointF);

            if (radPan.Active)
            {
                myMap.Zoom *= 1;
            }
            else if (radZoomIn.Active)
            {
                myMap.Zoom *= 0.5;
            }
            else
            {
                myMap.Zoom *= 1.5;
            }

            Bitmap img = (Bitmap)myMap.GetMap();
            image3.Pixbuf = ImageToPixbuf(img);
        }
    }
}