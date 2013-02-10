using System;
using GeoAPI.Geometries;
using GeoAPI.Geometries.Prepared;

namespace SharpMap.Data.Providers
{
    /// <summary>
    /// Abstract base provider that provides <see cref="IPreparedGeometry"/> for faster accurate topology evaluation
    /// </summary>
    [Serializable]
    public abstract class PreparedGeometryProvider : BaseProvider
    {
        /// <summary>
        /// Gets or sets a value indicating the <see cref="IPreparedGeometry"/>.
        /// </summary>
        protected IPreparedGeometry PreparedGeometry { get; set; }
        
        /// <summary>
        /// Creates an instance of this class. <see cref="BaseProvider.SRID"/> is set to <value>0</value>.
        /// </summary>
        protected PreparedGeometryProvider()
            :this(0)
        {
        }

        /// <summary>
        /// Creates an instance of this class. <see cref="BaseProvider.SRID"/> is set to <paramref name="srid"/>.
        /// </summary>
        /// <param name="srid">The spatial reference id</param>
        protected PreparedGeometryProvider(int srid)
            : base(srid)
        {
        }

        /// <summary>
        /// Releases all managed resources
        /// </summary>
        protected override void  ReleaseManagedResources()
        {
            PreparedGeometry = null;
 	        base.ReleaseManagedResources();
        }

        /// <summary>
        /// Method to perform preparatory things for executing an intersection query against the data source
        /// </summary>
        /// <param name="geom">The geometry to use as filter.</param>
        protected override void OnBeginExecuteIntersectionQuery(IGeometry geom)
        {
            PreparedGeometry = NetTopologySuite.Geometries.Prepared.PreparedGeometryFactory.Prepare(geom);
            base.OnBeginExecuteIntersectionQuery(geom);
        }

        /// <summary>
        /// Method to perform the intersection query against the data source
        /// </summary>
        /// <param name="geom">The geometry to use as filter</param>
        /// <param name="ds">The feature data set to store the results in</param>
        protected override void OnExecuteIntersectionQuery(IGeometry geom, FeatureDataSet ds)
        {
            ExecuteIntersectionQuery(geom.EnvelopeInternal, ds);

            //index of last added feature data table
            var index = ds.Tables.Count - 1;
            if (index < 0) return;

            var res = CloneTableStructure(ds.Tables[index]);
            res.BeginLoadData();

            var fdt = ds.Tables[index];
            foreach (FeatureDataRow row in fdt.Rows)
            {
                if (PreparedGeometry.Intersects(row.Geometry))
                {
                    var fdr = (FeatureDataRow)res.LoadDataRow(row.ItemArray, true);
                    fdr.Geometry = row.Geometry;
                }
            }

            res.EndLoadData();

            ds.Tables.RemoveAt(index);
            ds.Tables.Add(res);
        }

        /// <summary>
        /// Method to do cleanup work after having performed the intersection query against the data source
        /// </summary>
        protected override void OnEndExecuteIntersectionQuery()
        {
            PreparedGeometry = null;
            base.OnEndExecuteIntersectionQuery();
        }
    }
}