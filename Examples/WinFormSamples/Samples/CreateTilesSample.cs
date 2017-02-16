using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using GeoAPI.CoordinateSystems.Transformations;

/*------------------------------------------------------------------------------ 
Create Date : 2011-09-25
Author     : TrieuVy ( goldmelodyvn )
------------------------------------------------------------------------------
Update History:
Ver.    TRB#        Date          Author                Note
------------------------------------------------------------------------------*/
namespace WinFormSamples.Samples
{
    public class CreateTilesSample : IDisposable
    {
        private readonly SharpMap.Map _map;
        private int _typeImage = 0;
        //public System.Drawing.Imaging.ImageFormat ImageFormat { get; set; }

        private readonly string _rootTilesPath;
        private Dictionary<SharpMap.Layers.ILayer, ICoordinateTransformation> _coordinateTransformations =
            new Dictionary<SharpMap.Layers.ILayer, ICoordinateTransformation>();

        private float _opacity = 0.3f;
        public  float Opacity
        {
            get { return _opacity; }
            set
            {
                if (value < 0) value = 0;
                if (value > 1) value = 1;
                _opacity = value;
            }
        }

        public CreateTilesSample(SharpMap.Map map, bool transformToMercator)
            : this(map, transformToMercator, Environment.CurrentDirectory)
        {
        }

        public CreateTilesSample(SharpMap.Map map, bool transformToMercator, string rootTilesPath)
        {
            _map = map.Clone();
            
            _map.MaximumZoom = double.MaxValue;
            _map.MinimumZoom = 0;

            if (transformToMercator)
            {
                TransformLayers(LayerTools.Wgs84toGoogleMercator);
            }

            _rootTilesPath = rootTilesPath;
            if (!Directory.Exists(_rootTilesPath))
            {
                Directory.CreateDirectory(_rootTilesPath);
            }
        }
        
        private void TransformLayers(ICoordinateTransformation coorTransform)
        {
            foreach (SharpMap.Layers.ILayer layer in _map.Layers)
            {
                SharpMap.Layers.VectorLayer vLayer = layer as SharpMap.Layers.VectorLayer;
                if (vLayer != null)
                {
                    if (vLayer.CoordinateTransformation != null)
                        _coordinateTransformations.Add(vLayer, vLayer.CoordinateTransformation);
                    vLayer.CoordinateTransformation = coorTransform;
                }
                else
                {
                    SharpMap.Layers.LabelLayer lLayer = layer as SharpMap.Layers.LabelLayer;
                    if (lLayer != null)
                    {
                        if (lLayer.CoordinateTransformation != null)
                            _coordinateTransformations.Add(lLayer, lLayer.CoordinateTransformation);
                        lLayer.CoordinateTransformation = coorTransform;
                    }
                }
            }
        }

        private void RestoreTransformations()
        {
            foreach (var layer in  _map.Layers)
            {
                ICoordinateTransformation ct;
                _coordinateTransformations.TryGetValue(layer, out ct);

                var vlayer = layer as SharpMap.Layers.VectorLayer;
                if (vlayer != null)
                {
                    vlayer.CoordinateTransformation = ct;
                    continue;
                }
                var llayer = layer as SharpMap.Layers.LabelLayer;
                if (llayer != null)
                    llayer.CoordinateTransformation = ct;

            }
        }
        public void SaveImagesAtLevel(int level)
        {
            try
            {
                var path = string.Format(Path.Combine(_rootTilesPath, level.ToString()));
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);                    
                }

                _map.Size = new Size(256, 256);
                _map.ZoomToExtents();
                
                // number images of title on a line
                var lineNumberImage = (int)(Math.Pow(2, level));
                _map.Zoom = _map.Zoom / lineNumberImage;
                
                // 1/2 length image in world
                var delta = _map.Center.X - _map.Envelope.MinX;
                
                // image size per tile ( in world )
                var imageWidth = _map.Envelope.MaxX - _map.Envelope.MinX;
                var imageHeight = imageWidth;

                // move center to left-up img ( left-bottom in pixel )          
                var centerX0 = _map.Center.X - (lineNumberImage * imageWidth) / 2 + delta;
                var centerY0 = _map.Center.Y + (lineNumberImage * imageHeight) / 2 - delta;

                var ia = new System.Drawing.Imaging.ImageAttributes();
                var cm = new System.Drawing.Imaging.ColorMatrix {Matrix33 = _opacity};
                ia.SetColorMatrix(cm, System.Drawing.Imaging.ColorMatrixFlag.Default, System.Drawing.Imaging.ColorAdjustType.Bitmap);

                // All tile columns
                var centerX = centerX0;
                for (var i = 0; i < lineNumberImage; i++, centerX = centerX + imageWidth)
                {
                    var colPath = Path.Combine(path, i.ToString());
                    if (!Directory.Exists(colPath))
                        Directory.CreateDirectory(colPath);

                    var centerY = centerY0;
                    for (var j = 0; j < lineNumberImage; j++, centerY = centerY - imageHeight)
                    {
                        _map.Center = new GeoAPI.Geometries.Coordinate(centerX, centerY);
                        using (var img = _map.GetMap())
                        {
                            using (var transImg = new System.Drawing.Bitmap(img.Width, img.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                            {
                                using (var g = System.Drawing.Graphics.FromImage(transImg))
                                {
                                    g.DrawImage(img, 
                                        new[] { 
                                            new Point(0, 0), 
                                            new Point(transImg.Size.Width, 0), 
                                            new Point(0, transImg.Size.Height), 
                                            /*new Point(transImg.Size)*/ },
                                            new Rectangle(new Point(0, 0), img.Size), GraphicsUnit.Pixel, ia);
                                }
                                SaveImage(transImg, colPath, j, _typeImage);
                            }
                        }
                    }
                }
            }
            catch(Exception ex) {
                throw new Exception(ex.Message);
            }
        }
        private static void SaveImage(Image img, string colPath, int imageId, int type)
        {
            switch (type)
            {
                default:
                //case 0:
                    img.Save(Path.Combine(colPath, string.Format("{0}.png", imageId)), System.Drawing.Imaging.ImageFormat.Png);
                    break;
                case 1:
                    img.Save(Path.Combine(colPath, string.Format("{0}.jpg", imageId)), System.Drawing.Imaging.ImageFormat.Jpeg);
                    break;
                case 2:
                    img.Save(Path.Combine(colPath, string.Format("{0}.gif", imageId)), System.Drawing.Imaging.ImageFormat.Gif);
                    break;
                case 3:
                    img.Save(Path.Combine(colPath, string.Format("{0}.bmp", imageId)), System.Drawing.Imaging.ImageFormat.Bmp);
                    break;
                case 4:
                    img.Save(Path.Combine(colPath, string.Format("{0}.tiff", imageId)), System.Drawing.Imaging.ImageFormat.Tiff);
                    break;
            }
        }

        public static void CreateHtmlSamplePage(string path, string googleMapsApiKey)
        {
            using (var writer = new System.IO.StreamWriter(File.OpenWrite(path)))
            {
                writer.Write(
@"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN""
  ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"">
<html xmlns=""http://www.w3.org/1999/xhtml"">
  <head>
    <meta http-equiv=""content-type"" content=""text/html; charset=utf-8""/>
    <title>SharpMap Tile use google API</title>
    <script src=""http://maps.google.com/maps?file=api&amp;v=2&amp;key=");
                writer.WriteLine(string.Format("\"{0}\"", googleMapsApiKey));
                writer.WriteLine(
@"			type=""text/javascript""></script>
    <script type=""text/javascript"">");
                writer.WriteLine(string.Format("        path='file:///{0}';", new Uri(Path.GetDirectoryName(path)).AbsolutePath));
                writer.WriteLine(
@"        var map = '';
        function load() {
            if (GBrowserIsCompatible()) {
                map = new GMap2(document.getElementById(""map""), { draggableCursor: 'crosshair', draggingCursor: 'pointer' });
                map.addControl(new GLargeMapControl());
                map.addControl(new GMapTypeControl());
                map.enableScrollWheelZoom();
                var point = new GLatLng(10.73769, 106.71089);

                // copyCollection.addCopyright(copyright);
                // Set up the copyright information
                // Each image used should indicate its copyright permissions
                var myCopyright = new GCopyrightCollection(""SharpMap"");
                myCopyright.addCopyright(new GCopyright('Demo',
			  new GLatLngBounds(new GLatLng(-90, -180), new GLatLng(90, 180)),
			  0, 'goldmelodyvn@2011 '));
                //*******************************
                var goldmelodyvn_tilelayer1 = new GTileLayer(myCopyright);
                goldmelodyvn_tilelayer1.getTileUrl = GetGoldmelodyvnTile;
                goldmelodyvn_tilelayer1.isPng = function () { return true; };
                goldmelodyvn_tilelayer1.getOpacity = function () { return 0.9; }
                var G_goldmelodyvn = [G_SATELLITE_MAP.getTileLayers()[0], goldmelodyvn_tilelayer1];
                var GMapTypeOptions = new Object();
                GMapTypeOptions.minResolution = 0;
                GMapTypeOptions.maxResolution = 20;
                GMapTypeOptions.errorMessage = ""No map data available"";
                var maptype_sharpmap = new GMapType(G_goldmelodyvn, new GMercatorProjection(22), ""SharpMap"", GMapTypeOptions);
                //************************************
                map.addMapType(maptype_sharpmap);
                map.setCenter(point, 0, maptype_sharpmap);
            }
        }
        function GetGoldmelodyvnTile(a, b) {           
            var i = a.x;
            var j = a.y ;
            var tmp = path+ ""/"" + b + ""/"" + i + ""/"" + j + "".png"";
            return tmp;
        }	
    </script>
 </head>

<body onload=""load()"" onunload=""GUnload()"">
<div id=""map"" style=""height:500px; width:1024px; margin:10px; border:1px #b1c4d5 solid;""></div>
<p>SharpMap Tile overlay on Google's Sattelite</p>
</body>
</html>");
            }
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool IsDisposed { get; protected set; }
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!IsDisposed)
                {
                    RestoreTransformations();
                    _map.Dispose();

                }
            }
        }

        #endregion
    }
}
