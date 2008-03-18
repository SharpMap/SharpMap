// WFS provider by Peter Robineau (peter.robineau@gmx.at)
// This file can be redistributed and/or modified under the terms of the GNU Lesser General Public License.

using System;
using System.Collections.Generic;
using System.Text;
using SharpMap.Data.Providers;

namespace SharpMap.Utilities.Wfs
{
    /// <summary>
    /// This class provides text resources specific for WFS 1.1.0.
    /// </summary>
    public class WFS_1_1_0_XPathTextResources : WFS_XPathTextResourcesBase
    {
        #region Fields and Properties

        ////////////////////////////////////////////////////////////////////////
        // XPath                                                              //                      
        // GetCapabilities WFS 1.1.0                                          //
        ////////////////////////////////////////////////////////////////////////

        private static string _XPATH_SRS=
            "/wfs:WFS_Capabilities/wfs:FeatureTypeList/wfs:FeatureType[_PARAMCOMP_(wfs:Name, $_param1)]/wfs:DefaultSRS";

        /// <summary>
        /// Gets an XPath string addressing the SRID of a featuretype in 'GetCapabilities'.
        /// </summary>
        public string XPATH_SRS
        {
            get { return WFS_1_1_0_XPathTextResources._XPATH_SRS; }
        }

        private static string _XPATH_BBOX =
            "/wfs:WFS_Capabilities/wfs:FeatureTypeList/wfs:FeatureType[_PARAMCOMP_(wfs:Name, $_param1)]/ows:WGS84BoundingBox";

        /// <summary>
        /// Gets an XPath string addressing the bounding box of a featuretype in 'GetCapabilities'.
        /// </summary>
        public string XPATH_BBOX
        {
            get { return WFS_1_1_0_XPathTextResources._XPATH_BBOX; }
        }

        private static string _XPATH_GETFEATURERESOURCE =
           "/wfs:WFS_Capabilities/ows:OperationsMetadata/ows:Operation[@name='GetFeature']/ows:DCP/ows:HTTP/ows:Post/@xlink:href";

        /// <summary>
        /// Gets an XPath string addressing the URI of 'GetFeature'in 'GetCapabilities'.
        /// </summary>
        public string XPATH_GETFEATURERESOURCE
        {
            get { return WFS_1_1_0_XPathTextResources._XPATH_GETFEATURERESOURCE; }
        }

        private static string _XPATH_DESCRIBEFEATURETYPERESOURCE =
           "/wfs:WFS_Capabilities/ows:OperationsMetadata/ows:Operation[@name='DescribeFeatureType']/ows:DCP/ows:HTTP/ows:Post/@xlink:href";

        /// <summary>
        /// Gets an XPath string addressing the URI of 'DescribeFeatureType'in 'GetCapabilities'.
        /// </summary>
        public string XPATH_DESCRIBEFEATURETYPERESOURCE
        {
            get { return WFS_1_1_0_XPathTextResources._XPATH_DESCRIBEFEATURETYPERESOURCE; }
        }

        private static string _XPATH_BOUNDINGBOXMINX = "ows:LowerCorner/text()";

        /// <summary>
        /// Gets an XPath string addressing the lower corner of a featuretype's bounding box in 'GetCapabilities'
        /// for extracting 'minx'.
        /// </summary>
        public string XPATH_BOUNDINGBOXMINX
        {
            get { return WFS_1_1_0_XPathTextResources._XPATH_BOUNDINGBOXMINX; }
        }

        private static string _XPATH_BOUNDINGBOXMINY = "ows:LowerCorner/text()";

        /// <summary>
        /// Gets an XPath string addressing the lower corner of a featuretype's bounding box in 'GetCapabilities'
        /// for extracting 'miny'.
        /// </summary>
        public string XPATH_BOUNDINGBOXMINY
        {
            get { return WFS_1_1_0_XPathTextResources._XPATH_BOUNDINGBOXMINY; }
        }

        private static string _XPATH_BOUNDINGBOXMAXX = "ows:UpperCorner/text()";

        /// <summary>
        /// Gets an XPath string addressing the upper corner of a featuretype's bounding box in 'GetCapabilities'
        /// for extracting 'maxx'.
        /// </summary>
        public string XPATH_BOUNDINGBOXMAXX
        {
            get { return WFS_1_1_0_XPathTextResources._XPATH_BOUNDINGBOXMAXX; }
        }

        private static string _XPATH_BOUNDINGBOXMAXY = "ows:UpperCorner/text()";

        /// <summary>
        /// Gets an XPath string addressing the upper corner of a featuretype's bounding box in 'GetCapabilities'
        /// for extracting 'maxy'.
        /// </summary>
        public string XPATH_BOUNDINGBOXMAXY
        {
            get { return WFS_1_1_0_XPathTextResources._XPATH_BOUNDINGBOXMAXY; }
        }

        #endregion

        #region Constructors

        public WFS_1_1_0_XPathTextResources()
        {}

        #endregion
    }
}
