// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
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

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Xml;

namespace SharpMap.Converters.WellKnownText
{
    /// <summary>
    /// Converts spatial reference IDs to a Well-Known Text representation.
    /// </summary>
    public class SpatialReference
    {
        private static readonly Dictionary<int, string> _wkts = new Dictionary<int, string>();
        private static readonly Dictionary<int, string> _proj4s = new Dictionary<int, string>();
        
        /// <summary>
        /// Converts a Spatial Reference ID to a Well-known Text representation
        /// </summary>
        /// <param name="srid">Spatial Reference ID</param>
        /// <returns>Well-known text</returns>
        public static string SridToWkt(int srid)
        {
            string wkt;
            if (_wkts.TryGetValue(srid, out wkt))
            {
                return wkt;
            }

            return SridToDefinition(srid, _wkts);
        }

        /// <summary>
        /// Converts a Spatial Reference ID to a Well-known Text representation
        /// </summary>
        /// <param name="srid">Spatial Reference ID</param>
        /// <returns>Well-known text</returns>
        public static string SridToProj4(int srid)
        {
            string proj4;
            if (_proj4s.TryGetValue(srid, out proj4))
            {
                return proj4;
            }

            return SridToDefinition(srid, _proj4s, "PROJ4");
        }

        /// <summary>
        /// Returns an IEnumerable with all the SRID/WKT pairs known. 
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<KeyValuePair<int, string>> GetAllReferenceSystems()
        {
            XmlNodeList nodes = null;
            try
            {
                var xml = EnsureSpatialRefSysXml();
                nodes = xml.DocumentElement.SelectNodes("/SpatialReference/*");
            }
            catch (Exception e)
            {
                nodes = null;
            }

            if (nodes == null) 
                yield break;

            foreach (XmlNode referenceNode in nodes)
            {
                int srid = int.Parse(referenceNode.SelectSingleNode("SRID").InnerText);
                string wkt = referenceNode.LastChild.InnerText;

                yield return new KeyValuePair<int, string>(srid, wkt);
            }
        }

        private static string SridToDefinition(int srid, IDictionary<int, string> cache, string nodeName = null)
        {
            try
            {
                var xmldoc = EnsureSpatialRefSysXml();
                var node = xmldoc.DocumentElement.SelectSingleNode("/SpatialReference/ReferenceSystem[SRID='" + srid + "']");
                if (node != null)
                {
                    node = string.IsNullOrEmpty(nodeName) ? node.LastChild : node.SelectSingleNode("PROJ4");
                    if (node != null)
                    {
                        var def = node.InnerText;
                        if (!string.IsNullOrEmpty(def)) cache.Add(srid, def);
                        return def;
                    }
                }
            }
            catch (Exception)
            {

            }
            return "";
        }

        private static XmlDocument EnsureSpatialRefSysXml()
        {
            var res = new XmlDocument();

            var uri = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            string file = Path.Combine(Path.GetDirectoryName(uri.LocalPath), "SpatialRefSys.xml");

            if (!File.Exists(file))
                DownloadSrs(file);

            res.Load(file);
            return res;
        }

        private static void DownloadSrs(string path)
        {
            using (var wc = new WebClient())
                wc.DownloadFile(
                    new Uri("https://raw.githubusercontent.com/SharpMap/SharpMap/Branches/1.0/SharpMap/SpatialRefSys.xml"), 
                    path);
            
        }
    }
}
