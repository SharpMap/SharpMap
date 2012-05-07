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
        protected IPreparedGeometry PreparedGeometry { get; set; }
        
        protected PreparedGeometryProvider()
            :this(0)
        {
        }

        protected PreparedGeometryProvider(int srid)
            : base(srid)
        {
        }

        protected override void  ReleaseManagedResources()
        {
            PreparedGeometry = null;
 	        base.ReleaseManagedResources();
        }

        protected override void OnBeginExecuteIntersectionQuery(IGeometry geom)
        {
            PreparedGeometry = NetTopologySuite.Geometries.Prepared.PreparedGeometryFactory.Prepare(geom);
            base.OnBeginExecuteIntersectionQuery(geom);
        }

        protected override void OnExecuteIntersectionQuery(IGeometry geom, FeatureDataSet ds)
        {
            ExecuteIntersectionQuery(geom.EnvelopeInternal, ds);

            //index of last added feature data table
            var index = ds.Tables.Count - 1;
            if (index <= 0) return;

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

        protected override void OnEndExecuteIntersectionQuery()
        {
            PreparedGeometry = null;
            base.OnEndExecuteIntersectionQuery();
        }
    }
}