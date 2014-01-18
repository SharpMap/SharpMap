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
using SharpMap.Web.Wms.Exceptions;

namespace SharpMap.Web.Wms.Server.Handlers
{
    public class GetMap : AbstractHandler
    {
        public GetMap(Capabilities.WmsServiceDescription description) :
            base(description) { }

        protected override WmsParams ValidateParams(IContextRequest request, int targetSrid)
        {
            WmsParams @params = ValidateCommons(request, targetSrid);

            // Code specific for GetMap
            Color backColor;
            bool transparent = String.Equals(request.Params["TRANSPARENT"], "TRUE", Case);
            if (!transparent)
            {
                string bgcolor = request.Params["BGCOLOR"];
                if (bgcolor != null)
                {
                    try { backColor = ColorTranslator.FromHtml(bgcolor); }
                    catch
                    {
                        throw new WmsInvalidParameterException("Invalid parameter BGCOLOR: " + bgcolor);
                        //return WmsParams.Failure("Invalid parameter BGCOLOR: " + bgcolor);
                    }
                }
                else backColor = Color.White;
            }
            else backColor = Color.Transparent;
            @params.BackColor = backColor;
            return @params;
        }

        public override IHandlerResponse Handle(Map map, IContextRequest request)
        {
            WmsParams @params = ValidateParams(request, TargetSrid(map));
            
            map.BackColor = @params.BackColor;

            //Get the image format requested
            ImageCodecInfo imageEncoder = GetEncoderInfo(@params.Format);
            if (imageEncoder == null)
            {
                throw new WmsInvalidParameterException("Invalid MimeType specified in FORMAT parameter");
            }

            int width = @params.Width;
            if (Description.MaxWidth > 0 && width > Description.MaxWidth)
            {
                throw new WmsOperationNotSupportedException("Parameter WIDTH too large");
            }
            int height = @params.Height;
            if (Description.MaxHeight > 0 && height > Description.MaxHeight)
            {
                throw new WmsOperationNotSupportedException("Parameter HEIGHT too large");
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
                                                throw new WmsStyleNotDefinedException("Style not advertised for this layer");
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
                        throw new WmsOperationNotSupportedException("Too many layers requested");
                    }
                }

                foreach (ILayer layer in map.Layers)
                    layer.Enabled = false;
                foreach (string layer in layers)
                {
                    //SharpMap.Layers.ILayer lay = map.Layers.Find(delegate(SharpMap.Layers.ILayer findlay) { return findlay.message == layer; });
                    ILayer lay = null;
                    for (int i = 0; i < map.Layers.Count; i++)
                        if (String.Equals(map.Layers[i].LayerName, layer,
                            StringComparison.InvariantCultureIgnoreCase))
                            lay = map.Layers[i];


                    if (lay == null)
                    {
                        throw new WmsLayerNotDefinedException(layer);
                    }
                    lay.Enabled = true;
                }
            }

            //Render map
            Image img = map.GetMap();
            return new GetMapResponse(img, imageEncoder);
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

    public class GetMapResponse : IHandlerResponse
    {
        private readonly Image _image;
        private readonly ImageCodecInfo _codecInfo;

        public GetMapResponse(Image image, ImageCodecInfo codecInfo)
        {
            if (image == null)
                throw new ArgumentNullException("image");
            if (codecInfo == null)
                throw new ArgumentNullException("codecInfo");
            _image = image;
            _codecInfo = codecInfo;
        }

        public void WriteToContextAndFlush(IContextResponse response)
        {
            //Png can't stream directy. Going through a memorystream instead
            byte[] buffer;
            using (MemoryStream ms = new MemoryStream())
            {
                _image.Save(ms, _codecInfo, null);
                _image.Dispose();
                buffer = ms.ToArray();
            }
            response.Clear();
            response.ContentType = _codecInfo.MimeType;
            response.Write(buffer);
            response.End();
        }
    }
}
