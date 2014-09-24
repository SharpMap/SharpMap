using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using SharpMap;
using SharpMap.Data.Providers;
using SharpMap.Layers;
using SharpMap.Rendering;
using SharpMap.Rendering.Symbolizer;
using SharpMap.Styles;
using Point = GeoAPI.Geometries.Coordinate;
namespace WinFormSamples.Samples
{
    class TextOnPathSample
    {
        public static Map InitializeMapOrig(float angle)
        {
            //Initialize a new map of size 'imagesize'
            Map map = new Map();

            //Set up the countries layer
            VectorLayer layRoads = new VectorLayer("Roads");
            //Set the datasource to a shapefile in the App_data folder
            layRoads.DataSource = new ShapeFile("GeoData/World/shp_textonpath/DeMo_Quan5.shp", false);
            (layRoads.DataSource as ShapeFile).Encoding = Encoding.UTF8;
            //Set fill-style to green
            layRoads.Style.Fill = new SolidBrush(Color.Yellow);
            layRoads.Style.Line = new Pen(Color.Yellow, 4);
            //Set the polygons to have a black outline
            layRoads.Style.Outline = new Pen(Color.Black, 5); ;
            layRoads.Style.EnableOutline = true;
            layRoads.SRID = 4326;

            //Set up a country label layer
            LabelLayer layLabel = new LabelLayer("Roads labels");
            layLabel.DataSource = layRoads.DataSource;
            layLabel.Enabled = true;
            layLabel.LabelColumn = "tenduong";
            layLabel.LabelFilter = SharpMap.Rendering.LabelCollisionDetection.ThoroughCollisionDetection;
            layLabel.Style = new LabelStyle();
            layLabel.Style.ForeColor = Color.White;
            layLabel.Style.Font = new Font(FontFamily.GenericSerif, 9f, FontStyle.Bold);
            layLabel.Style.Halo = new Pen(Color.Black, 2f);
            layLabel.Style.IsTextOnPath = true;
            layLabel.Style.CollisionDetection = true;
            //layLabel.Style.BackColor = new SolidBrush(Color.FromArgb(128, 255, 0, 0));
            //layLabel.MaxVisible = 90;
            //layLabel.MinVisible = 30;
            layLabel.Style.HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center;
            layLabel.SRID = 4326;
            //layLabel.MultipartGeometryBehaviour = LabelLayer.MultipartGeometryBehaviourEnum.Largest;

          
            //Add the layers to the map object.
            //The order we add them in are the order they are drawn, so we add the rivers last to put them on top
            map.Layers.Add(layRoads);          
            map.Layers.Add(layLabel);        


            //limit the zoom to 360 degrees width
            //map.MaximumZoom = 360;
           // map.BackColor = Color.LightBlue;

            map.ZoomToExtents();

            Matrix mat = new Matrix();
            mat.RotateAt(angle, map.WorldToImage(map.Center));
            map.MapTransform = mat;

            return map;
        }
    }
}
