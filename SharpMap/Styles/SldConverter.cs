// Copyright 2009 - John Diss (www.newgrove.com)
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

﻿using System;
using System.Collections.Generic;
using System.Drawing;
﻿using System.Drawing.Drawing2D;
﻿using System.IO;
﻿using System.Linq;
﻿using System.Xml;

namespace SharpMap.Styles
{
    /// <summary>
    /// A conversion class to get a <see cref="VectorStyle"/> from an Styled Layer Descriptor (v1.0) document
    /// </summary>
    public class SldConverter
    {
        /// <summary>
        /// Method to parse the vector styles from a xml text
        /// </summary>
        /// <param name="xmlText">The xml text</param>
        /// <returns>A dictionary of vector styles</returns>
        public static IDictionary<string, VectorStyle> ParseFeatureStyleFromXmlText(string xmlText)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xmlText);
            return ParseFeatureStyle(doc);
        }

        /// <summary>
        /// Method to parse the vector styles from a xml file
        /// </summary>
        /// <param name="filePath">The file path of the xml file</param>
        /// <returns>A dictionary of vector styles</returns>
        public static IDictionary<string, VectorStyle> ParseFeatureStyleFromFile(string filePath)
        {
            var doc = new XmlDocument();
            doc.Load(filePath);
            return ParseFeatureStyle(doc);
        }

        /// <summary>
        /// Method to parse the vector styles from a <see cref="XmlDocument"/>
        /// </summary>
        /// <param name="doc">The xml document</param>
        /// <returns>A dictionary of vector styles</returns>
        public static IDictionary<string, VectorStyle> ParseFeatureStyle(XmlDocument doc)
        {
            var styles = new Dictionary<string, VectorStyle>();

            // Load SLD file
            var nt = new NameTable();
            var nsm = new XmlNamespaceManager(nt);
            nsm.AddNamespace("sld", "http://www.opengis.net/sld");
            nsm.AddNamespace("ogc", "http://www.opengis.net/ogc");
            nsm.AddNamespace("xlink", "http://www.w3.org/1999/xlink");


            var sldConfig = new XmlDocument(nt);
            sldConfig.LoadXml(doc.OuterXml);

            var featureTypeStyleEls = sldConfig.SelectNodes("//sld:FeatureTypeStyle", nsm);
            if (featureTypeStyleEls == null)
                return null;
            
            foreach (XmlElement featTypeStyle in featureTypeStyleEls)
            {
                var el = (XmlElement) featTypeStyle.SelectSingleNode("sld:FeatureTypeName", nsm);
                var mainName = el != null ? el.InnerText : "";
                var rules = featTypeStyle.SelectNodes("sld:Rule", nsm);

                if (rules != null)
                {
                    foreach (XmlElement rule in rules)
                    {
                        el = (XmlElement) rule.SelectSingleNode("sld:Name", nsm);
                        var name = el != null ? el.InnerText : "";
                        var style = new VectorStyle();
                        SetSymbologyForRule(style, rule, nsm);
                        styles.Add(mainName + ":" + name, style);
                    }
                }
            }

            return styles;


            //style.AreFeaturesSelectable
            //style.Enabled
            //style.EnableOutline
            //style.Fill
            //style.HighlightFill
            //style.HighlightLine
            //style.HighlightOutline
            //style.HighlightSymbol
            //style.Line
            //style.MaxVisible
            //style.MinVisible
            //style.Outline
            //style.RenderingMode
            //style.SelectFill
            //style.SelectLine
            //style.SelectOutline
            //style.SelectSymbol
        }

        #region "SetStyle"

        private static void SetFillStyle(VectorStyle style, XmlNode fillSymbolizer, XmlNamespaceManager nsm)
        {
            string stroke = string.Empty;
            string strokeWidth = string.Empty;
            string strokeOpacity = string.Empty;
            string strokeLinejoin = string.Empty;
            string strokeLineCap = string.Empty;
            string strokeDasharray = string.Empty;
            string strokeDashOffset = string.Empty;
            string fill = string.Empty;
            string fillOpacity = string.Empty;
            string pointSymbolPath = string.Empty;

            var polygonFillSymbols = fillSymbolizer.SelectNodes("sld:Fill/sld:CssParameter", nsm);

            if (polygonFillSymbols == null)
                return;

            foreach (XmlElement polygonFillSymbol in polygonFillSymbols)
            {
                if (polygonFillSymbol != null)
                {
                    switch (polygonFillSymbol.GetAttribute("name"))
                    {
                        //polygon
                        case "fill":
                            fill = polygonFillSymbol.InnerXml;
                            break;
                        case "fill-opacity":
                            fillOpacity = polygonFillSymbol.InnerXml;
                            break;
                    }
                }
            }

            SetStyle(style, stroke, strokeWidth, strokeOpacity, strokeLinejoin, strokeLineCap, strokeDasharray,
                strokeDashOffset, fill, fillOpacity, pointSymbolPath);


            //Call down to stroke style
            SetStrokeStyle(style, fillSymbolizer, nsm);
        }

        private static void SetStrokeStyle(VectorStyle style, XmlNode strokeSymbolizer, XmlNamespaceManager nsm)
        {
            string stroke = string.Empty;
            string strokeWidth = string.Empty;
            string strokeOpacity = string.Empty;
            string strokeLinejoin = string.Empty;
            string strokeLineCap = string.Empty;
            string strokeDasharray = string.Empty;
            string strokeDashOffset = string.Empty;
            string fill = string.Empty;
            string fillOpacity = string.Empty;
            string pointSymbolPath = string.Empty;

            var polygonStrokeSymbols = strokeSymbolizer.SelectNodes("sld:Stroke/sld:CssParameter", nsm);
            if (polygonStrokeSymbols == null)
                return;

            foreach (XmlElement polygonStrokeSymbol in polygonStrokeSymbols)
            {
                if (polygonStrokeSymbol != null)
                {
                    switch (polygonStrokeSymbol.GetAttribute("name"))
                    {
                        // line 
                        case "stroke":
                            stroke = polygonStrokeSymbol.InnerXml;
                            break;
                        case "stroke-width":
                            strokeWidth = polygonStrokeSymbol.InnerXml;
                            break;
                        case "stroke-opacity":
                            strokeOpacity = polygonStrokeSymbol.InnerXml;
                            break;
                        case "stroke-linejoin": //“mitre”, “round”, and “bevel”
                            strokeLinejoin = polygonStrokeSymbol.InnerXml;
                            break;
                        case "stroke-linecap": //“butt”, “round”, and “square”.
                            strokeLineCap = polygonStrokeSymbol.InnerXml;
                            break;
                        case "stroke-dasharray":
                            strokeDasharray = polygonStrokeSymbol.InnerXml;
                            break;
                        case "stroke-dashoffset":
                            strokeDashOffset = polygonStrokeSymbol.InnerXml;
                            break;
                    }
                }
            }

            SetStyle(style, stroke, strokeWidth, strokeOpacity, strokeLinejoin, strokeLineCap, strokeDasharray,
                strokeDashOffset, fill, fillOpacity, pointSymbolPath);
        }

        private static void SetPointStyle(VectorStyle style, XmlNode pointSymbolizer, XmlNamespaceManager nsm)
        {
            string stroke = string.Empty;
            string strokeWidth = string.Empty;
            string strokeOpacity = string.Empty;
            string strokeLinejoin = string.Empty;
            string strokeLineCap = string.Empty;
            string strokeDasharray = string.Empty;
            string strokeDashOffset = string.Empty;
            string fill = string.Empty;
            string fillOpacity = string.Empty;
            string pointSymbolPath = string.Empty;


            var pointSymbols = pointSymbolizer.SelectNodes(
                "sld:Graphic/sld:ExternalGraphic/sld:OnlineResource", nsm);

            if (pointSymbols == null)
                return;

            foreach (XmlElement pointSymbol in pointSymbols)
            {
                if (pointSymbol != null)
                {
                    pointSymbolPath = pointSymbol.GetAttribute("xlink:href");
                }
            }
            SetStyle(style, stroke, strokeWidth, strokeOpacity, strokeLinejoin, strokeLineCap, strokeDasharray,
                strokeDashOffset, fill, fillOpacity, pointSymbolPath);
        }

        private static void SetSymbologyForRule(VectorStyle style, XmlElement rule, XmlNamespaceManager nsm)
        {
            var polygonSymbolizers = rule.SelectNodes("sld:PolygonSymbolizer", nsm);
            var lineSymbolizers = rule.SelectNodes("sld:LineSymbolizer", nsm);
            var pointSymbolizers = rule.SelectNodes("sld:PointSymbolizer", nsm);

            if (polygonSymbolizers != null)
                if (polygonSymbolizers.Count > 0)
                {
                    foreach (XmlElement polygonSymbolizer in polygonSymbolizers)
                    {
                        SetFillStyle(style, polygonSymbolizer, nsm);
                    }
                }

            if (lineSymbolizers != null)
                if (lineSymbolizers.Count > 0)
                {
                    foreach (XmlElement lineSymbolizer in lineSymbolizers)
                    {
                        SetStrokeStyle(style, lineSymbolizer, nsm);
                    }
                }

            if (pointSymbolizers != null)
                if (pointSymbolizers.Count > 0)
                {
                    foreach (XmlElement pointSymbolizer in pointSymbolizers)
                    {
                        SetPointStyle(style, pointSymbolizer, nsm);
                    }
                }
        }

        private static void SetStyle(
            VectorStyle style,
            string stroke,
            string strokeWidth,
            string strokeOpacity,
            string strokeLinejoin,
            string strokeLineCap,
            string strokeDasharray,
            string strokeDashOffset,
            string fill,
            string fillOpacity,
            string pointSymbolPath
            )
        {
            if (!String.IsNullOrEmpty(stroke))
            {
                var color = ColorTranslator.FromHtml(stroke);
                var opacity = 255;
                var width = 1f;

                if (!String.IsNullOrEmpty(strokeOpacity))
                {
                    opacity = Convert.ToInt32(Math.Round(Convert.ToDouble(strokeOpacity)/0.0039215, 0));
                    if (opacity > 255)
                        opacity = 255;
                }

                if (!String.IsNullOrEmpty(strokeWidth))
                {
                    width = Convert.ToSingle(strokeWidth);
                }

                Brush brush = new SolidBrush(Color.FromArgb(opacity, Convert.ToInt32(color.R), Convert.ToInt32(color.G),
                    Convert.ToInt32(color.B)));
                var pen = new Pen(brush, width);

                if (!String.IsNullOrEmpty(strokeLinejoin))
                {
                    switch (strokeLinejoin.ToLower())
                    {
                        case "mitre":
                            pen.LineJoin = LineJoin.Miter;
                            break;
                        case "round":
                            pen.LineJoin = LineJoin.Round;
                            break;
                        case "bevel":
                            pen.LineJoin = LineJoin.Bevel;
                            break;

                            //case "miterclipped": // Not in SLD
                            //    pen.LineJoin = StyleLineJoin.MiterClipped;
                            //    break;
                    }
                }

                if (!String.IsNullOrEmpty(strokeLineCap))
                {
                    switch (strokeLineCap.ToLower())
                    {
                        case "butt":
                            pen.StartCap = LineCap.Flat;
                            pen.EndCap = LineCap.Flat;
                            break;
                        case "round":
                            pen.StartCap = LineCap.Round;
                            pen.EndCap = LineCap.Round;
                            break;
                        case "square":
                            pen.StartCap = LineCap.Square;
                            pen.EndCap = LineCap.Square;
                            break;

                            // N.B. Loads of others not used in SLD
                    }
                }

                if (!String.IsNullOrEmpty(strokeDasharray))
                {
                    var numbers = strokeDasharray.Split(Char.Parse(" "));
                    Func<string[], float[]> processor = strings =>
                    {
                        var res = new float[strings.Length];
                        for(var i = 0; i < strings.Length; i++)
                            res[i] = float.Parse(strings[i]);
                        return res.ToArray();
                    };
                    pen.DashPattern = processor(numbers);
                }

                if (!String.IsNullOrEmpty(strokeDashOffset))
                {
                    float dashOffset;
                    var success = float.TryParse(strokeDashOffset, out dashOffset);
                    if (success)
                        pen.DashOffset = dashOffset;
                }

                // Set pen
                style.Line = pen;
            }

            if (!String.IsNullOrEmpty(fill))
            {
                Color color = ColorTranslator.FromHtml(fill);
                int opacity = 255;

                if (!String.IsNullOrEmpty(fillOpacity))
                {
                    opacity = Convert.ToInt32(Math.Round(Convert.ToDouble(fillOpacity)/0.0039215, 0));
                    if (opacity > 255)
                        opacity = 255;
                }

                Brush brush =
                    new SolidBrush(Color.FromArgb(opacity, Convert.ToInt32(color.R), Convert.ToInt32(color.G),
                        Convert.ToInt32(color.B)));

                style.Fill = brush;
            }


            if (!String.IsNullOrEmpty(pointSymbolPath))
            {
                var source = new Uri(pointSymbolPath);

                if (source.IsFile && File.Exists(source.AbsolutePath))
                {
                    style.Symbol = new Bitmap(source.AbsolutePath);

                }
                else if (source.IsAbsoluteUri)
                {

                }
            }

            style.Enabled = true;
            style.EnableOutline = true;
        }

        #endregion
    }
}