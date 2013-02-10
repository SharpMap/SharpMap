using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SharpMap.Rendering.Decoration
{
    /// <summary>
    /// Eye of sight class
    /// </summary>
    [Serializable]
    public class EyeOfSight : NorthArrow
    {
        private static readonly string[] Directions = {"N", "E", "S", "W"};

        private static Bitmap GetEyeOfSightImage()
        {
            var roestte = new Bitmap(120, 120);
            
            //Anyone for a more sophisticated roestte?
            using (var g = Graphics.FromImage(roestte))
            {
                var t = new Matrix(1f, 0f, 0f, 1f, 60, 60);
                g.Transform = t;

                var f = new Font("Arial", 20, FontStyle.Bold);
                var b = new SolidBrush(Color.Black);
                var p = new Pen(Color.Black, 5);
                var sf = new StringFormat(StringFormat.GenericTypographic) {Alignment = StringAlignment.Center};
                var rect = new RectangleF(- 45f, - 45f, 90f, 90f);

                foreach (var s in Directions)
                {
                    g.DrawString(s, f, b, 0, -55, sf);
                    g.DrawArc(p, rect, 290f, 50f);
                    g.RotateTransform(90f);
                }
            }
            return roestte;
        }
        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        public EyeOfSight()
        {
            NorthArrowImage = GetEyeOfSightImage();
        }
        
        /// <summary>
        /// Gets or sets the outline color 
        /// </summary>
        public Color NeedleOutlineColor { get; set; }
        
        /// <summary>
        /// The width of the needle outline
        /// </summary>
        public int NeedleOutlineWidth { get; set; }
        
        /// <summary>
        /// The color to
        /// </summary>
        public Color NeedleFillColor { get; set; }

        #region MapDecoration overrides

        /// <summary>
        /// Function to render the actual map decoration
        /// </summary>
        /// <param name="g"></param>
        /// <param name="map"></param>
        protected override void OnRender(Graphics g, Map map)
        {
            // Render the rosetta
            base.OnRender(g, map);
            
            var clip = g.ClipBounds;
            var oldTransform = g.Transform;
            var newTransform = new Matrix(1f, 0f, 0f, 1f, clip.Left + Size.Width*0.5f, clip.Top + Size.Height*0.5f);

            g.Transform = newTransform;

            var width = Size.Width;
            var height = Size.Height;
            var pts = new[]
                          {
                              new PointF(0f, -0.35f*height),
                              new PointF(0.125f*width, 0.35f*height),
                              new PointF(0f, 0.275f*height),
                              new PointF(-0.125f*width, 0.35f*height),
                              new PointF(0f, -0.35f*height),
                          };

            // need to outline the needle
            if (NeedleOutlineWidth>0)
            {
                g.DrawPolygon(new Pen(OpacityColor(NeedleOutlineColor), NeedleOutlineWidth), pts);
            }

            // need to outline the needle
            g.FillPolygon(new SolidBrush(OpacityColor(NeedleFillColor)), pts );

            g.Transform = oldTransform;

        }
        
        #endregion
    }
}