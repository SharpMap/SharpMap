using System;
using System.Collections.Generic;
using System.Text;
using SharpMap.Geometries;

namespace SharpMap.Web.Wms
{
    /// <summary>
    /// Spatial referenced boundingbox
    /// </summary>
    /// <remarks>
    /// The spatial referenced boundingbox is used to communicate boundingboxes of WMS layers together 
    /// with their spatial reference system in which they are specified
    /// </remarks>
    public class SpatialReferencedBoundingBox : BoundingBox
    {
        int _SRID;
        /// <summary>
        /// Initializes a new SpatialReferencedBoundingBox which stores a boundingbox together with the SRID
        /// </summary>
        /// <remarks>This class is used to communicate all the boundingboxes of a WMS server between client.cs and wmslayer.cs</remarks>
        /// <param name="BoundingBox">BoundingBox</param>
        /// <param name="SRID">Spatial Reference ID</param>
        public SpatialReferencedBoundingBox(double minX, double minY, double maxX, double maxY, int srid) : base(minX, minY, maxX, maxY)
        {
            _SRID = srid;
        }

        /// <summary>
        /// The spatial reference ID (CRS)
        /// </summary>
        public int SRID
        {
            get { return _SRID; }
            set { _SRID = value; }
        }
    }
}
