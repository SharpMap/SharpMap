// WFS provider by Peter Robineau (peter.robineau@gmx.at)
// This file can be redistributed and/or modified under the terms of the GNU Lesser General Public License.

using NetTopologySuite.Geometries;
using System;
using System.IO;
using System.Text;
using System.Xml;


namespace SharpMap.Utilities.Wfs
{
    //ReSharper disable InconsistentNaming
    /// <summary>
    /// Text resources class for WebFeatureService v1.0.0
    /// </summary>
    public class WFS_1_0_0_TextResources : WFS_1_0_0_XPathTextResources, IWFS_TextResources
    {
        #region Public Member

        ////////////////////////////////////////////////////////////////////////
        // HTTP Configuration                                                 //                      
        // POST & GET                                                         //
        ////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// This method returns the query string for 'GetCapabilities'.
        /// </summary>
        public string GetCapabilitiesRequest()
        {
            return "?SERVICE=WFS&Version=1.0.0&REQUEST=GetCapabilities";
        }

        /// <summary>
        /// This method returns the query string for 'DescribeFeatureType'.
        /// </summary>
        /// <param name="featureTypeName">The name of the featuretype to query</param>
        public string DescribeFeatureTypeRequest(string featureTypeName)
        {
            return "?SERVICE=WFS&Version=1.0.0&REQUEST=DescribeFeatureType&TYPENAME=" + featureTypeName;
        }

        #endregion

        #region IWFS_TextResources Member

        /// <summary>
        /// This method returns the query string for 'GetFeature'.
        /// </summary>
        /// <param name="featureTypeInfo">A <see cref="Wfs.WfsFeatureTypeInfo"/> instance providing metadata of the featuretype to query</param>
        /// <param name="boundingBox">The bounding box of the query</param>
        /// <param name="filter">An instance implementing <see cref="IFilter"/></param>
        /// <param name="loadAllElements">True to get all feature elements, false to get only geometry element</param>
        public string GetFeatureGETRequest(WfsFeatureTypeInfo featureTypeInfo, Envelope boundingBox, IFilter filter, bool loadAllElements)
        {
            string qualification = string.IsNullOrEmpty(featureTypeInfo.Prefix)
                                       ? string.Empty
                                       : featureTypeInfo.Prefix + ":";
            string filterString = string.Empty;

            if (filter != null)
            {
                filterString = filter.Encode();
                filterString = filterString.Replace("<", "%3C");
                filterString = filterString.Replace(">", "%3E");
                filterString = filterString.Replace(" ", "");
                filterString = filterString.Replace("*", "%2a");
                filterString = filterString.Replace("#", "%23");
                filterString = filterString.Replace("!", "%21");
            }

            StringBuilder filterBuilder = new StringBuilder();
            // &filter=<Filter xmlns="http://www.opengis.net/ogc" xmlns:gml="http://www.opengis.net/gml">
            filterBuilder.Append("&filter=%3CFilter%20xmlns=%22" + Uri.EscapeDataString(NSOGC) + "%22%20xmlns:gml=%22" + Uri.EscapeDataString(NSGML) +
                                 "%22%3E");
            // <BBOX>
            filterBuilder.Append("%3CBBOX%3E");
            if (!loadAllElements)
            {
                // <PropertyName>
                filterBuilder.Append("%3CPropertyName%3E");
                filterBuilder.Append(qualification);
                filterBuilder.Append(featureTypeInfo.Geometry._GeometryName);
                // </PropertyName>
                filterBuilder.Append("%3C/PropertyName%3E");
            }
            // <gml:Box srsName="EPSG:....">
            filterBuilder.Append("%3Cgml:Box%20srsName=%22EPSG:" + featureTypeInfo.SRID + "%22%3E");
            // <gml:coordinates>
            filterBuilder.Append("%3Cgml:coordinates%3E");
            filterBuilder.Append(XmlConvert.ToString(boundingBox.MinX) + ",");
            filterBuilder.Append(XmlConvert.ToString(boundingBox.MinY) + "%20");
            filterBuilder.Append(XmlConvert.ToString(boundingBox.MaxX) + ",");
            filterBuilder.Append(XmlConvert.ToString(boundingBox.MaxY));
            // </gml:coordinates></gml:Box></BBOX>
            filterBuilder.Append("%3C/gml:coordinates%3E%3C/gml:Box%3E%3C/BBOX%3E");
            filterBuilder.Append(filterString);
            filterBuilder.Append("%3C/Filter%3E");

            return "?SERVICE=WFS&Version=1.0.0&REQUEST=GetFeature&TYPENAME=" + qualification + featureTypeInfo.Name +
                   (loadAllElements ? "" : "&PROPERTYNAME=" + qualification + featureTypeInfo.Geometry._GeometryName) +
                   "&SRS=" + featureTypeInfo.SRID + filterBuilder;
        }

        /// <summary>
        /// This method returns the POST request for 'GetFeature'.
        /// </summary>
        /// <param name="featureTypeInfo">A <see cref="Wfs.WfsFeatureTypeInfo"/> instance providing metadata of the featuretype to query</param>
        /// <param name="labelProperty">A property necessary for label rendering</param>
        /// <param name="boundingBox">The bounding box of the query</param>
        /// <param name="filter">An instance implementing <see cref="IFilter"/></param>
        /// <param name="loadAllElements">True to get all feature elements, false to get only geometry element</param>
        public byte[] GetFeaturePOSTRequest(WfsFeatureTypeInfo featureTypeInfo, string labelProperty,
                                            Envelope boundingBox, IFilter filter, bool loadAllElements)
        {
            string qualification = string.IsNullOrEmpty(featureTypeInfo.Prefix)
                                       ? string.Empty
                                       : featureTypeInfo.Prefix + ":";

            using (StringWriter sWriter = new StringWriter())
            {
                using (XmlTextWriter xWriter = new XmlTextWriter(sWriter))
                {
                    xWriter.Namespaces = true;
                    xWriter.WriteStartElement("GetFeature", NSWFS);
                    xWriter.WriteAttributeString("service", "WFS");
                    xWriter.WriteAttributeString("version", "1.0.0");
                    xWriter.WriteStartElement("Query", NSWFS);
                    xWriter.WriteAttributeString("typeName", qualification + featureTypeInfo.Name);
                    if (!loadAllElements)
                    {
                        xWriter.WriteElementString("PropertyName",
                            qualification + featureTypeInfo.Geometry._GeometryName);
                        if (!string.IsNullOrEmpty(labelProperty))
                            xWriter.WriteElementString("PropertyName", qualification + labelProperty);
                    }
                    xWriter.WriteStartElement("Filter", NSOGC);
                    xWriter.WriteStartElement("BBOX");
                    if (!loadAllElements)
                        xWriter.WriteElementString("PropertyName", featureTypeInfo.Geometry._GeometryName);
                    xWriter.WriteStartElement("gml", "Box", NSGML);
                    xWriter.WriteAttributeString("srsName",
                                                 "http://www.opengis.net/gml/srs/epsg.xml#" + featureTypeInfo.SRID);
                    xWriter.WriteElementString("coordinates", NSGML,
                                               XmlConvert.ToString(boundingBox.MinX) + "," +
                                               XmlConvert.ToString(boundingBox.MinY) + " " +
                                               XmlConvert.ToString(boundingBox.MaxX) + "," +
                                               XmlConvert.ToString(boundingBox.MaxY));
                    xWriter.WriteEndElement();
                    xWriter.WriteEndElement();
                    if (filter != null) xWriter.WriteRaw(filter.Encode());
                    if (filter != null) xWriter.WriteEndElement();
                    xWriter.WriteEndElement();
                    xWriter.WriteEndElement();
                    xWriter.WriteEndElement();
                    xWriter.Flush();
                    return Encoding.UTF8.GetBytes(sWriter.ToString());
                }
            }
        }

        #endregion
    }
}
