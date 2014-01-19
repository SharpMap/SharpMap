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
using System.Linq;

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
            bool transparent = String.Equals(request.GetParam("TRANSPARENT"), "TRUE", Case);
            if (!transparent)
            {
                string bgcolor = request.GetParam("BGCOLOR");
                if (bgcolor != null)
                {
                    try { backColor = ColorTranslator.FromHtml(bgcolor); }
                    catch
                    {
                        throw new WmsInvalidParameterException("Invalid parameter BGCOLOR: " + bgcolor);
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

            Size size = GetSize(@params);

            map.Size = size;
            Envelope bbox = @params.BBOX;
            map.PixelAspectRatio = (size.Width / (double)size.Height) / (bbox.Width / bbox.Height);
            map.Center = bbox.Centre;
            map.Zoom = bbox.Width;

            //set Styles for layerNames
            //first, if the request ==  STYLES=, set all the vectorlayers with Themes not null the Theme to the first theme from Themes
            string ptheme = @params.Styles;
            string players = @params.Layers;
            string cqlFilter = @params.CqlFilter;

            if (!String.IsNullOrEmpty(players))
            {
                string[] layerNames = GetLayerNames(map, players);

                // we have a known set of layerNames in request 
                // so we will set each layer to its default theme
                // and disable the layer, layers will be enabled 
                // later as per the request
                foreach (ILayer layer in map.Layers)
                {
                    VectorLayer vectorLayer = layer as VectorLayer;
                    SetDefaultThemeForLayer(vectorLayer);
                    layer.Enabled = false;
                }

                var themeTable = new Dictionary<string, string>();

                BuildThemeLookup(ptheme, layerNames, themeTable);

                // since we have layerNames specified
                // enable only those layerNames
                for (int j = 0; j < layerNames.Length; j++)
                {
                    var layerName = layerNames[j];
                    var layer = GetMapLayerByName(map, layerName);

                    //Set layer on/off
                    layer.Enabled = true;

                    //check if a styles have been specified for layers
                    if (themeTable.Count > 0)
                    {
                        var themeName = themeTable[layerName];
                        //do nothing if themeName is empty
                        if (!string.IsNullOrEmpty(themeName))
                        {
                            //is this a vector layer at all
                            VectorLayer vectorLayer = layer as VectorLayer;

                            //does it have several themes to choose from
                            //TODO -> Refactor VectorLayer.Themes to Rendering.Thematics.ThemeList : ITheme
                            if (vectorLayer != null && vectorLayer.Themes != null && vectorLayer.Themes.Count > 0)
                            {
                                // we need to a case invariant comparison for themeName
                                var themeDict = new Dictionary<string, ITheme>(vectorLayer.Themes, new StringComparerIgnoreCase());

                                if (themeDict.ContainsKey(themeName))
                                {
                                    vectorLayer.Theme = themeDict[themeName];
                                }
                                else
                                {
                                    throw new WmsStyleNotDefinedException("Style not advertised for this layer");
                                }
                            }
                        }
                    }

                    if (!String.IsNullOrEmpty(cqlFilter))
                    {
                        ApplyCqlFilter(cqlFilter, layer);
                    }
                }

            }
            //Render map
            Image img = map.GetMap();
            return new GetMapResponse(img, imageEncoder);
        }

        private string[] GetLayerNames(Map map, string players)
        {
            string[] layerNames = players.Split(new[] { ',' });
            if (Description.LayerLimit > 0)
            {
                if (layerNames.Length == 0 &&
                    map.Layers.Count > Description.LayerLimit ||
                    layerNames.Length > Description.LayerLimit)
                {
                    throw new WmsOperationNotSupportedException("Too many layerNames requested");
                }
            }
            return layerNames;
        }

        private static void BuildThemeLookup(string pstyle, string[] layerNames, Dictionary<string, string> themeTable)
        {
            if (!String.IsNullOrEmpty(pstyle))
            {
                string[] styleNames = pstyle.Split(new[] { ',' });
                
                //test whether the number of the layers and the styles specified are equal. 
                //WMS spec is unclear on what to do if there is no one-to-one correspondence
                if (layerNames.Length == styleNames.Length)
                {
                    for (var i = 0; i < layerNames.Length; i++)
                    {
                        themeTable.Add(layerNames[i], styleNames[i]);
                    }
                }
            }
        }



        private Size GetSize(WmsParams @params)
        {

            if (Description.MaxWidth > 0 && @params.Width > Description.MaxWidth)
            {
                throw new WmsOperationNotSupportedException("Parameter WIDTH too large");
            }

            if (Description.MaxHeight > 0 && @params.Height > Description.MaxHeight)
            {
                throw new WmsOperationNotSupportedException("Parameter HEIGHT too large");
            }

            return new Size(@params.Width, @params.Height);
        }

        private ILayer GetMapLayerByName(Map map, string layerName)
        {
            for (int i = 0; i < map.Layers.Count; i++)
            {
                // this function should be an  map.GetLayerByName
                if (String.Equals(map.Layers[i].LayerName, layerName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return map.Layers[i];
                }
            }
            throw new WmsLayerNotDefinedException(layerName);
        }

        private void ApplyCqlFilter(string cqlFilter, ILayer layer)
        {
            VectorLayer vectorLayer = layer as VectorLayer;
            if (vectorLayer != null)
            {
                PrepareDataSourceForCql(vectorLayer.DataSource, cqlFilter);
            }

            LabelLayer labelLayer = layer as LabelLayer;
            if (labelLayer != null)
            {
                PrepareDataSourceForCql(labelLayer.DataSource, cqlFilter);
            }
        }

        private void SetDefaultThemeForLayer(VectorLayer vectorLayer)
        {
            if (vectorLayer != null && vectorLayer.Themes != null && vectorLayer.Themes.Count > 0)
            {
                // we assume that the first theme in the themes list is the default theme
                foreach (KeyValuePair<string, ITheme> kvp in vectorLayer.Themes)
                {
                    vectorLayer.Theme = kvp.Value;
                    break;
                }
            }
        }


        private void PrepareDataSourceForCql(IProvider provider, string cqlFilterString)
        {
            //for layerNames with a filterprovider
            FilterProvider filterProvider = provider as FilterProvider;
            if (filterProvider != null)
            {
                filterProvider.FilterDelegate = row => CqlFilter(row, cqlFilterString);
                return;
            }
            //for layerNames with a SQL datasource with a DefinitionQuery property
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
            throw new WmsInvalidParameterException("Invalid MimeType specified in FORMAT parameter");
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
