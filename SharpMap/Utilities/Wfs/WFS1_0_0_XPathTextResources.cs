// WFS provider by Peter Robineau (peter.robineau@gmx.at)
// This file can be redistributed and/or modified under the terms of the GNU Lesser General Public License.

using System;
using System.IO;
using System.Text;
using System.Xml;
using SharpMap.Data.Providers;
using SharpMap.Geometries;

namespace SharpMap.Utilities.Wfs
{
    /// <summary>
    /// This class provides text resources specific for WFS 1.0.0 XML schema (for precompiling).
    /// </summary>
    public class WFS_1_0_0_XPathTextResources : WFS_XPathTextResourcesBase
    {
        #region Fields and Properties

        ////////////////////////////////////////////////////////////////////////
        // XPath                                                              //                      
        // GetCapabilities WFS 1.0.0                                          //
        ////////////////////////////////////////////////////////////////////////

        private static string _XPATH_SRS =
            "/wfs:WFS_Capabilities/wfs:FeatureTypeList/wfs:FeatureType[_PARAMCOMP_(wfs:Name, $_param1)]/wfs:SRS";

        /// <summary>
        /// Gets an XPath string addressing the SRID of a featuretype in 'GetCapabilities'.
        /// </summary>
        public string XPATH_SRS
        {
            get { return WFS_1_0_0_XPathTextResources._XPATH_SRS; }
        }

        private static string _XPATH_BBOX =
            "/wfs:WFS_Capabilities/wfs:FeatureTypeList/wfs:FeatureType[_PARAMCOMP_(wfs:Name, $_param1)]/wfs:LatLongBoundingBox";

        /// <summary>
        /// Gets an XPath string addressing the bounding box of a featuretype in 'GetCapabilities'.
        /// </summary>
        public string XPATH_BBOX
        {
            get { return WFS_1_0_0_XPathTextResources._XPATH_BBOX; }
        }

        private static string _XPATH_GETFEATURERESOURCE =
            "/wfs:WFS_Capabilities/wfs:Capability/wfs:Request/wfs:GetFeature/wfs:DCPType/wfs:HTTP/wfs:Post/@onlineResource";

        /// <summary>
        /// Gets an XPath string addressing the URI of 'GetFeature'in 'GetCapabilities'.
        /// </summary>
        public string XPATH_GETFEATURERESOURCE
        {
            get { return WFS_1_0_0_XPathTextResources._XPATH_GETFEATURERESOURCE; }
        }

        private static string _XPATH_DESCRIBEFEATURETYPERESOURCE =
            "/wfs:WFS_Capabilities/wfs:Capability/wfs:Request/wfs:DescribeFeatureType/wfs:DCPType/wfs:HTTP/wfs:Post/@onlineResource";

        /// <summary>
        /// Gets an XPath string addressing the URI of 'DescribeFeatureType'in 'GetCapabilities'.
        /// </summary>
        public string XPATH_DESCRIBEFEATURETYPERESOURCE
        {
            get { return WFS_1_0_0_XPathTextResources._XPATH_DESCRIBEFEATURETYPERESOURCE; }
        }

        private static string _XPATH_BOUNDINGBOXMINX = "@minx";

        /// <summary>
        /// Gets an XPath string addressing the 'minx'-attribute of a featuretype's bounding box in 'GetCapabilities'.
        /// </summary>
        public string XPATH_BOUNDINGBOXMINX
        {
            get { return WFS_1_0_0_XPathTextResources._XPATH_BOUNDINGBOXMINX; }
        }

        private static string _XPATH_BOUNDINGBOXMAXX = "@maxx";

        /// <summary>
        /// Gets an XPath string addressing the 'maxx'-attribute of a featuretype's bounding box in 'GetCapabilities'.
        /// </summary>
        public string XPATH_BOUNDINGBOXMAXX
        {
            get { return WFS_1_0_0_XPathTextResources._XPATH_BOUNDINGBOXMAXX; }
        }

        private static string _XPATH_BOUNDINGBOXMINY = "@miny";

        /// <summary>
        /// Gets an XPath string addressing the 'miny'-attribute of a featuretype's bounding box in 'GetCapabilities'.
        /// </summary>
        public string XPATH_BOUNDINGBOXMINY
        {
            get { return WFS_1_0_0_XPathTextResources._XPATH_BOUNDINGBOXMINY; }
        }

        private static string _XPATH_BOUNDINGBOXMAXY = "@maxy";

        /// <summary>
        /// Gets an XPath string addressing the 'maxy'-attribute of a featuretype's bounding box in 'GetCapabilities'.
        /// </summary>
        public string XPATH_BOUNDINGBOXMAXY
        {
            get { return WFS_1_0_0_XPathTextResources._XPATH_BOUNDINGBOXMAXY; }
        }

        #endregion

        #region Constructors

        public WFS_1_0_0_XPathTextResources()
        {}

        #endregion
    }
}
