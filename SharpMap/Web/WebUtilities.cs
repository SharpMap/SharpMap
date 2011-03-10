using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;

namespace SharpMap.Web
{
    class WebUtilities
    {
        #region Reusable XML Parsing

        public static XmlNode FindEpsgNode(XmlNode bbox)
        {
            if (bbox == null || bbox.Attributes == null)
                throw new ArgumentNullException("bbox");

            XmlNode epsgNode = ((bbox.Attributes["srs"] ?? bbox.Attributes["crs"]) ?? bbox.Attributes["SRS"]) ?? bbox.Attributes["CRS"];
            return epsgNode;
        }

        public static bool TryParseNodeAsEpsg(XmlNode node, out int epsg)
        {
            epsg = default(int);
            if (node == null) return false;
            string epsgString = node.Value;
            if (String.IsNullOrEmpty(epsgString)) return false;
            const string prefix = "EPSG:";
            int index = epsgString.IndexOf(prefix);
            if (index < 0) return false;
            return (Int32.TryParse(epsgString.Substring(index + prefix.Length), NumberStyles.Any, Map.NumberFormatEnUs, out epsg));
        }

        public static double ParseNodeAsDouble(XmlNode node, double defaultValue)
        {
            if (node == null) return defaultValue;
            if (String.IsNullOrEmpty(node.InnerText)) return defaultValue;
            double value;
            if (Double.TryParse(node.InnerText, NumberStyles.Any, Map.NumberFormatEnUs, out value))
                return value;
            return defaultValue;
        }

        public static bool TryParseNodeAsDouble(XmlNode node, out double value)
        {
            value = default(double);
            if (node == null) return false;
            if (String.IsNullOrEmpty(node.InnerText)) return false;
            return Double.TryParse(node.InnerText, NumberStyles.Any, Map.NumberFormatEnUs, out value);
        }

        #endregion
    }
}
