using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Common.Logging;
using System.Drawing;
using SharpMap.Layers;
using System.Globalization;
using System.Linq;
using SharpMap.Styles;
using GeoAPI.Geometries;
namespace SharpMap.Serialization
{
    /// <summary>
    /// Creates a SharpMap MapObject from a MapServer MAP File
    /// </summary>
    public static class MapServerMapFileLoader
    {
        static readonly ILog logger = LogManager.GetLogger(typeof(MapServerMapFileLoader));

        public static Map LoadMapFile(string filename)
        {
            Map m = null;
            using (FileStream fs = File.OpenRead(filename))
            {
                m = LoadMapFile(fs, System.IO.Path.GetDirectoryName(filename));
                fs.Close();
            }
            return m;
        }

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
                            if (string.Compare(parts[0], "MAP", true) == 0)
                            {
                                ParseMap(sr, m, basePath);
                            }
                        }
                    }

                }
                catch (Exception ee)
                {
                    logger.Error("Exception when loading MapFile", ee);
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
        /// <param name="sr"></param>
        /// <param name="m"></param>
        private static void ParseMap(StreamReader sr, Map m, string fileDir)
        {
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                line = TrimFixLine(line);

                string[] parts = ParseLine(line);
                string shapePath = fileDir;
                Envelope env = null;
                if (parts != null && parts.Length > 0)
                {
                    if (string.Compare(parts[0], "ANGLE", true) == 0)
                    {
                        //TODO
                    }
                    else if (string.Compare(parts[0], "SHAPEPATH", true) == 0)
                    {
                        shapePath = parts[1];
                    }
                    else if (string.Compare(parts[0], "EXTENT", true) == 0)
                    {
                        env = new Envelope(Convert.ToDouble(parts[1], CultureInfo.InvariantCulture),
                            Convert.ToDouble(parts[3], CultureInfo.InvariantCulture),
                            Convert.ToDouble(parts[2], CultureInfo.InvariantCulture),
                            Convert.ToDouble(parts[4], CultureInfo.InvariantCulture));
                        m.ZoomToBox(env);
                    }
                    else if (string.Compare(parts[0], "IMAGECOLOR", true) == 0)
                    {
                        m.BackColor = Color.FromArgb(Convert.ToInt32(parts[1]), Convert.ToInt32(parts[2]), Convert.ToInt32(parts[3]));
                    }
                    else if (string.Compare(parts[0], "LAYER", true) == 0)
                    {
                        ParseLayer(sr, m, shapePath);
                    }
                    else if (string.Compare(parts[0], "WEB", true) == 0)
                    {
                        //Just parse it to the END element
                        while ((line = sr.ReadLine()) != null)
                        {
                            line = TrimFixLine(line);
                            parts = ParseLine(line);
                            if (parts != null && parts.Length > 0)
                            {
                                if (string.Compare(parts[0], "END") == 0)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    else if (string.Compare(parts[0], "END", true) == 0)
                    {
                        return;
                    }
                }
            }
        }
        /// <summary>
        /// Parses a Layer element
        /// </summary>
        /// <param name="sr"></param>
        /// <param name="m"></param>
        private static void ParseLayer(StreamReader sr, Map m, string shapePath)
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

                    if (string.Compare(parts[0], "CLASS", true) == 0)
                    {
                        classes.Add(ParseClass(sr, m));
                    }
                    else if (string.Compare(parts[0], "CLASSITEM", true) == 0)
                    {
                        classItem = parts[1];
                    }
                    else if (string.Compare(parts[0], "END", true) == 0)
                    {
                        ILayer lay = new VectorLayer(name);
                        lay.MaxVisible = maxScale;
                        lay.MinVisible = minScale;

                        if (connType == "local")
                        {
                            if (!Path.HasExtension(data))
                                data = data + ".shp";
                            SharpMap.Data.Providers.ShapeFile sf = new Data.Providers.ShapeFile(data, true);
                            (lay as VectorLayer).DataSource = sf;
                        }

                        ApplyStyling(lay, classes,classItem, type);

                        m.Layers.Add(lay);
                        return;
                    }
                    else if (string.Compare(parts[0], "MAXSCALEDENOM", true) == 0)
                    {
                        maxScale = Convert.ToDouble(parts[1], CultureInfo.InvariantCulture);
                    }
                    else if (string.Compare(parts[0], "TYPE", true) == 0)
                    {
                        type = parts[1];
                    }
                    else if (string.Compare(parts[0], "MINSCALEDENOM", true) == 0)
                    {
                        minScale = Convert.ToDouble(parts[1], CultureInfo.InvariantCulture);
                    }
                    else if (string.Compare(parts[0], "NAME", true) == 0)
                    {
                        name = parts[1];
                    }
                    else if (string.Compare(parts[0], "OPACITY", true) == 0)
                    {
                        minScale = Convert.ToDouble(parts[1], CultureInfo.InvariantCulture);
                    }
                    else if (string.Compare(parts[0], "DATA", true) == 0)
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

        private static void ApplyStyling(ILayer lay, List<ClassElement> classes, string classItem, string type)
        {
            if (classes.Count == 1)
            {
                /*Simple case.. only one class*/
                (lay as VectorLayer).Style = CreateStyle(classes[0].Styles.ToArray(), type);
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

                (lay as VectorLayer).Theme = new SharpMap.Rendering.Thematics.UniqueValuesTheme<string>(classItem, styleMap, new VectorStyle());
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
                if (string.Compare(type, "point", true) == 0)
                {
                    Brush b = new SolidBrush(styles[0].color);
                    vs.PointColor = b;
                    vs.PointSize = (float)styles[0].Size;
                    if (styles[0].Offset != Point.Empty)
                    {
                        vs.SymbolOffset = new PointF(styles[0].Offset.X, styles[0].Offset.Y);
                    }
                }
                else if (string.Compare(type, "line", true) == 0)
                {
                    vs.Line = new Pen(styles[0].color, (float)styles[0].Width);
                    if (styles[0].Offset != Point.Empty)
                    {
                        vs.LineOffset = styles[0].Offset.Y;
                    }
                }
                else if (string.Compare(type, "polygon", true) == 0)
                {
                    Brush b = new SolidBrush(styles[0].color);
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
                    gs.AddStyle(CreateStyle(new StyleElement[] { styles[i] }, type));
                return gs;
            }
        }

        private static ClassElement ParseClass(StreamReader sr, Map m)
        {
            string line;
            ClassElement classel = new ClassElement()
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
                    if (string.Compare(parts[0],"END",true) == 0)
                    {
                        return classel;
                    }
                    else if (string.Compare(parts[0], "STYLE", true) == 0)
                    {
                        classel.Styles.Add(ParseStyle(sr));
                    }
                    else if (string.Compare(parts[0], "MAXSCALEDENOM", true) == 0)
                    {
                        classel.MaxScale = Convert.ToDouble(parts[1], CultureInfo.InvariantCulture);
                    }
                    else if (string.Compare(parts[0], "MINSCALEDENOM", true) == 0)
                    {
                        classel.MinScale = Convert.ToDouble(parts[1], CultureInfo.InvariantCulture);
                    }
                    else if (string.Compare(parts[0], "EXPRESSION", true) == 0)
                    {
                        classel.Expression = parts[1];
                    }
                    else if (string.Compare(parts[0], "NAME", true) == 0)
                    {
                        classel.Name= parts[1];
                    }
                }
            }
            return null;
        }

        private static StyleElement ParseStyle(StreamReader sr)
        {
            string line;
            StyleElement styleel = new StyleElement()
            {
                Angle = 0,
                LineCap = System.Drawing.Drawing2D.LineCap.Round,
                LineJoin = System.Drawing.Drawing2D.LineJoin.Round,
                BackGroundColor = Color.Empty,
                OutlineColor = Color.Empty,
                Size = 1,
                Offset = Point.Empty
            };

            while ((line = sr.ReadLine()) != null)
            {
                line = TrimFixLine(line);

                string[] parts = ParseLine(line);
                if (parts != null && parts.Length > 0)
                {
                    if (string.Compare(parts[0], "END", true) == 0)
                    {
                        return styleel;
                    }
                    else if (string.Compare(parts[0], "BACKGROUNDCOLOR", true) == 0)
                    {
                        styleel.BackGroundColor = Color.FromArgb(int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]));
                    }
                    else if (string.Compare(parts[0], "COLOR", true) == 0)
                    {
                        styleel.color = Color.FromArgb(int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]));
                    }
                    else if (string.Compare(parts[0], "EXPRESSION", true) == 0)
                    {
                        styleel.Offset = new Point(int.Parse(parts[1]), int.Parse(parts[2]));
                    }
                    else if (string.Compare(parts[0], "SIZE", true) == 0)
                    {
                        styleel.Size = Convert.ToDouble(parts[1], CultureInfo.InvariantCulture);
                    }
                    else if (string.Compare(parts[0], "WIDTH", true) == 0)
                    {
                        styleel.Width = Convert.ToDouble(parts[1], CultureInfo.InvariantCulture);
                    }
                    else if (string.Compare(parts[0], "WIDTH", true) == 0)
                    {
                        styleel.Symbol = parts[1];
                    }
                    else if (string.Compare(parts[0], "OUTLINECOLOR", true) == 0)
                    {
                        styleel.OutlineColor = Color.FromArgb(int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]));
                    }
                    else if (string.Compare(parts[0], "LINECAP", true) == 0)
                    {
                        switch(parts[1].ToLower())
                        {
                            case "square":
                                styleel.LineCap = System.Drawing.Drawing2D.LineCap.Square;
                                break;
                                case "round":
                                styleel.LineCap = System.Drawing.Drawing2D.LineCap.Round;
                                break;
                                case "butt":
                                styleel.LineCap = System.Drawing.Drawing2D.LineCap.Flat;
                                break;
                        }
                    }
                    else if (string.Compare(parts[0], "LINEJOIN", true) == 0)
                    {
                        switch (parts[1].ToLower())
                        {
                            case "miter":
                                styleel.LineJoin = System.Drawing.Drawing2D.LineJoin.Miter;
                                break;
                            case "round":
                                styleel.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
                                break;
                            case "bevel":
                                styleel.LineJoin = System.Drawing.Drawing2D.LineJoin.Bevel;
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
            {
                return "";
            }
            else if (pos > 0)
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
            string[] parts = new string(parmChars).Split(new char[] {'\n'},  StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
                parts[i] = parts[i].Trim(' ', '\"','\'');

            return parts;
        }

        class StyleElement
        {
            public Color color { get; set; }
            public Color OutlineColor { get; set; }
            public string Symbol { get; set; }
            public double Angle { get; set; }
            public double Size { get; set; }
            public double Width { get; set; }
            public Color BackGroundColor { get; set; }
            public System.Drawing.Drawing2D.LineCap LineCap { get; set; }
            public System.Drawing.Drawing2D.LineJoin LineJoin { get; set; }
            public Point Offset { get; set; }

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
