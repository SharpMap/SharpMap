using System;
using System.Collections.Generic;
using System.Net;
#if !DotSpatialProjections
using System.Runtime.Serialization;
using IMathTransform = ProjNet.CoordinateSystems.Transformations.MathTransform;
#else
using DotSpatial.Projections;
using IMathTransform = DotSpatial.Projections.ProjectionInfo;
#endif
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Drawing;
using GeoAPI.Geometries;
using System.Threading;
using BruTile;

namespace SharpMap.Rendering.Decoration
{
    
    /// <summary>
    /// Displays Disclaimer-information as on maps.google.com
    /// </summary>
    /// <remarks>
    /// To be used when using Google Maps as BaseMap via for example BruTile
    /// Calls ViewPortInfo Service at maps.google.com to get information about DisclaimerText
    /// <para/>
    /// Supports Async-Mode (use special .ctor)
    /// </remarks>
    [Serializable]
    public class GoogleMapsDisclaimer : MapDecoration
    {
        // Logging
        private static readonly Common.Logging.ILog Log = Common.Logging.LogManager.GetCurrentClassLogger();
        
        /// <summary>
        /// Google map type enum
        /// </summary>
        public enum MapType
        {
            /// <summary>
            /// Map
            /// </summary>
            Map = 0,

            /// <summary>
            /// Images
            /// </summary>
            Images = 2,

            /// <summary>
            /// Hybrid
            /// </summary>
            Hybrid = 3
        };

        readonly MapType _mapType = MapType.Map;
        readonly IMathTransform _mathTransform;
#if DotSpatialProjections
        private readonly IMathTransform _to = KnownCoordinateSystems.Geographic.World.WGS1984;
#endif
        //string _language = "us-EN";
        readonly EventHandler _downloadCompleteHandler;
        readonly bool _runInRunAsyncMode;
        readonly ITileSource _tileSource;

        private Envelope _currentDisclaimerEnvelope;

        private const string MapPrefix = "Mapdata ©";
        private const string SatellitePrefix = "Images ©";

        /// <summary>
        /// Initialize with custom parameters
        /// </summary>
        /// <remarks>
        /// IMPORTANT: In Async mode you need to use UpdateBoundingBox when the MapBox/MapImage center or ZoomLevel changes, else the text will be wrong
        /// </remarks>
        /// <param name="mapToWgs84Transform">Transformation to transform MapCoordinates to WGS84</param>
        /// <param name="mapType">Type of Map Displayed</param>
        /// <param name="disclaimerDownloaded">Optional EventHandler that is called after Disclaimer Async Download (to be used to refresh map)</param>
        /// <param name="runInAsyncMode">whether to download disclaimer information async (non blocking operation)</param>
        public GoogleMapsDisclaimer(IMathTransform mapToWgs84Transform, MapType mapType, EventHandler disclaimerDownloaded, bool runInAsyncMode) : this()
        {
            _mathTransform = mapToWgs84Transform;
            _runInRunAsyncMode = runInAsyncMode;
            _downloadCompleteHandler = disclaimerDownloaded;
            _mapType = mapType;
        }

        /// <summary>
        /// New instance of Google Maps Disclaimer
        /// </summary>
        public GoogleMapsDisclaimer()
        {
            _mathTransform = null; //Assuming WGS84
            _font = new Font("Arial", (float)Math.Floor((11.0 * 72 / 96)));
            //_language = Thread.CurrentThread.CurrentCulture.Name;
            _tileSource = new BruTile.Web.GoogleTileSource(BruTile.Web.GoogleMapType.GoogleMap);
        }

        //Regex rex = new Regex("resp && resp\\( \\[\"(?<text>.*?)\",");
        readonly Regex _rex = new Regex("GAddCopyright\\(\"(?<type>.*?)\",\".*?\",(?<miny>[0-9\\.-]+),(?<minx>[0-9\\.-]+),(?<maxy>[0-9\\.-]+),(?<maxx>[0-9\\.-]+),(?<minlevel>\\d+),\"(?<txt>.*?)\",(?<maxlevel>\\d+),.*?\\);", RegexOptions.Singleline);
        string _disclaimerText = "";
        readonly Font _font = new Font("Arial",12f);

        /// <summary>
        /// Function to compute the required size for rendering the map decoration object
        /// <para>This is just the size of the decoration object, border settings are excluded</para>
        /// </summary>
        /// <param name="g"></param>
        /// <param name="map"></param>
        /// <returns>The</returns>
        protected override Size InternalSize(Graphics g, Map map)
        {
            RequestDisclaimer(map);

            var s = g.MeasureString(_disclaimerText, _font);
            return new Size((int)Math.Ceiling(s.Width), (int)Math.Ceiling(s.Height));
        }


        /// <summary>
        /// Function to render the actual map decoration
        /// </summary>
        /// <param name="g"></param>
        /// <param name="map"></param>
        protected override void OnRender(Graphics g, Map map)
        {
            RequestDisclaimer(map);

            var layoutRectangle = g.ClipBounds;
            var textColor = _mapType == MapType.Map 
                ? Color.Black 
                : Color.White;
            
            var b = new SolidBrush(OpacityColor(textColor));
                g.DrawString(_disclaimerText, _font, b, layoutRectangle);
        }
        
        /// <summary>
        /// Method to update the map's view when the disclaimer was retrieved in async mode
        /// </summary>
        /// <param name="map">The map</param>
        public void UpdateBoundingBox(Map map)
        {
            var level = BruTile.Utilities.GetNearestLevel(_tileSource.Schema.Resolutions, map.PixelSize);

            double[] ul, lr;
            GetCorners(map, out ul, out lr);

            if (_runInRunAsyncMode)
            {
                DownloadDisclaimerAsync(ul, lr, level);
            }
            else
            {
                DownloadDisclaimer(ul, lr, level);
            }
        }

        private void GetCorners(Map map, out double[] lr, out double[] ul)
        {
#if !DotSpatialProjections
            ul = _mathTransform != null
                         ? _mathTransform.Transform(new[] {map.Envelope.MinX, map.Envelope.MaxY})
                         : new[] {map.Envelope.MinX, map.Envelope.MaxY};
            lr = _mathTransform != null
                              ? _mathTransform.Transform(new[] {map.Envelope.MaxX, map.Envelope.MinY})
                              : new[] {map.Envelope.MaxX, map.Envelope.MinY};
#else
            ul = new[] { map.Envelope.Left(), map.Envelope.Top() };
            if (_mathTransform != null)
                Reproject.ReprojectPoints(ul, null, _mathTransform, _to, 0, 1);
            lr = new[] {map.Envelope.Right(), map.Envelope.Bottom()};
            if (_mathTransform != null)
                Reproject.ReprojectPoints(lr, null, _mathTransform, _to, 0, 1);
#endif
        }

        private void RequestDisclaimer(Map map)
        {
            if (_currentDisclaimerEnvelope == null || !_currentDisclaimerEnvelope.Equals(map.Envelope))
            {
                double[] ul, lr;
                GetCorners(map, out ul, out lr);
                var level = BruTile.Utilities.GetNearestLevel(_tileSource.Schema.Resolutions, map.PixelSize);

                /*
                 * Download only when run in Sync mode, 
                 * else rely on setting SetBoundingBox 
                 * (else we will flood google with requests 
                 * during panning
                 */
                if (!_runInRunAsyncMode)
                {
                    DownloadDisclaimer(ul, lr, level);
                }

                _currentDisclaimerEnvelope = map.Envelope;
            }
        }

        private void DownloadDisclaimerAsync(double[] ul, double[] lr, int level)
        {
            ThreadPool.QueueUserWorkItem(delegate
                {
                    DownloadDisclaimer(ul, lr, level);
                    if (_downloadCompleteHandler != null)
                    {
                        _downloadCompleteHandler(null, new EventArgs());
                    }
                });
        }

        private void DownloadDisclaimer(double[] ul, double[] lr, int level)
        {
            try
            {
                //level = m_TileSource.Schema.Resolutions.Count - level;
                string mapType = "";
                if (_mapType == MapType.Images)
                    mapType = "k";
                else if (_mapType == MapType.Hybrid)
                {
                    mapType = "h";
                }
               
                //string url = string.Format(CultureInfo.InvariantCulture, "http://maps.googleapis.com/maps/api/js/ViewportInfoService.GetViewportInfo?1m6&1m2&1d{0}&2d{1}&2m2&1d{2}&2d{3}&2u{7}&4s{4}&5e{6}&callback=resp&token={5}", lr[1], ul[0], ul[1], lr[0], m_Language, 12345, (int)m_MapType, level);
                var url = string.Format(CultureInfo.InvariantCulture, "http://maps.google.com/maps/vp?spn={0},{1}&t={5}&z={2}&key=&mapclient=jsapi&vp={3},{4}&ev=mk",
                    ul[1] - lr[1], lr[0] - ul[0], level, (lr[1] + ul[1]) / 2.0, (ul[0] + lr[0]) / 2.0, mapType);

                var rq = (HttpWebRequest)WebRequest.Create(url);
                rq.Referer = "http://localhost";
                rq.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.1 (KHTML, like Gecko) Chrome/13.0.782.112 Safari/535.1";

                string json;
                using (var s = rq.GetResponse().GetResponseStream())
                {
                    if (s == null)
                        throw new WebException("Failed to get web response stream");
                    json = new StreamReader(s).ReadToEnd();
                }

                var mstrs = new List<string>();
                var kstrs = new List<string>();

                var bbox = new Envelope(new Coordinate(ul[0],lr[1]),new Coordinate(lr[0],ul[1]));
                foreach (Match m in _rex.Matches(json))
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



                        if (bbox.Intersects(new Envelope(minx, maxx, miny, maxy)))
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
                if (_mapType == MapType.Map || _mapType == MapType.Hybrid)
                {
                    txt = MapPrefix + " " + string.Join(",", mstrs.ToArray());
                }
                else if ( _mapType == MapType.Images)
                {
                    txt = SatellitePrefix + " " + string.Join(",",kstrs.ToArray());
                }


                if (_mapType == MapType.Hybrid)
                {
                    txt += ", " + SatellitePrefix + " " + string.Join(",", kstrs.ToArray());
                }

                _disclaimerText = txt;
                /*if (rex.IsMatch(jSon))
                {
                    Match m = rex.Match(jSon);
                    if (m.Groups["text"].Success)
                    {
                        m_DisclaymerText = m.Groups["text"].Value;
                    }
                }*/
            }
            catch (Exception ex)
            {
                if (Log.IsDebugEnabled)
                    Log.Debug(ex);
            }
        }

       
    }
}
