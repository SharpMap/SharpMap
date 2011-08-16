using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using ProjNet.CoordinateSystems.Transformations;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Drawing.Drawing2D;
using SharpMap.Geometries;
using System.Threading;
using BruTile;

namespace SharpMap.Rendering.Decoration
{
    /// <summary>
    /// Displays Disclaimer-information as on maps.google.com
    /// 
    /// To be used when using Google Maps as BaseMap via for example BruTile
    /// Calls ViewPortInfo Service at maps.google.com to get information about DisclaimerText
    /// 
    /// Supports Async-Mode (use special .ctor)
    /// </summary>
    public class GoogleMapsDisclaimer : MapDecoration
    {
        public enum MapType { Map = 0, Images=2, Labels = 1, Hybrid = 3 };
        MapType m_MapType = MapType.Map;
        IMathTransform m_MathTransform = null;
        string m_Language = "us-EN";
        EventHandler m_DownloadComplete = null;
        bool m_RunAsync = false;
        ITileSource m_TileSource = null;

        /// <summary>
        /// Initialize with custom parameters
        /// </summary>
        /// <param name="mapToWgs84Transform">Transformation to transform MapCoordinates to WGS84</param>
        /// <param name="mapType">Type of Map Displayed</param>
        /// <param name="disclaimerDownloaded">Optional EventHandler that is called after Disclaimer Async Download (to be used to refresh map)</param>
        /// <param name="downloadAsync">wether to download disclaimer information async (non blocking operation)</param>
        public GoogleMapsDisclaimer(IMathTransform mapToWgs84Transform, MapType mapType, EventHandler disclaimerDownloaded, bool downloadAsync) : this()
        {
            m_MathTransform = mapToWgs84Transform;
            m_RunAsync = downloadAsync;
            m_DownloadComplete = disclaimerDownloaded;
            m_MapType = mapType;
            m_TileSource = new BruTile.Web.GoogleTileSource(BruTile.Web.GoogleMapType.GoogleMap);
        }

        /// <summary>
        /// New instance of Google Maps Disclaimer
        /// </summary>
        public GoogleMapsDisclaimer()
        {
            m_MathTransform = null; //Assuming WGS84
            m_Font = new Font("Arial", (float)Math.Floor((11.0 * 72 / 96)));
            m_Language = System.Threading.Thread.CurrentThread.CurrentCulture.Name;
        }

        Regex rex = new Regex("resp && resp\\( \\[\"(?<text>.*?)\",");
        string m_DisclaymerText = "";
        Font m_Font = new Font("Arial",12f);

        protected override Size InternalSize(Graphics g, Map map)
        {
            RequestDisclaimer(map);

            var s = g.MeasureString(m_DisclaymerText, m_Font);
            return new Size((int)Math.Ceiling(s.Width), (int)Math.Ceiling(s.Height));
        }


        protected override void OnRender(System.Drawing.Graphics g, Map map)
        {
            RequestDisclaimer(map);

            var layoutRectangle = g.ClipBounds;
            Color textColor = m_MapType == MapType.Map ? Color.Black : Color.White;
            var b = new SolidBrush(OpacityColor(textColor));
                g.DrawString(m_DisclaymerText, m_Font, b, layoutRectangle);

            //base.OnRender(g, map);
        }

        BoundingBox m_CurDisclaymerRect = null;
        private void RequestDisclaimer(Map map)
        {
            if (m_CurDisclaymerRect != null && m_CurDisclaymerRect.Equals(map.Envelope))
            {
            }
            else
            {
                int level = BruTile.Utilities.GetNearestLevel(m_TileSource.Schema.Resolutions, map.PixelSize);
                double[] ul = m_MathTransform != null ?
                    m_MathTransform.Transform(new double[] { map.Envelope.Left, map.Envelope.Top }) : new double[] { map.Envelope.Left, map.Envelope.Top };

                double[] lr = m_MathTransform != null ?
                    m_MathTransform.Transform(new double[] { map.Envelope.Right, map.Envelope.Bottom }) : new double[] { map.Envelope.Right, map.Envelope.Bottom };

                if (m_RunAsync)
                {
                    DownloadDisclaimerAsync(ul, lr, level);
                }
                else
                {
                    DownloadDisclaimer(ul, lr, level);
                }

                m_CurDisclaymerRect = map.Envelope;
            }
        }

        private void DownloadDisclaimerAsync(double[] ul, double[] lr, int level)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(delegate
            {
                DownloadDisclaimer(ul, lr, level);
                if (m_DownloadComplete != null)
                {
                    m_DownloadComplete(null, new EventArgs());
                }
            }));
        }

        private void DownloadDisclaimer(double[] ul, double[] lr, int level)
        {
            WebRequest rq = HttpWebRequest.Create(string.Format(CultureInfo.InvariantCulture, "http://maps.googleapis.com/maps/api/js/ViewportInfoService.GetViewportInfo?1m6&1m2&1d{0}&2d{1}&2m2&1d{2}&2d{3}&2u{7}&4s{4}&5e{6}&callback=resp&token={5}",
                lr[1], ul[0], ul[1], lr[0], m_Language, 12345, (int)m_MapType, level));
            string jSon = new StreamReader(rq.GetResponse().GetResponseStream()).ReadToEnd();

            if (rex.IsMatch(jSon))
            {
                Match m = rex.Match(jSon);
                if (m.Groups["text"].Success)
                {
                    m_DisclaymerText = m.Groups["text"].Value;
                }
            }
        }

       
    }
}
