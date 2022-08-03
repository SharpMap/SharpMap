using Common.Logging;
using NetTopologySuite.Geometries;
using SharpMap.Layers;
using SharpMap.Styles;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
namespace SharpMap.Serialization
{
    /// <summary>
    /// Creates a SharpMap MapObject from a MapServer MAP File
    /// </summary>
    public static class MapServerMapFileLoader
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(MapServerMapFileLoader));

        /// <summary>
        /// Loads a map from a map file
        /// </summary>
        /// <param name="filePath">Path to the map file</param>
        /// <returns>A map</returns>
        public static Map LoadMapFile(string filePath)
        {
            Map m = null;
            if (File.Exists(filePath))
            {

                using (FileStream fs = File.OpenRead(filePath))
                {
                    m = LoadMapFile(fs, Path.GetDirectoryName(filePath));
                    fs.Close();
                }
            }

            return m;
        }

        /// <summary>
        /// Loads a map from a map file stream
        /// </summary>
        /// <param name="s">A map file stream</param>
        /// <param name="basePath">Path to the folder of the map file stream</param>
        /// <returns>A map</returns>
        public static Map LoadMapFile(Stream s, string basePath)
        {
            Map m = new Map();

            using (StreamReader sr = new StreamReader(s))
            {
                try
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        line = TrimFixLine(line);

                        string[] parts = ParseLine(line);
                        if (parts != null && parts.Length > 0)
                        {
                            if (string.Compare(parts[0], "MAP", StringComparison.InvariantCultureIgnoreCase) == 0)
                            {
                                ParseMap(sr, m, basePath);
                            }
                        }
                    }

                }
                catch (Exception ee)
                {
                    _logger.Error("Exception when loading MapFile", ee);
                }
                finally
                {
                    sr.Close();
                }
            }
            return m;
        }

        /// <summary>
        /// Parses the MAP element
        /// </summary>
        /// <param name="sr">A text reader</param>
        /// <param name="m">The map</param>
        /// <param name="basePath">Path to the folder of the map file stream</param>
        private static void ParseMap(TextReader sr, Map m, string basePath)
        {
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                line = TrimFixLine(line);

                string[] parts = ParseLine(line);
                string shapePath = basePath;
                if (parts != null && parts.Length > 0)
                {
                    if (string.Compare(parts[0], "ANGLE", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        //TODO
                    }
                    else if (string.Compare(parts[0], "SHAPEPATH", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        shapePath = parts[1];
                    }
                    else if (string.Compare(parts[0], "EXTENT", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        var env = new Envelope(Convert.ToDouble(parts[1], CultureInfo.InvariantCulture),
                            Convert.ToDouble(parts[3], CultureInfo.InvariantCulture),
                            Convert.ToDouble(parts[2], CultureInfo.InvariantCulture),
                            Convert.ToDouble(parts[4], CultureInfo.InvariantCulture));
                        m.ZoomToBox(env);
                    }
                    else if (string.Compare(parts[0], "IMAGECOLOR", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        m.BackColor = Color.FromArgb(Convert.ToInt32(parts[1]), Convert.ToInt32(parts[2]), Convert.ToInt32(parts[3]));
                    }
                    else if (string.Compare(parts[0], "LAYER", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        ParseLayer(sr, m, shapePath);
                    }
                    else if (string.Compare(parts[0], "WEB", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        //Just parse it to the END element
                        while ((line = sr.ReadLine()) != null)
                        {
                            line = TrimFixLine(line);
                            parts = ParseLine(line);
                            if (parts != null && parts.Length > 0)
                            {
                                if (string.Compare(parts[0], "END", StringComparison.InvariantCultureIgnoreCase) == 0)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    else if (string.Compare(parts[0], "END", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        return;
                    }
                }
            }
        }
        /// <summary>
        /// Parses a Layer element
        /// </summary>
        /// <param name="sr">A text reader</param>
        /// <param name="m">The map</param>
        /// <param name="shapePath">Path to the geodata file</param>
        private static void ParseLayer(TextReader sr, Map m, string shapePath)
        {
            string line;
            double maxScale = double.MaxValue;
            double minScale = double.MinValue;

            string connType = "local";
            string name = "";
            string data = "";
            string type = "";
            string classItem = "";
            List<ClassElement> classes = new List<ClassElement>();
            while ((line = sr.ReadLine()) != null)
            {
                line = TrimFixLine(line);

                string[] parts = ParseLine(line);
                if (parts != null && parts.Length > 0)
                {

                    if (string.Compare(parts[0], "CLASS", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        classes.Add(ParseClass(sr));
                    }
                    else if (string.Compare(parts[0], "CLASSITEM", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        classItem = parts[1];
                    }
                    else if (string.Compare(parts[0], "END", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        var lay = new VectorLayer(name);
                        lay.MaxVisible = maxScale;
                        lay.MinVisible = minScale;

                        if (connType == "local")
                        {
                            if (!Path.HasExtension(data))
                                data = data + ".shp";
                            Data.Providers.ShapeFile sf = new Data.Providers.ShapeFile(data, true);
                            lay.DataSource = sf;
                        }

                        ApplyStyling(lay, classes, classItem, type);

                        m.Layers.Add(lay);
                        return;
                    }
                    else if (string.Compare(parts[0], "MAXSCALEDENOM", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        maxScale = Convert.ToDouble(parts[1], CultureInfo.InvariantCulture);
                    }
                    else if (string.Compare(parts[0], "TYPE", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        type = parts[1];
                    }
                    else if (string.Compare(parts[0], "MINSCALEDENOM", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        minScale = Convert.ToDouble(parts[1], CultureInfo.InvariantCulture);
                    }
                    else if (string.Compare(parts[0], "NAME", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        name = parts[1];
                    }
                    else if (string.Compare(parts[0], "OPACITY", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        minScale = Convert.ToDouble(parts[1], CultureInfo.InvariantCulture);
                    }
                    else if (string.Compare(parts[0], "DATA", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        data = parts[1];
                        if (!Path.IsPathRooted(data))
                        {
                            data = Path.Combine(shapePath, data);
                        }


                    }
                }
            }
        }

        private static void ApplyStyling(ILayer lay, IReadOnlyList<ClassElement> classes, string classItem, string type)
        {
            if (!(lay is VectorLayer vLay))
                return;

            if (classes.Count == 1)
            {
                /*Simple case.. only one class*/
                vLay.Style = CreateStyle(classes[0].Styles.ToArray(), type);
                lay.MaxVisible = classes[0].MaxScale;
                lay.MinVisible = classes[0].MinScale;
            }
            else
            {
                /* Need to apply themes for be able to do this..*/
                //(lay as VectorLayer).Theme = new SharpMap.Rendering.Thematics.C
                Dictionary<string, IStyle> styleMap = new Dictionary<string, IStyle>();
                foreach (var c in classes)
                {
                    styleMap.Add(c.Expression, CreateStyle(c.Styles.ToArray(), type));
                }

                vLay.Theme = new Rendering.Thematics.UniqueValuesTheme<string>(classItem, styleMap, new VectorStyle());
            }
        }

        private static VectorStyle CreateStyle(StyleElement[] styles, string type)
        {
            if (styles.Length == 1)
            {
                VectorStyle vs = new VectorStyle();
                vs.Symbol = null;
                vs.Fill = null;
                vs.Line = null;
                if (string.Compare(type, "point", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    Brush b = new SolidBrush(styles[0].Color);
                    vs.PointColor = b;
                    vs.PointSize = (float)styles[0].Size;
                    if (styles[0].Offset != System.Drawing.Point.Empty)
                    {
                        vs.SymbolOffset = new PointF(styles[0].Offset.X, styles[0].Offset.Y);
                    }
                }
                else if (string.Compare(type, "line", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    vs.Line = new Pen(styles[0].Color, (float)styles[0].Width);
                    if (styles[0].Offset != System.Drawing.Point.Empty)
                    {
                        vs.LineOffset = styles[0].Offset.Y;
                    }
                }
                else if (string.Compare(type, "polygon", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    Brush b = new SolidBrush(styles[0].Color);
                    vs.Fill = b;
                }

                if (styles[0].OutlineColor != Color.Empty)
                {
                    vs.EnableOutline = true;
                    vs.Outline = new Pen(styles[0].OutlineColor);
                }
                return vs;
            }
            else
            {
                GroupStyle gs = new GroupStyle();
                for (int i = 0; i < styles.Length; i++)
                    gs.AddStyle(CreateStyle(new[] { styles[i] }, type));
                return gs;
            }
        }

        private static ClassElement ParseClass(TextReader sr)
        {
            string line;
            ClassElement classEl = new ClassElement()
            {
                MaxScale = double.MaxValue,
                MinScale = double.MinValue,
                Styles = new List<StyleElement>()
            };

            while ((line = sr.ReadLine()) != null)
            {
                line = TrimFixLine(line);

                string[] parts = ParseLine(line);
                if (parts != null && parts.Length > 0)
                {
                    if (string.Compare(parts[0], "END", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        return classEl;
                    }
                    else if (string.Compare(parts[0], "STYLE", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        classEl.Styles.Add(ParseStyle(sr));
                    }
                    else if (string.Compare(parts[0], "MAXSCALEDENOM", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        classEl.MaxScale = Convert.ToDouble(parts[1], CultureInfo.InvariantCulture);
                    }
                    else if (string.Compare(parts[0], "MINSCALEDENOM", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        classEl.MinScale = Convert.ToDouble(parts[1], CultureInfo.InvariantCulture);
                    }
                    else if (string.Compare(parts[0], "EXPRESSION", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        classEl.Expression = parts[1];
                    }
                    else if (string.Compare(parts[0], "NAME", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        classEl.Name = parts[1];
                    }
                }
            }
            return null;
        }

        private static StyleElement ParseStyle(TextReader sr)
        {
            string line;
            StyleElement styleEl = new StyleElement()
            {
                Angle = 0,
                LineCap = System.Drawing.Drawing2D.LineCap.Round,
                LineJoin = System.Drawing.Drawing2D.LineJoin.Round,
                BackGroundColor = Color.Empty,
                OutlineColor = Color.Empty,
                Size = 1,
                Offset = System.Drawing.Point.Empty
            };

            while ((line = sr.ReadLine()) != null)
            {
                line = TrimFixLine(line);

                string[] parts = ParseLine(line);
                if (parts != null && parts.Length > 0)
                {
                    if (string.Compare(parts[0], "END", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        return styleEl;
                    }
                    else if (string.Compare(parts[0], "BACKGROUNDCOLOR", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        styleEl.BackGroundColor = Color.FromArgb(int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]));
                    }
                    else if (string.Compare(parts[0], "COLOR", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        styleEl.Color = Color.FromArgb(int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]));
                    }
                    else if (string.Compare(parts[0], "EXPRESSION", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        styleEl.Offset = new System.Drawing.Point(int.Parse(parts[1]), int.Parse(parts[2]));
                    }
                    else if (string.Compare(parts[0], "SIZE", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        styleEl.Size = Convert.ToDouble(parts[1], CultureInfo.InvariantCulture);
                    }
                    else if (string.Compare(parts[0], "WIDTH", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        styleEl.Width = Convert.ToDouble(parts[1], CultureInfo.InvariantCulture);
                    }
                    else if (string.Compare(parts[0], "WIDTH", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        styleEl.Symbol = parts[1];
                    }
                    else if (string.Compare(parts[0], "OUTLINECOLOR", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        styleEl.OutlineColor = Color.FromArgb(int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]));
                    }
                    else if (string.Compare(parts[0], "LINECAP", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        switch (parts[1].ToLower())
                        {
                            case "square":
                                styleEl.LineCap = System.Drawing.Drawing2D.LineCap.Square;
                                break;
                            case "round":
                                styleEl.LineCap = System.Drawing.Drawing2D.LineCap.Round;
                                break;
                            case "butt":
                                styleEl.LineCap = System.Drawing.Drawing2D.LineCap.Flat;
                                break;
                        }
                    }
                    else if (string.Compare(parts[0], "LINEJOIN", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        switch (parts[1].ToLower())
                        {
                            case "miter":
                                styleEl.LineJoin = System.Drawing.Drawing2D.LineJoin.Miter;
                                break;
                            case "round":
                                styleEl.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
                                break;
                            case "bevel":
                                styleEl.LineJoin = System.Drawing.Drawing2D.LineJoin.Bevel;
                                break;
                        }
                    }
                }
            }
            return null;
        }

        private static string TrimFixLine(string line)
        {
            int pos = line.IndexOf('#');
            if (pos == 0)
                return "";

            if (pos > 0)
            {
                line = line.Substring(0, pos);
            }

            return line.Trim();
        }

        static string[] ParseLine(string line)
        {
            char[] parmChars = line.ToCharArray();
            bool inQuote = false;
            for (int index = 0; index < parmChars.Length; index++)
            {
                if (parmChars[index] == '"' || parmChars[index] == '\'')
                    inQuote = !inQuote;
                if (!inQuote && parmChars[index] == ' ')
                    parmChars[index] = '\n';
            }
            string[] parts = new string(parmChars).Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
                parts[i] = parts[i].Trim(' ', '\"', '\'');

            return parts;
        }

        class StyleElement
        {
            public Color Color { get; set; }
            public Color OutlineColor { get; set; }
            public string Symbol { get; set; }
            public double Angle { get; set; }
            public double Size { get; set; }
            public double Width { get; set; }
            public Color BackGroundColor { get; set; }
            public System.Drawing.Drawing2D.LineCap LineCap { get; set; }
            public System.Drawing.Drawing2D.LineJoin LineJoin { get; set; }
            public System.Drawing.Point Offset { get; set; }

        }
        class ClassElement
        {
            public string Name { get; set; }
            public double MinScale { get; set; }
            public double MaxScale { get; set; }
            public string Expression { get; set; }
            public List<StyleElement> Styles { get; set; }
        }
    }
}
