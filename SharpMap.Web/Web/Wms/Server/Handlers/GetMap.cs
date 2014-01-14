using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using GeoAPI.Geometries;
using SharpMap.Data.Providers;
using SharpMap.Layers;
using SharpMap.Rendering.Thematics;

namespace SharpMap.Web.Wms.Server.Handlers
{
    public class GetMap : AbstractHandler
    {
        public GetMap(Capabilities.WmsServiceDescription description) : 
            base(description) { }

        protected override WmsParams ValidateParams(IContext context, int targetSrid)
        {
            WmsParams @params = ValidateCommons(context, targetSrid);
            if (!@params.IsValid)
                return @params;

            // Code specific for GetMap
            Color backColor;
            bool transparent = String.Equals(context.Params["TRANSPARENT"], "TRUE", Case);            
            if (!transparent)
            {
                string bgcolor = context.Params["BGCOLOR"];
                if (bgcolor != null)
                {
                    try { backColor = ColorTranslator.FromHtml(bgcolor); }
                    catch
                    {
                        return WmsParams.Failure("Invalid parameter BGCOLOR: " + bgcolor);
                    }
                }
                else backColor = Color.White;
            }
            else backColor = Color.Transparent;
            @params.BackColor = backColor;            
            return @params;
        }

        public override void Handle(Map map, IContext context)
        {
            WmsParams @params = ValidateParams(context, TargetSrid(map));
            if (!@params.IsValid)
            {
                WmsException.ThrowWmsException(@params.ErrorCode, @params.Error, context);
                return;
            }

            map.BackColor = @params.BackColor;

            //Get the image format requested
            ImageCodecInfo imageEncoder = GetEncoderInfo(@params.Format);
            if (imageEncoder == null)
            {
                WmsException.ThrowWmsException("Invalid MimeType specified in FORMAT parameter", context);
                return;
            }

            int width = @params.Width;
            if (Description.MaxWidth > 0 && width > Description.MaxWidth)
            {
                WmsException.ThrowWmsException(WmsException.WmsExceptionCode.OperationNotSupported,
                    "Parameter WIDTH too large", context);
                return;
            }
            int height = @params.Height;
            if (Description.MaxHeight > 0 && height > Description.MaxHeight)
            {
                WmsException.ThrowWmsException(WmsException.WmsExceptionCode.OperationNotSupported,
                    "Parameter HEIGHT too large", context);
                return;
            }
            map.Size = new Size(width, height);

            Envelope bbox = @params.BBOX;
            map.PixelAspectRatio = (width / (double)height) / (bbox.Width / bbox.Height);
            map.Center = bbox.Centre;
            map.Zoom = bbox.Width;

            //set Styles for layers
            //first, if the request ==  STYLES=, set all the vectorlayers with Themes not null the Theme to the first theme from Themes
            string pstyle = @params.Styles;
            string players = @params.Layers;
            if (String.IsNullOrEmpty(pstyle))
            {
                foreach (ILayer layer in map.Layers)
                {
                    VectorLayer vectorLayer = layer as VectorLayer;
                    if (vectorLayer != null)
                    {
                        if (vectorLayer.Themes != null)
                        {
                            foreach (KeyValuePair<string, ITheme> kvp in vectorLayer.Themes)
                            {
                                vectorLayer.Theme = kvp.Value;
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                if (!String.IsNullOrEmpty(players))
                {
                    string[] layerz = players.Split(new[] { ',' });
                    string[] styles = pstyle.Split(new[] { ',' });
                    //test whether the lengt of the layers and the styles is the same. WMS spec is unclear on what to do if there is no one-to-one correspondence
                    if (layerz.Length == styles.Length)
                    {
                        foreach (ILayer layer in map.Layers)
                        {
                            //is this a vector layer at all
                            VectorLayer vectorLayer = layer as VectorLayer;
                            if (vectorLayer == null) continue;

                            //does it have several themes applied
                            //TODO -> Refactor VectorLayer.Themes to Rendering.Thematics.ThemeList : ITheme
                            if (vectorLayer.Themes != null && vectorLayer.Themes.Count > 0)
                            {
                                for (int i = 0; i < layerz.Length; i++)
                                {
                                    if (String.Equals(layer.LayerName, layerz[i],
                                        StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        //take default style if style is empty
                                        if (styles[i] == "")
                                        {
                                            foreach (KeyValuePair<string, ITheme> kvp in vectorLayer.Themes)
                                            {
                                                vectorLayer.Theme = kvp.Value;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (vectorLayer.Themes.ContainsKey(styles[i]))
                                            {
                                                vectorLayer.Theme = vectorLayer.Themes[styles[i]];
                                            }
                                            else
                                            {
                                                WmsException.ThrowWmsException(
                                                    WmsException.WmsExceptionCode.StyleNotDefined,
                                                    "Style not advertised for this layer", context);
                                                return;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            string cqlFilter = @params.CqlFilter;
            if (!String.IsNullOrEmpty(cqlFilter))
            {
                foreach (ILayer layer in map.Layers)
                {
                    VectorLayer vectorLayer = layer as VectorLayer;
                    if (vectorLayer != null)
                    {
                        PrepareDataSourceForCql(vectorLayer.DataSource, cqlFilter);
                        continue;
                    }

                    LabelLayer labelLayer = layer as LabelLayer;
                    if (labelLayer != null)
                    {
                        PrepareDataSourceForCql(labelLayer.DataSource, cqlFilter);
                    }
                }
            }

            //Set layers on/off
            string layersString = players;
            if (!String.IsNullOrEmpty(layersString))
            //If LAYERS is empty, use default layer on/off settings
            {
                string[] layers = layersString.Split(new[] { ',' });
                if (Description.LayerLimit > 0)
                {
                    if (layers.Length == 0 && map.Layers.Count > Description.LayerLimit ||
                        layers.Length > Description.LayerLimit)
                    {
                        WmsException.ThrowWmsException(WmsException.WmsExceptionCode.OperationNotSupported,
                            "Too many layers requested", context);
                        return;
                    }
                }

                foreach (ILayer layer in map.Layers)
                    layer.Enabled = false;
                foreach (string layer in layers)
                {
                    //SharpMap.Layers.ILayer lay = map.Layers.Find(delegate(SharpMap.Layers.ILayer findlay) { return findlay.LayerName == layer; });
                    ILayer lay = null;
                    for (int i = 0; i < map.Layers.Count; i++)
                        if (String.Equals(map.Layers[i].LayerName, layer,
                            StringComparison.InvariantCultureIgnoreCase))
                            lay = map.Layers[i];


                    if (lay == null)
                    {
                        WmsException.ThrowWmsException(WmsException.WmsExceptionCode.LayerNotDefined,
                            String.Format("Unknown layer '{0}'", layer), context);
                        return;
                    }
                    lay.Enabled = true;
                }
            }

            //Render map
            Image img = map.GetMap();

            //Png can't stream directy. Going through a memorystream instead
            byte[] buffer;
            using (MemoryStream ms = new MemoryStream())
            {
                img.Save(ms, imageEncoder, null);
                img.Dispose();
                buffer = ms.ToArray();
            }
            context.Clear();
            context.ContentType = imageEncoder.MimeType;
            context.Write(buffer);
            context.End();
        }

        private void PrepareDataSourceForCql(IProvider provider, string cqlFilterString)
        {
            //for layers with a filterprovider
            FilterProvider filterProvider = provider as FilterProvider;
            if (filterProvider != null)
            {
                filterProvider.FilterDelegate = row => CqlFilter(row, cqlFilterString);
                return;
            }
            //for layers with a SQL datasource with a DefinitionQuery property
            PropertyInfo piDefinitionQuery = provider.GetType().GetProperty("DefinitionQuery", BindingFlags.Public | BindingFlags.Instance);
            if (piDefinitionQuery != null)
                piDefinitionQuery.SetValue(provider, cqlFilterString, null);
        }

        /// <summary>
        /// Used for setting up output format of image file
        /// </summary>
        private ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            foreach (ImageCodecInfo encoder in ImageCodecInfo.GetImageEncoders())
                if (encoder.MimeType == mimeType)
                    return encoder;
            return null;
        }
    }
}
