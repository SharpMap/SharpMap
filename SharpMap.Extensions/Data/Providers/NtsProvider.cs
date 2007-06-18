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
using System.Diagnostics;
using System.Globalization;
using System.Text;

using SharpMap.Converters.NTS;

namespace SharpMap.Data.Providers
{
    /// <summary>
	/// The NtsProvider enables you to feed any SharpMap datasource through the <a href="http://sourceforge.net/projects/nts">NetTopologySuite</a>
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
	/// public static void NtsOperation(List<GisSharpBlog.NetTopologySuite.Features.Feature> geoms)
	/// {
	///		foreach (GisSharpBlog.NetTopologySuite.Features.Feature f in geoms)
	/// 		f.Geometry = f.Geometry.Buffer(0.5);
	/// }
	/// </code>
	/// </remarks>
    public class NtsProvider : IProvider
    {

		/// <summary>
		/// Defines a geometry operation that will be applied to all geometries in <see cref="NtsProvider"/>.
		/// </summary>
		/// <param name="features"></param>
		public delegate void GeometryOperationDelegate(List<GisSharpBlog.NetTopologySuite.Features.Feature> features);
        
        #region Fields

        // Factory for NTS features
        private GisSharpBlog.NetTopologySuite.Geometries.GeometryFactory geometryFactory = null;

        // NTS features
        private List<GisSharpBlog.NetTopologySuite.Features.Feature> features = null;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="T:NtsProvider"/> class
        /// using a default <see cref="GisSharpBlog.NetTopologySuite.Geometries.PrecisionModel"/> 
        /// with Floating precision.
        /// </summary>        
        protected internal NtsProvider() : this(new GisSharpBlog.NetTopologySuite.Geometries.PrecisionModel()) { }

        /// <summary>
		/// Initializes a new instance of the <see cref="T:NtsProvider"/> class
        /// using the given <paramref name="precisionModel"/>.
        /// </summary>
        /// <param name="precisionModel">
        /// The <see cref="GisSharpBlog.NetTopologySuite.Geometries.PrecisionModel"/>  
        /// to use for define the precision of the geometry operations.
        /// </param>
        /// <seealso cref="GisSharpBlog.NetTopologySuite.Geometries.PrecisionModels"/>
        /// <seealso cref="GisSharpBlog.NetTopologySuite.Geometries.GeometryFactory"/>
        protected internal NtsProvider(GisSharpBlog.NetTopologySuite.Geometries.PrecisionModel precisionModel)
        {
            geometryFactory = new GisSharpBlog.NetTopologySuite.Geometries.GeometryFactory(precisionModel);
        }

        /// <summary>
		/// Initializes a new instance of the <see cref="T:NtsProvider"/> class 
        /// from another <see cref="SharpMap.Data.Providers.IProvider" />.
        /// </summary>
        /// <param name="provider">
        /// The base <see cref="SharpMap.Data.Providers.IProvider"/> 
		/// from witch initialize the <see cref="NtsProvider"/> instance.
        /// </param>
        public NtsProvider(SharpMap.Data.Providers.IProvider provider) : this()
        {                        
            BuildFromProvider(provider);            
        }

        /// <summary>
		/// Initializes a new instance of the <see cref="T:NtsProvider"/> class
        /// from another <see cref="SharpMap.Data.Providers.IProvider" />.
        /// </summary>
        /// <param name="provider">
        /// The base <see cref="SharpMap.Data.Providers.IProvider"/> 
		/// from witch initialize the <see cref="NtsProvider"/> instance.
        /// </param>
        /// <param name="precisionModel">
        /// The <see cref="GisSharpBlog.NetTopologySuite.Geometries.PrecisionModel"/>  
        /// to use for define the precision of the geometry operations.
        /// </param>
        /// <seealso cref="GisSharpBlog.NetTopologySuite.Geometries.PrecisionModels"/>     
        /// <seealso cref="GisSharpBlog.NetTopologySuite.Geometries.GeometryFactory"/>
        public NtsProvider(SharpMap.Data.Providers.IProvider provider, 
            GisSharpBlog.NetTopologySuite.Geometries.PrecisionModel precisionModel) : this(precisionModel)
        {                       
            BuildFromProvider(provider);            
        }

        /// <summary>
		/// Initializes a new instance of the <see cref="T:NtsProvider"/> class
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
        public NtsProvider(SharpMap.Data.Providers.IProvider provider, GeometryOperationDelegate operation) : this(provider)
        {            
            operation(features);
         }

        /// <summary>
		 /// Initializes a new instance of the <see cref="T:NtsProvider"/> class
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
        /// The <see cref="GisSharpBlog.NetTopologySuite.Geometries.PrecisionModel"/>  
        /// to use for define the precision of the geometry operations.
        /// </param>
        /// <seealso cref="GisSharpBlog.NetTopologySuite.Geometries.PrecisionModels"/> 
        /// <seealso cref="GisSharpBlog.NetTopologySuite.Geometries.GeometryFactory"/>
        public NtsProvider(SharpMap.Data.Providers.IProvider provider, GeometryOperationDelegate operation,
            GisSharpBlog.NetTopologySuite.Geometries.PrecisionModel precisionModel) : this(provider, precisionModel)
        {            
            operation(features);         
        }

        /// <summary>
        /// Builds from the given provider.
        /// </summary>
        /// <param name="provider">
        /// The base <see cref="SharpMap.Data.Providers.IProvider"/> 
		/// from witch initialize the <see cref="NtsProvider"/> instance.
        /// </param>
        private void BuildFromProvider(SharpMap.Data.Providers.IProvider provider)
        {            
            // Features list initialization
            features = new List<GisSharpBlog.NetTopologySuite.Features.Feature>(provider.GetFeatureCount());

            try
            {
                // Load all features from the given provider
                provider.Open();                
                Collection<uint> ids = provider.GetObjectIDsInView(provider.GetExtents());             
                foreach (uint id in ids)
                {
                    SharpMap.Data.FeatureDataRow dataRow = provider.GetFeature(id);
                    GisSharpBlog.NetTopologySuite.Geometries.Geometry geometry = GeometryConverter.ToNTSGeometry(dataRow.Geometry, geometryFactory);
                    GisSharpBlog.NetTopologySuite.Features.AttributesTable attributes = new GisSharpBlog.NetTopologySuite.Features.AttributesTable();
                    foreach (DataColumn column in dataRow.Table.Columns)
                    {                        
                        if (dataRow[column] == null || dataRow[column].GetType() == typeof(System.DBNull))
                            throw new ApplicationException("Null values not supported");
                        attributes.AddAttribute(column.ColumnName, dataRow[column]);
                    }
                    features.Add(new GisSharpBlog.NetTopologySuite.Features.Feature(geometry, attributes));
                }
            }
            finally
            {
                if (provider.IsOpen)
                    provider.Close();
            }
        }

        #endregion

        #region IProvider Members
		
        /// <summary>
        /// Returns the data associated with all the geometries that is within 'distance' of 'geom'
        /// </summary>
        /// <param name="geom"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
		[Obsolete("Use ExecuteIntersectionQuery instead")]
		public SharpMap.Data.FeatureDataTable QueryFeatures(SharpMap.Geometries.Geometry geom, double distance)
        {
			throw new NotImplementedException("QueryFeatures is obsolete. Use ExecuteIntersectionQuery.");
        }

        /// <summary>
        /// Creates a new row in the given <see cref="SharpMap.Data.FeatureDataTable"/> <paramref name="dataTable"/>
        /// using data in <see cref="GisSharpBlog.NetTopologySuite.Features.Feature"/> <paramref name="feature"/>.
        /// </summary>
        /// <param name="dataTable">The <see cref="SharpMap.Data.FeatureDataTable"/> to fill.</param>
        /// <param name="feature">Data to insert in the <see cref="SharpMap.Data.FeatureDataTable"/>.</param>
        private void CreateNewRow(SharpMap.Data.FeatureDataTable dataTable, GisSharpBlog.NetTopologySuite.Features.Feature feature)
        {
            SharpMap.Data.FeatureDataRow dataRow = dataTable.NewRow();
            dataRow.Geometry = GeometryConverter.ToSharpMapGeometry(feature.Geometry);
            foreach (string columnName in feature.Attributes.GetNames())
                dataRow[columnName] = feature.Attributes[columnName];
			dataTable.AddRow(dataRow);
        }

        /// <summary>
        /// Creates a <see cref="SharpMap.Data.FeatureDataTable"/> using a stub feature (feature[0]).
        /// </summary>
        /// <returns><see cref="SharpMap.Data.FeatureDataTable"/></returns>
        private SharpMap.Data.FeatureDataTable CreateFeatureDataTable()
        {            
            SharpMap.Data.FeatureDataTable dataTable = new SharpMap.Data.FeatureDataTable();
            foreach (string columnName in features[0].Attributes.GetNames())
                dataTable.Columns.Add(new DataColumn(columnName, features[0].Attributes.GetType(columnName)));
            return dataTable;
        }

        /// <summary>
        /// Gets the connection ID.
        /// </summary>
        /// <value>The connection ID.</value>
        [Obsolete("Does nothing at all")]
        public string ConnectionID
        {
            get 
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Gets the features in view.
        /// </summary>
        /// <param name="bbox">The bbox.</param>
        /// <param name="ds">The ds.</param>
        public void GetFeaturesInView(SharpMap.Geometries.BoundingBox bbox, SharpMap.Data.FeatureDataSet ds)
        {
			ExecuteIntersectionQuery(bbox, ds);
        }

        /// <summary>
        /// Returns the BoundingBox of the dataset.
        /// </summary>
        /// <returns>BoundingBox</returns>
        public SharpMap.Geometries.BoundingBox GetExtents()
        {            
            GisSharpBlog.NetTopologySuite.Geometries.Envelope envelope = new GisSharpBlog.NetTopologySuite.Geometries.Envelope();
            foreach (GisSharpBlog.NetTopologySuite.Features.Feature feature in features)
                envelope.ExpandToInclude(feature.Geometry.EnvelopeInternal);
            return GeometryConverter.ToSharpMapBoundingBox(envelope);
        }

        /// <summary>
        /// Gets the feature identified from the given <paramref name="rowID" />.
        /// </summary>
        /// <param name="rowID">The row ID.</param>
        /// <returns></returns>
        public SharpMap.Data.FeatureDataRow GetFeature(uint rowID)
        {
            GisSharpBlog.NetTopologySuite.Features.Feature feature = features[Convert.ToInt32(rowID)];            
            SharpMap.Data.FeatureDataTable dataTable = new SharpMap.Data.FeatureDataTable();            
            foreach (string columnName in feature.Attributes.GetNames())
                dataTable.Columns.Add(new DataColumn(columnName, feature.Attributes.GetType(columnName)));            
            
            SharpMap.Data.FeatureDataRow dataRow = dataTable.NewRow();
            dataRow.Geometry = GeometryConverter.ToSharpMapGeometry(feature.Geometry);
            foreach (string columnName in feature.Attributes.GetNames())
                dataRow[columnName] = feature.Attributes[columnName];
            return dataRow;
        }

        /// <summary>
        /// Returns the number of features in the dataset.
        /// </summary>
        /// <returns>number of features</returns>
        public int GetFeatureCount()
        {
            return features.Count;
        }

        /// <summary>
        /// Returns features within the specified bounding box.
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns></returns>
        public Collection<SharpMap.Geometries.Geometry> GetGeometriesInView(SharpMap.Geometries.BoundingBox bbox)
        {
            // Identifies all the features within the given BoundingBox
            GisSharpBlog.NetTopologySuite.Geometries.Envelope envelope = GeometryConverter.ToNTSEnvelope(bbox);
            Collection<SharpMap.Geometries.Geometry> geoms = new Collection<SharpMap.Geometries.Geometry>();
            foreach (GisSharpBlog.NetTopologySuite.Features.Feature feature in features)
                if (envelope.Intersects(feature.Geometry.EnvelopeInternal))
                    geoms.Add(GeometryConverter.ToSharpMapGeometry(feature.Geometry));  
            return geoms;        
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="box"></param>
        /// <param name="ds"></param>
		public void ExecuteIntersectionQuery(SharpMap.Geometries.BoundingBox box, FeatureDataSet ds)
		{
			// Identifies all the features within the given BoundingBox
			GisSharpBlog.NetTopologySuite.Geometries.Envelope envelope = GeometryConverter.ToNTSEnvelope(box);
			List<GisSharpBlog.NetTopologySuite.Features.Feature> results = new List<GisSharpBlog.NetTopologySuite.Features.Feature>(features.Count);
			foreach (GisSharpBlog.NetTopologySuite.Features.Feature feature in features)
				if (envelope.Intersects(feature.Geometry.EnvelopeInternal))
					results.Add(feature);

			// Fill DataSet
			SharpMap.Data.FeatureDataTable dataTable = CreateFeatureDataTable();
			foreach (GisSharpBlog.NetTopologySuite.Features.Feature feature in results)
				CreateNewRow(dataTable, feature);
			ds.Tables.Add(dataTable);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geom"></param>
        /// <param name="ds"></param>
        public void ExecuteIntersectionQuery(SharpMap.Geometries.Geometry geom, FeatureDataSet ds)
		{
			GisSharpBlog.NetTopologySuite.Geometries.Geometry geometry = GeometryConverter.ToNTSGeometry(geom, geometryFactory);
			SharpMap.Data.FeatureDataTable dataTable = CreateFeatureDataTable();

			foreach (GisSharpBlog.NetTopologySuite.Features.Feature feature in features)
				if (feature.Geometry.Intersects(geometry))
					CreateNewRow(dataTable, feature);

			ds.Tables.Add(dataTable);
		}

        /// <summary>
        /// Gets the geometry by ID.
        /// </summary>
        /// <param name="oid">The oid.</param>
        /// <returns></returns>
        public SharpMap.Geometries.Geometry GetGeometryByID(uint oid)
        {
            GisSharpBlog.NetTopologySuite.Features.Feature feature = features[Convert.ToInt32(oid)];
            return GeometryConverter.ToSharpMapGeometry(feature.Geometry);
        }

        /// <summary>
        /// Gets the object IDs in the view.
        /// </summary>
        /// <param name="bbox">The bbox.</param>
        /// <returns></returns>
        public Collection<uint> GetObjectIDsInView(SharpMap.Geometries.BoundingBox bbox)
        {
            // Identifies all the features within the given BoundingBox
            GisSharpBlog.NetTopologySuite.Geometries.Envelope envelope = GeometryConverter.ToNTSEnvelope(bbox);
            Collection<uint> geoms = new Collection<uint>();
            for(int i = 0; i < features.Count; i++)            
                if (envelope.Intersects(features[i].Geometry.EnvelopeInternal))
                    geoms.Add(Convert.ToUInt32(i));
            return geoms;                   
        }

        /// <summary>
        /// Opens this instance.
        /// </summary>
        [Obsolete("Does nothing at all")]
        public void Open() { }

        /// <summary>
        /// Gets a value indicating whether this instance is open.
        /// </summary>
        /// <value><c>true</c> if this instance is open; otherwise, <c>false</c>.</value>
        public bool IsOpen
        {
            get
            {
                return features.Count > 0;
            }
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        [Obsolete("Does nothing at all")]
        public void Close() { }

		private int _SRID = -1;

		/// <summary>
		/// The spatial reference ID (CRS)
		/// </summary>
		public int SRID
		{
			get { return _SRID; }
			set { _SRID = value; }
		}

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() { }

        #endregion

	}
}
