using System;
using System.Collections.Generic;
using System.Net;
#if !DotSpatialProjections
using IMathTransform = ProjNet.CoordinateSystems.Transformations.MathTransform;
#else
using DotSpatial.Projections;
using IMathTransform = DotSpatial.Projections.ProjectionInfo;
#endif
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Drawing;
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
        public enum MapType { Map = 0, Images=2, Hybrid = 3 };

        readonly MapType m_MapType = MapType.Map;
        IMathTransform m_MathTransform = null;
#if DotSpatialProjections
        private readonly IMathTransform _to = KnownCoordinateSystems.Geographic.World.WGS1984;
#endif
        string m_Language = "us-EN";
        EventHandler m_DownloadComplete = null;
        readonly bool m_RunAsync = false;
        ITileSource m_TileSource = null;

        string m_MapPrefix = "Mapdata ©";
        string m_SatPrefix = "Images ©";

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
        }

        /// <summary>
        /// New instance of Google Maps Disclaimer
        /// </summary>
        public GoogleMapsDisclaimer()
        {
            m_MathTransform = null; //Assuming WGS84
            m_Font = new Font("Arial", (float)Math.Floor((11.0 * 72 / 96)));
            m_Language = System.Threading.Thread.CurrentThread.CurrentCulture.Name;
            m_TileSource = new BruTile.Web.GoogleTileSource(BruTile.Web.GoogleMapType.GoogleMap);
        }

        //Regex rex = new Regex("resp && resp\\( \\[\"(?<text>.*?)\",");
        readonly Regex _rex = new Regex("GAddCopyright\\(\"(?<type>.*?)\",\".*?\",(?<miny>[0-9\\.-]+),(?<minx>[0-9\\.-]+),(?<maxy>[0-9\\.-]+),(?<maxx>[0-9\\.-]+),(?<minlevel>\\d+),\"(?<txt>.*?)\",(?<maxlevel>\\d+),.*?\\);", RegexOptions.Singleline);
        string m_DisclaymerText = "";
        readonly Font m_Font = new Font("Arial",12f);

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
#if !DotSpatialProjections
                double[] ul = m_MathTransform != null ?
                    m_MathTransform.Transform(new double[] { map.Envelope.Left, map.Envelope.Top }) : new double[] { map.Envelope.Left, map.Envelope.Top };
#else
                var ul = new[] { map.Envelope.Left, map.Envelope.Top };
                if (m_MathTransform != null)
                    Reproject.ReprojectPoints(ul, null, m_MathTransform, _to, 0, 1);
#endif

#if !DotSpatialProjections
                double[] lr = m_MathTransform != null ?
                    m_MathTransform.Transform(new double[] { map.Envelope.Right, map.Envelope.Bottom }) : new double[] { map.Envelope.Right, map.Envelope.Bottom };
#else
                var lr = new[] {map.Envelope.Right, map.Envelope.Bottom};
                if (m_MathTransform != null)
                    Reproject.ReprojectPoints(lr, null, m_MathTransform, _to, 0, 1);
#endif


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
            try
            {
                //level = m_TileSource.Schema.Resolutions.Count - level;
                string mapType = "";
                if (m_MapType == MapType.Images)
                    mapType = "k";
                else if (m_MapType == MapType.Hybrid)
                {
                    mapType = "h";
                }
               
                //string url = string.Format(CultureInfo.InvariantCulture, "http://maps.googleapis.com/maps/api/js/ViewportInfoService.GetViewportInfo?1m6&1m2&1d{0}&2d{1}&2m2&1d{2}&2d{3}&2u{7}&4s{4}&5e{6}&callback=resp&token={5}", lr[1], ul[0], ul[1], lr[0], m_Language, 12345, (int)m_MapType, level);
                string url = string.Format(CultureInfo.InvariantCulture, "http://maps.google.com/maps/vp?spn={0},{1}&t={5}&z={2}&key=&mapclient=jsapi&vp={3},{4}&ev=mk",
                    ul[1] - lr[1], lr[0] - ul[0], level, (lr[1] + ul[1]) / 2.0, (ul[0] + lr[0]) / 2.0, mapType);
                WebRequest rq = HttpWebRequest.Create(url);
                (rq as HttpWebRequest).Referer = "http://localhost";
                (rq as HttpWebRequest).UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.1 (KHTML, like Gecko) Chrome/13.0.782.112 Safari/535.1";
                string jSon = new StreamReader(rq.GetResponse().GetResponseStream()).ReadToEnd();

                List<string> mstrs = new List<string>();
                List<string> kstrs = new List<string>();

                BoundingBox bbox = new BoundingBox(ul[0],lr[1],lr[0],ul[1]);
                foreach (Match m in _rex.Matches(jSon))
                {
                    if (m.Groups["txt"].Success && !string.IsNullOrEmpty(m.Groups["txt"].Value))
                    {
                        int minLevel = int.Parse(m.Groups["minlevel"].Value);
                        int maxLevel = int.Parse(m.Groups["maxlevel"].Value);
                        if (level < minLevel || level > maxLevel)
                            continue;
                        double minx = double.Parse(m.Groups["minx"].Value, CultureInfo.InvariantCulture);
                        double miny = double.Parse(m.Groups["miny"].Value, CultureInfo.InvariantCulture);
                        double maxx = double.Parse(m.Groups["maxx"].Value, CultureInfo.InvariantCulture);
                        double maxy = double.Parse(m.Groups["maxy"].Value, CultureInfo.InvariantCulture);


                        
                        if (bbox.Intersects(new BoundingBox(minx, miny, maxx, maxy)))
                        {

                            if (m.Groups["type"].Value == "m")
                            {
                                if (!mstrs.Contains(m.Groups["txt"].Value))
                                    mstrs.Add(m.Groups["txt"].Value);
                            }
                            else
                            {
                                if (!kstrs.Contains(m.Groups["txt"].Value))
                                {
                                kstrs.Add(m.Groups["txt"].Value);
                                }
                            }
                        }
                    }
                }

                string txt = "";
                if (m_MapType == MapType.Map || m_MapType == MapType.Hybrid)
                {
                    txt = m_MapPrefix + " " + string.Join(",", mstrs.ToArray());
                }
                else if ( m_MapType == MapType.Images)
                {
                    txt = m_SatPrefix + " " + string.Join(",",kstrs.ToArray());
                }


                if (m_MapType == MapType.Hybrid)
                {
                    txt += ", " + m_SatPrefix + " " + string.Join(",", kstrs.ToArray());
                }

                m_DisclaymerText = txt;
                /*if (rex.IsMatch(jSon))
                {
                    Match m = rex.Match(jSon);
                    if (m.Groups["text"].Success)
                    {
                        m_DisclaymerText = m.Groups["text"].Value;
                    }
                }*/
            }
            catch
            {
            }
        }

       
    }
}
