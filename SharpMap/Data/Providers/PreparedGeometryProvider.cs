using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Common.Logging.Configuration;
using GeoAPI.Features;
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
        protected override void OnBeginExecuteIntersectionQuery(IGeometry geom, CancellationToken? cancellationToken = null)
        {
            PreparedGeometry = NetTopologySuite.Geometries.Prepared.PreparedGeometryFactory.Prepare(geom);
            base.OnBeginExecuteIntersectionQuery(geom);
        }

        /// <summary>
        /// Method to perform the intersection query against the data source
        /// </summary>
        /// <param name="geom">The geometry to use as filter</param>
        /// <param name="ds">The feature data set to store the results in</param>
        protected override void OnExecuteIntersectionQuery(IGeometry geom, IFeatureCollectionSet ds, CancellationToken? cancellationToken = null)
        {
            ExecuteIntersectionQuery(geom.EnvelopeInternal, ds);

            //index of last added feature data table
            var index = ds.Count - 1;
            if (index < 0) return;

            var fds = ds[index].Clone();
            fds.AddRange(FilterFeatures(ds[index]));

            ds.Remove(ds[index]);
            ds.Add(fds);
        }

        private IEnumerable<IFeature> FilterFeatures(IEnumerable<IFeature> features)
        {
            foreach (var feature in features)
            {
                if (PreparedGeometry.Intersects(feature.Geometry))
                    yield return feature;
            }
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