// Copyright 2006 - Diego Guidi
//
// This file is part of NtsProvider.
// NtsProvider is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with NtsProvider; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NtsGeometry = NetTopologySuite.Geometries.Geometry;

using Geometry = GeoAPI.Geometries.IGeometry;
using BoundingBox = GeoAPI.Geometries.Envelope;


namespace SharpMap.Data.Providers
{
    /// <summary>
    /// The NtsProvider enables you to feed any SharpMap datasource through the <a href="http://github.com/nettopologysuite/nettopologysuite">NetTopologySuite</a>
    /// geometry using any NTS operation.
    /// </summary>
    /// <remarks>
    /// The following example shows how to apply buffers to a shapefile-based river-dataset:
    /// <code lang="C#">
    /// public void InitializeMap(SharpMap.Map map)
    /// {
    ///		//Create Shapefile datasource
    ///		SharpMap.Data.Providers.ShapeFile shp = new SharpMap.Data.Providers.ShapeFile("rivers.shp", true);
    ///		//Create NTS Datasource that gets its data from 'shp' and calls 'NtsOperation' that defines a geoprocessing method
    ///		SharpMap.Data.Providers.NtsProvider nts = new SharpMap.Data.Providers.NtsProvider(shp,new SharpMap.Data.Providers.NtsProvider.GeometryOperationDelegate(NtsOperation));
    ///		//Create the layer for rendering
    ///		SharpMap.Layers.VectorLayer layRivers = new SharpMap.Layers.VectorLayer("Rivers");
    ///		layRivers.DataSource = nts;
    ///		layRivers.Style.Fill = Brushes.Blue;
    ///		map.Layers.Add(layRivers);
    /// }
    /// //Define geoprocessing delegate that buffers all geometries with a distance of 0.5 mapunits
    /// public static void NtsOperation(List&lt;NetTopologySuite.Features.Feature&gt; geoms)
    /// {
    ///		foreach (GisSharpBlog.NetTopologySuite.Features.Feature f in geoms)
    /// 		f.Geometry = f.Geometry.Buffer(0.5);
    /// }
    /// </code>
    /// </remarks>
    public class NtsProvider : PreparedGeometryProvider
    {
        #region Delegates

        /// <summary>
        /// Defines a geometry operation that will be applied to all geometries in <see cref="NtsProvider"/>.
        /// </summary>
        /// <param name="features"></param>
        public delegate void GeometryOperationDelegate(List<Feature> features);

        #endregion

        #region Fields

        // NTS features
        private List<Feature> _features;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NtsProvider"/> class
        /// using a default <see cref="NetTopologySuite.Geometries.PrecisionModel"/> 
        /// with Floating precision.
        /// </summary>        
        protected internal NtsProvider() : this(new PrecisionModel())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NtsProvider"/> class
        /// using the given <paramref name="precisionModel"/>.
        /// </summary>
        /// <param name="precisionModel">
        /// The <see cref="NetTopologySuite.Geometries.PrecisionModel"/>  
        /// to use for define the precision of the geometry operations.
        /// </param>
        /// <seealso cref="NetTopologySuite.Geometries.PrecisionModel"/>
        /// <seealso cref="NetTopologySuite.Geometries.GeometryFactory"/>
        protected internal NtsProvider(IPrecisionModel precisionModel)
        {
            Factory = new GeometryFactory(precisionModel);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NtsProvider"/> class 
        /// from another <see cref="SharpMap.Data.Providers.IProvider" />.
        /// </summary>
        /// <param name="provider">
        /// The base <see cref="SharpMap.Data.Providers.IProvider"/> 
        /// from witch initialize the <see cref="NtsProvider"/> instance.
        /// </param>
        public NtsProvider(IProvider provider) : this()
        {
            BuildFromProvider(provider);
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="NtsProvider"/> class 
        /// from another <see cref="SharpMap.Data.Providers.IProvider" />.
        /// </summary>
        /// <param name="features">
        /// A list of <see cref="NetTopologySuite.Features.Feature"/> 
        /// from witch initialize the <see cref="NtsProvider"/> instance.
        /// </param>
        public NtsProvider(IList<Feature> features) : this()
        {
            if (features == null)
                throw new ArgumentNullException();

            Factory = features[0].Geometry.Factory;
            SRID = Factory.SRID;
            _features = new List<Feature>(features);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NtsProvider"/> class
        /// from another <see cref="SharpMap.Data.Providers.IProvider" />.
        /// </summary>
        /// <param name="provider">
        /// The base <see cref="SharpMap.Data.Providers.IProvider"/> 
        /// from witch initialize the <see cref="NtsProvider"/> instance.
        /// </param>
        /// <param name="precisionModel">
        /// The <see cref="NetTopologySuite.Geometries.PrecisionModel"/>  
        /// to use for define the precision of the geometry operations.
        /// </param>
        /// <seealso cref="NetTopologySuite.Geometries.PrecisionModel"/>     
        /// <seealso cref="NetTopologySuite.Geometries.GeometryFactory"/>
        public NtsProvider(IProvider provider,
            IPrecisionModel precisionModel) : this(precisionModel)
        {
            BuildFromProvider(provider);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NtsProvider"/> class
        /// from another <see cref="SharpMap.Data.Providers.IProvider" />.
        /// </summary>
        /// <param name="provider">
        /// The base <see cref="SharpMap.Data.Providers.IProvider"/> 
        /// from witch initialize the <see cref="NtsProvider"/> instance.
        /// </param>
        /// <param name="operation">
        /// The <see cref="GeometryOperationDelegate"/> to apply 
        /// to all geometry elements in the <paramref name="provider"/>.
        /// </param>  
        public NtsProvider(IProvider provider, GeometryOperationDelegate operation) : this(provider)
        {
            operation(_features);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NtsProvider"/> class
        /// from another <see cref="SharpMap.Data.Providers.IProvider" />.
        /// </summary>
        /// <param name="provider">
        /// The base <see cref="SharpMap.Data.Providers.IProvider"/> 
        /// from witch initialize the <see cref="NtsProvider"/> instance.
        /// </param>
        /// <param name="operation">
        /// The <see cref="GeometryOperationDelegate"/> to apply 
        /// to all geometry elements in the <paramref name="provider"/>.
        /// </param>         
        /// <param name="precisionModel">
        /// The <see cref="NetTopologySuite.Geometries.PrecisionModel"/>  
        /// to use for define the precision of the geometry operations.
        /// </param>
        /// <seealso cref="NetTopologySuite.Geometries.PrecisionModel"/> 
        /// <seealso cref="NetTopologySuite.Geometries.GeometryFactory"/>
        public NtsProvider(IProvider provider, GeometryOperationDelegate operation,
            PrecisionModel precisionModel) : this(provider, precisionModel)
        {
            operation(_features);
        }

        /// <summary>
        /// Builds from the given provider.
        /// </summary>
        /// <param name="provider">
        /// The base <see cref="SharpMap.Data.Providers.IProvider"/> 
        /// from witch initialize the <see cref="NtsProvider"/> instance.
        /// </param>
        private void BuildFromProvider(IProvider provider)
        {
            // Features list initialization
            _features = new List<Feature>(provider.GetFeatureCount());

            try
            {
                // Load all features from the given provider
                provider.Open();
                Collection<uint> ids = provider.GetObjectIDsInView(provider.GetExtents());
                foreach (uint id in ids)
                {
                    var dataRow = provider.GetFeature(id);
                    var geometry = dataRow.Geometry;
                    AttributesTable attributes = new AttributesTable();
                    foreach (DataColumn column in dataRow.Table.Columns)
                    {
                        //if (dataRow[column] == null || dataRow[column] is DBNull)
                        //    throw new ApplicationException("Null values not supported");
                        var value = dataRow[column];
                        if (value is DBNull) value = null;
                        attributes.Add(column.ColumnName, value);
                    }
                    _features.Add(new Feature(geometry, attributes));
                }
            }
            finally
            {
                if (provider.IsOpen)
                    provider.Close();
            }

            // Setting factory and spatial reference id;
            Factory = _features[0].Geometry.Factory;
            SRID = Factory.SRID;
        }

        #endregion

        #region IProvider Members

        /// <summary>
        /// Returns the BoundingBox of the dataset.
        /// </summary>
        /// <returns>BoundingBox</returns>
        public override BoundingBox GetExtents()
        {
            var envelope = new Envelope();
            foreach (var feature in _features)
                envelope.ExpandToInclude(feature.Geometry.EnvelopeInternal);
            return envelope;
        }

        /// <summary>
        /// Gets the feature identified from the given <paramref name="rowId" />.
        /// </summary>
        /// <param name="rowId">The row ID.</param>
        /// <returns></returns>
        public override FeatureDataRow GetFeature(uint rowId)
        {
            var feature = _features[Convert.ToInt32(rowId)];

            var dataTable = new FeatureDataTable();
            foreach (var columnName in feature.Attributes.GetNames())
                dataTable.Columns.Add(new DataColumn(columnName, feature.Attributes.GetType(columnName)));

            var dataRow = dataTable.NewRow();
            dataRow.Geometry = (Geometry) feature.Geometry.Clone();
            foreach (var columnName in feature.Attributes.GetNames())
                dataRow[columnName] = feature.Attributes[columnName];
            return dataRow;
        }

        /// <summary>
        /// Returns the number of features in the dataset.
        /// </summary>
        /// <returns>number of features</returns>
        public override int GetFeatureCount()
        {
            return _features.Count;
        }

        /// <summary>
        /// Returns features within the specified bounding box.
        /// </summary>
        /// <param name="envelope"></param>
        /// <returns></returns>
        public override Collection<Geometry> GetGeometriesInView(BoundingBox envelope)
        {
            // Identifies all the features within the given BoundingBox
            var geoms = new Collection<Geometry>();
            foreach (var feature in _features)
                if (envelope.Intersects(feature.Geometry.EnvelopeInternal))
                    geoms.Add(feature.Geometry);
            return geoms;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="envelope"></param>
        /// <param name="ds"></param>
        public override void ExecuteIntersectionQuery(BoundingBox envelope, FeatureDataSet ds)
        {
            // Identifies all the features within the given BoundingBox
            var dataTable = CreateFeatureDataTable();
            dataTable.BeginLoadData();
            foreach (Feature feature in _features)
            {
                if (envelope.Intersects(feature.Geometry.EnvelopeInternal))
                    CreateNewRow(dataTable, feature);
            }
            dataTable.EndLoadData();

            ds.Tables.Add(dataTable);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geom"></param>
        /// <param name="ds"></param>
        protected override void OnExecuteIntersectionQuery(Geometry geom, FeatureDataSet ds)
        {
            FeatureDataTable dataTable = CreateFeatureDataTable();
            dataTable.BeginLoadData();
            foreach (Feature feature in _features)
                if (PreparedGeometry.Intersects(feature.Geometry))
                    CreateNewRow(dataTable, feature);
            dataTable.EndLoadData();

            ds.Tables.Add(dataTable);
        }

        /// <summary>
        /// Gets the geometry by ID.
        /// </summary>
        /// <param name="oid">The oid.</param>
        /// <returns></returns>
        public override Geometry GetGeometryByID(uint oid)
        {
            var feature = _features[Convert.ToInt32(oid)];
            return feature.Geometry;
        }

        /// <summary>
        /// Gets the object IDs in the view.
        /// </summary>
        /// <param name="bbox">The bbox.</param>
        /// <returns></returns>
        public override Collection<uint> GetObjectIDsInView(BoundingBox bbox)
        {
            // Identifies all the features within the given BoundingBox
            Envelope envelope = bbox;
            Collection<uint> geoms = new Collection<uint>();
            for (int i = 0; i < _features.Count; i++)
                if (envelope.Intersects(_features[i].Geometry.EnvelopeInternal))
                    geoms.Add(Convert.ToUInt32(i));
            return geoms;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is open.
        /// </summary>
        /// <value><c>true</c> if this instance is open; otherwise, <c>false</c>.</value>
        public override bool IsOpen
        {
            get { return _features.Count > 0; }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        protected override void ReleaseManagedResources()
        {
            _features.Clear();
            base.ReleaseManagedResources();
        }

        #endregion

        /// <summary>
        /// Creates a new row in the given <see cref="SharpMap.Data.FeatureDataTable"/> <paramref name="dataTable"/>
        /// using data in <see cref="NetTopologySuite.Features.Feature"/> <paramref name="feature"/>.
        /// </summary>
        /// <param name="dataTable">The <see cref="SharpMap.Data.FeatureDataTable"/> to fill.</param>
        /// <param name="feature">Data to insert in the <see cref="SharpMap.Data.FeatureDataTable"/>.</param>
        private static void CreateNewRow(FeatureDataTable dataTable, Feature feature)
        {
            var row = (FeatureDataRow) dataTable.LoadDataRow(feature.Attributes.GetValues(), true);
            row.Geometry = feature.Geometry;
        }

        /// <summary>
        /// Creates a <see cref="SharpMap.Data.FeatureDataTable"/> using a stub feature (feature[0]).
        /// </summary>
        /// <returns><see cref="SharpMap.Data.FeatureDataTable"/></returns>
        private FeatureDataTable CreateFeatureDataTable()
        {
            if (_features.Count == 0)
                throw new InvalidOperationException();

            var dataTable = new FeatureDataTable();
            var ff = _features[0];
            //foreach (var columnName in ff.Attributes.GetNames())
            //    dataTable.Columns.Add(new DataColumn(columnName, _features[0].Attributes.GetType(columnName)));
            foreach (var columnName in ff.Attributes.GetNames())
            {
                var value = ff.Attributes[columnName];
                dataTable.Columns.Add(value == null
                    ? new DataColumn(columnName, typeof(object))
                    : new DataColumn(columnName, _features[0].Attributes.GetType(columnName)));
            }
            return dataTable;
        }

        /// <summary>
        /// Gets the features in view.
        /// </summary>
        /// <param name="bbox">The bbox.</param>
        /// <param name="ds">The ds.</param>
        public void GetFeaturesInView(BoundingBox bbox, FeatureDataSet ds)
        {
            ExecuteIntersectionQuery(bbox, ds);
        }
    }
}
