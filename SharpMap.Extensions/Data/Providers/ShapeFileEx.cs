using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using Geometry = SharpMap.Geometries.Geometry;

namespace SharpMap.Data.Providers
{
    /// <summary>
    /// Extended shapefile provider that does true intersection testing using
    /// <see cref="NetTopologySuite.Geometries.Prepared"/> namespace.
    /// </summary>
    public class ShapeFileEx : ShapeFile
    {
        private IGeometryFactory _factory = new GeometryFactory();
        
        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="filename">The path to the shapefile's fileset</param>
        public ShapeFileEx(string filename)
            :base(filename)
        {}

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="filename">The path to the shapefile's fileset</param>
        /// <param name="fileBasedIndex">An indicator whether to use a file base index.</param>
        public ShapeFileEx(string filename, bool fileBasedIndex)
            :base(filename, fileBasedIndex)
        {}

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="filename">The path to the shapefile's fileset</param>
        /// <param name="fileBasedIndex">An indicator whether to use a file base index.</param>
        /// <param name="useMemoryCache">An indicator whether to cache query results</param>
        public ShapeFileEx(string filename, bool fileBasedIndex, bool useMemoryCache)
            : base(filename, fileBasedIndex, useMemoryCache)
        {}

        /// <summary>
        /// Gets or sets the spatial reference ID (CRS)
        /// </summary>
        public override int SRID
        {
            get { return base.SRID; } 
            set
            {
                base.SRID = value;
                _factory = new GeometryFactory(_factory.PrecisionModel, value);
            }

        }

        /// <summary>
        /// Returns the data associated with all the geometries that are intersected by 'geom'.
        /// </summary>
        /// <param name="geom">The geometry.</param>
        /// <param name="ds">The <see cref="FeatureDataSet"/> to fill data into.</param>
        public override void ExecuteIntersectionQuery(Geometry geom, FeatureDataSet ds)
        {
            //Use the spatial index to get a list of features whose boundingbox intersects bbox
            var objectlist = GetObjectIDsInView(geom.GetBoundingBox());
			if (objectlist.Count == 0)
                return;

            var dt = DbaseFile.NewTable;
            var preparedGeometry = new NetTopologySuite.Geometries.Prepared.PreparedGeometryFactory()
                .Create(Converters.NTS.GeometryConverter.ToNTSGeometry(geom, _factory));
			for (int i = 0; i < objectlist.Count; i++)
			{
			    var testGeom = GetGeometryByID(objectlist[i]);
			    var testNtsGeom = Converters.NTS.GeometryConverter.ToNTSGeometry(testGeom, _factory);
                if (preparedGeometry.Intersects(testNtsGeom))
                {
                    var fdr = GetFeature(objectlist[i], dt);
                    if (fdr != null) dt.AddRow(fdr);
                }
			}

            if (dt.Rows.Count > 0)
                ds.Tables.Add(dt);
        }
    }
}