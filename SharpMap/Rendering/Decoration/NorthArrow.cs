using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using SharpMap.Utilities;

namespace SharpMap.Rendering.Decoration
{
    /// <summary>
    /// North arrow map decoration
    /// </summary>
    [Serializable]
    public class NorthArrow : MapDecoration
    {
        private static readonly object _lockObject = new object();
        private static readonly Bitmap DefaultNorthArrowBitmap;

        static NorthArrow()
        {
            lock(_lockObject)
            {
                DefaultNorthArrowBitmap = new Bitmap(120, 120);
                using (var g = Graphics.FromImage(DefaultNorthArrowBitmap))
                {
                    g.Clear(Color.Transparent);
                    var b = new SolidBrush(Color.Black);
                    var p = new Pen(new SolidBrush(Color.Black), 10) {LineJoin = LineJoin.Miter};
                    g.FillEllipse(b, new RectangleF(50, 50, 20, 20));
                    g.DrawLine(p, 60, 110, 60, 40);
                    var pts = new[] {new PointF(45, 40), new PointF(60, 10), new PointF(75, 40), new PointF(45, 40)};
                    g.FillPolygon(b, pts);
                    g.DrawPolygon(p, pts);

                    b = new SolidBrush(Color.White);
                    g.DrawString("N", new Font("Arial", 20, FontStyle.Bold, GraphicsUnit.Pixel), b, new RectangleF(50, 25, 20, 20),
                                 new StringFormat
                                     {LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center});
                }
            }
        }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        public NorthArrow()
        {
            Size = new Size(45, 45);
            ForeColor = Color.Silver;
            Location = new Point(5, 5);
            Anchor = MapDecorationAnchor.LeftTop;
        }

        /// <summary>
        /// Gets or sets the NorthArrowImage
        /// </summary>
        public Image NorthArrowImage { get; set; }

        /// <summary>
        /// Gets or sets the size of the North arrow Bitmap
        /// </summary>
        public Size Size { get; set; }

        /// <summary>
        /// Gets or sets the fore color
        /// </summary>
        public Color ForeColor { get; set; }


        #region MapDecoration overrides

        /// <summary>
        /// Function to compute the required size for rendering the map decoration object
        /// <para>This is just the size of the decoration object, border settings are excluded</para>
        /// </summary>
        /// <param name="g">The graphics object</param>
        /// <param name="map">The map</param>
        /// <returns>The size of the map decoration</returns>
        protected override Size InternalSize(Graphics g, Map map)
        {
            return Size;
        }

        /// <summary>
        /// Function to render the actual map decoration
        /// </summary>
        /// <param name="g"></param>
        /// <param name="map"></param>
        protected override void OnRender(Graphics g, Map map)
        {
            var image = NorthArrowImage ?? DefaultNorthArrowBitmap;

            var mapSize = map.Size;

            //Get rotation
            var ptTop = map.ImageToWorld(new PointF(mapSize.Width/2f, 0f),true);
            var ptBottom = map.ImageToWorld(new PointF(mapSize.Width / 2f, mapSize.Height * 0.5f), true);

            var dx = ptTop.X - ptBottom.X;
            var dy = ptBottom.Y - ptTop.Y;
            var length = Math.Sqrt(dx*dx + dy*dy);

            var cos = dx/length;

            var rot = -90 + (dy > 0 ? -1 : 1) * Math.Acos(cos) / GeoSpatialMath.DegToRad;
            var halfSize = new Size((int)(0.5f*Size.Width), (int)(0.5f*Size.Height));
            var oldTransform = g.Transform;
            
            var clip = g.ClipBounds;
            var newTransform = new Matrix(1f, 0f, 0f, 1f, 
                                          clip.Left + halfSize.Width,
                                          clip.Top + halfSize.Height);
            newTransform.Rotate((float)rot);

            // Setup image attributes
            var ia = new ImageAttributes();
            var cmap = new [] { 
                new ColorMap { OldColor = Color.Transparent, NewColor = OpacityColor(BackgroundColor) },
                new ColorMap { OldColor = Color.Black, NewColor = OpacityColor(ForeColor) }
            };
            ia.SetRemapTable( cmap );

            g.Transform = newTransform;
            
            var rect = new Rectangle(-halfSize.Width, -halfSize.Height, Size.Width, Size.Height);
            g.DrawImage(image, rect, 0, 0, image.Size.Width, image.Size.Height, GraphicsUnit.Pixel, ia);

            g.Transform = oldTransform;
        }

        #endregion
    }
}