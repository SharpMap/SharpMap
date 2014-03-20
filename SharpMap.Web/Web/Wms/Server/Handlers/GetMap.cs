using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
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

            // code specific for GetMap
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
                        string s = String.Format("Invalid parameter BGCOLOR: {0}", bgcolor);
                        throw new WmsInvalidParameterException(s);
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

            // get the image format requested
            ImageCodecInfo imageEncoder = GetEncoderInfo(@params.Format);

            Size size = GetSize(@params);
            map.Size = size;
            Envelope bbox = @params.BBOX;
            double sizeRatio = size.Width / (double)size.Height;
            double bboxRatio = bbox.Width / bbox.Height;
            map.PixelAspectRatio = sizeRatio / bboxRatio;
            map.Center = bbox.Centre;
            map.Zoom = bbox.Width;

            // set Styles for layerNames
            // first, if the request ==  STYLES=, set all the vectorlayers with Themes not null the Theme to the first theme from Themes
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

                Dictionary<string, string> themeTable = new Dictionary<string, string>();
                BuildThemeLookup(ptheme, layerNames, themeTable);

                // since we have layerNames specified
                // enable only those layerNames
                for (int j = 0; j < layerNames.Length; j++)
                {
                    string layerName = layerNames[j];
                    ILayer layer = GetMapLayerByName(map, layerName);

                    // set layer on/off
                    layer.Enabled = true;

                    // check if a styles have been specified for layers
                    if (themeTable.Count > 0)
                    {
                        string themeName = themeTable[layerName];
                        // do nothing if themeName is empty
                        if (!string.IsNullOrEmpty(themeName))
                        {
                            // is this a vector layer at all
                            VectorLayer vectorLayer = layer as VectorLayer;

                            // does it have several themes to choose from
                            // TODO -> Refactor VectorLayer.Themes to Rendering.Thematics.ThemeList : ITheme
                            if (vectorLayer != null && vectorLayer.Themes != null && vectorLayer.Themes.Count > 0)
                            {
                                // we need to a case invariant comparison for themeName
                                Dictionary<string, ITheme> themeDict = new Dictionary<string, ITheme>(vectorLayer.Themes, new StringComparerIgnoreCase());

                                if (!themeDict.ContainsKey(themeName))
                                    throw new WmsStyleNotDefinedException("Style not advertised for this layer");
                                vectorLayer.Theme = themeDict[themeName];
                            }
                        }
                    }

                    if (!String.IsNullOrEmpty(cqlFilter))
                        ApplyCqlFilter(cqlFilter, layer);
                }

            }
            // render map
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
            if (String.IsNullOrEmpty(pstyle)) 
                return;

            string[] styleNames = pstyle.Split(new[] { ',' });
            // test whether the number of the layers and the styles specified are equal. 
            // WMS spec is unclear on what to do if there is no one-to-one correspondence
            if (layerNames.Length == styleNames.Length)
                for (int i = 0; i < layerNames.Length; i++)
                    themeTable.Add(layerNames[i], styleNames[i]);
        }

        private Size GetSize(WmsParams @params)
        {
            if (Description.MaxWidth > 0 && @params.Width > Description.MaxWidth)
                throw new WmsOperationNotSupportedException("Parameter WIDTH too large");
            if (Description.MaxHeight > 0 && @params.Height > Description.MaxHeight)
                throw new WmsOperationNotSupportedException("Parameter HEIGHT too large");
            return new Size(@params.Width, @params.Height);
        }

        private ILayer GetMapLayerByName(Map map, string layerName)
        {
            for (int i = 0; i < map.Layers.Count; i++)
            {
                // this function should be map.GetLayerByName
                if (String.Equals(map.Layers[i].LayerName, layerName))
                    return map.Layers[i];
            }
            throw new WmsLayerNotDefinedException(layerName);
        }

        private void ApplyCqlFilter(string cqlFilter, ILayer layer)
        {
            VectorLayer vectorLayer = layer as VectorLayer;
            if (vectorLayer != null)
                PrepareDataSourceForCql(vectorLayer.DataSource, cqlFilter);

            LabelLayer labelLayer = layer as LabelLayer;
            if (labelLayer != null)
                PrepareDataSourceForCql(labelLayer.DataSource, cqlFilter);
        }

        private void SetDefaultThemeForLayer(VectorLayer vectorLayer)
        {
            if (vectorLayer == null || vectorLayer.Themes == null || vectorLayer.Themes.Count <= 0) 
                return;
            // we assume that the first theme in the themes list is the default theme
            foreach (KeyValuePair<string, ITheme> kvp in vectorLayer.Themes)
            {
                vectorLayer.Theme = kvp.Value;
                break;
            }
        }


        private void PrepareDataSourceForCql(IProvider provider, string cqlFilterString)
        {
            // for layerNames with a filterprovider
            FilterProvider filterProvider = provider as FilterProvider;
            if (filterProvider != null)
            {
                filterProvider.FilterDelegate = row => CqlFilter(row, cqlFilterString);
                return;
            }
            // for layerNames with a SQL datasource with a DefinitionQuery property
            PropertyInfo piDefinitionQuery = provider.GetType().GetProperty("DefinitionQuery", BindingFlags.Public | BindingFlags.Instance);
            if (piDefinitionQuery != null)
                piDefinitionQuery.SetValue(provider, cqlFilterString, null);
        }

        // used for setting up output format of image file
        private ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            foreach (ImageCodecInfo encoder in ImageCodecInfo.GetImageEncoders())
                if (encoder.MimeType == mimeType)
                    return encoder;
            throw new WmsInvalidParameterException("Invalid MimeType specified in FORMAT parameter");
        }
    }
}
