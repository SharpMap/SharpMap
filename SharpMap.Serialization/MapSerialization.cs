using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Net;
using System.Xml.Serialization;
using SharpMap.Serialization.Model;

namespace SharpMap.Serialization
{
    public static class MapSerialization
    {
        /// <summary>
        /// Parses a Map from a MapDocument
        /// </summary>
        /// <param name="s">Instance of <see cref="Stream"/></param>
        /// <returns>Map Document accodring to the MapDocument</returns>
        public static Map LoadMapFromStream(Stream s)
        {
            XmlSerializer ser = new XmlSerializer(typeof(MapDefinition));
            MapDefinition md = (MapDefinition)ser.Deserialize(s);

            Map m = new Map();

            if (md.Extent != null)
                m.ZoomToBox(new GeoAPI.Geometries.Envelope(md.Extent.Xmin, md.Extent.Xmax, md.Extent.Ymin, md.Extent.Ymax));

            if (!string.IsNullOrEmpty(md.BackGroundColor))
            {
                m.BackColor = ColorTranslator.FromHtml(md.BackGroundColor);
            }

            m.SRID = md.SRID;

            foreach (var l in md.Layers)
            {
                SharpMap.Layers.ILayer lay = null;
                //WMSLayer?
                if (l is WmsLayer)
                {
                    ICredentials cred = null;
                    if (!string.IsNullOrEmpty((l as WmsLayer).WmsUser))
                        cred = new NetworkCredential((l as WmsLayer).WmsUser, (l as WmsLayer).WmsPassword);

                    SharpMap.Layers.WmsLayer wmsl = new Layers.WmsLayer(l.Name, (l as WmsLayer).OnlineURL, TimeSpan.MaxValue, WebRequest.DefaultWebProxy, cred);
                    if ((l as WmsLayer).WmsLayers != null)
                    {
                        string[] layers = (l as WmsLayer).WmsLayers.Split(',');
                        foreach (var wl in layers)
                        {
                            wmsl.AddLayer(wl);
                        }
                    }
                    else
                    {
                        wmsl.AddChildLayers(wmsl.RootLayer,true);
                    }
                    lay = wmsl;
                }
                //And some simple tiled layers
                else if (l is OsmLayer)
                {
                    lay = new Layers.TileLayer(new BruTile.Web.OsmTileSource(), l.Name);
                }
                else if (l is GoogleLayer)
                {
                    lay = new Layers.TileLayer(new BruTile.Web.GoogleTileSource(BruTile.Web.GoogleMapType.GoogleMap), l.Name);
                }
                else if (l is GoogleSatLayer)
                {
                    lay = new Layers.TileLayer(new BruTile.Web.GoogleTileSource(BruTile.Web.GoogleMapType.GoogleSatellite), l.Name);
                }
                else if (l is GoogleTerrainLayer)
                {
                    lay = new Layers.TileLayer(new BruTile.Web.GoogleTileSource(BruTile.Web.GoogleMapType.GoogleTerrain), l.Name);
                }

                if (lay != null)
                {
                    lay.MinVisible = l.MinVisible;
                    lay.MaxVisible = l.MaxVisible;
                    m.Layers.Add(lay);
                }
                
            }


            return m;
        }

        public static void SaveMapToStream(Map m, Stream s)
        {
            MapDefinition md = new MapDefinition();
            md.Extent = new Extent()
            {
                Xmin = m.Envelope.MinX,
                Xmax = m.Envelope.MaxX,
                Ymin = m.Envelope.MinY,
                Ymax = m.Envelope.MaxY
            };

            md.BackGroundColor = ColorTranslator.ToHtml(m.BackColor);
            md.SRID = m.SRID;

            List<MapLayer> layers = new List<MapLayer>();
            foreach (var layer in m.Layers)
            {
                MapLayer ml = null;
                if (layer is SharpMap.Layers.VectorLayer)
                {

                }
                else if (layer is SharpMap.Layers.WmsLayer)
                {
                    WmsLayer sl = new WmsLayer();
                    sl.OnlineURL = (layer as SharpMap.Layers.WmsLayer).CapabilitiesUrl;
                    sl.WmsLayers = string.Join(",", (layer as SharpMap.Layers.WmsLayer).LayerList.ToArray());
                    if ((layer as SharpMap.Layers.WmsLayer).Credentials is NetworkCredential)
                    {
                        sl.WmsUser = ((layer as SharpMap.Layers.WmsLayer).Credentials as NetworkCredential).UserName;
                        sl.WmsPassword = ((layer as SharpMap.Layers.WmsLayer).Credentials as NetworkCredential).Password;
                    }
                    ml = sl;
                }

                ml.MinVisible = layer.MinVisible;
                ml.MaxVisible = layer.MaxVisible;
                ml.Name = layer.LayerName;

                if (ml != null)
                    layers.Add(ml);
            }

            md.Layers = layers.ToArray();

            XmlSerializer serializer = new XmlSerializer(typeof(MapDefinition));
            serializer.Serialize(s, md);
        }
    }
}
