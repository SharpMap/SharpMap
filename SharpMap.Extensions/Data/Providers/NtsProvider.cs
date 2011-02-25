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
using GisSharpBlog.NetTopologySuite.Features;
using GisSharpBlog.NetTopologySuite.Geometries;
using SharpMap.Converters.NTS;
using SharpMap.Geometries;
using Geometry=SharpMap.Geometries.Geometry;

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
        #region Delegates

        /// <summary>
        /// Defines a geometry operation that will be applied to all geometries in <see cref="NtsProvider"/>.
        /// </summary>
        /// <param name="features"></param>
        public delegate void GeometryOperationDelegate(List<Feature> features);

        #endregion

        #region Fields

        // Factory for NTS features
        private readonly GeometryFactory geometryFactory;

        // NTS features
        private List<Feature> features;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="T:NtsProvider"/> class
        /// using a default <see cref="GisSharpBlog.NetTopologySuite.Geometries.PrecisionModel"/> 
        /// with Floating precision.
        /// </summary>        
        protected internal NtsProvider() : this(new PrecisionModel())
        {
        }

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
        protected internal NtsProvider(PrecisionModel precisionModel)
        {
            geometryFactory = new GeometryFactory(precisionModel);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:NtsProvider"/> class 
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
        public NtsProvider(IProvider provider,
                           PrecisionModel precisionModel) : this(precisionModel)
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
        public NtsProvider(IProvider provider, GeometryOperationDelegate operation) : this(provider)
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
        public NtsProvider(IProvider provider, GeometryOperationDelegate operation,
                           PrecisionModel precisionModel) : this(provider, precisionModel)
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
        private void BuildFromProvider(IProvider provider)
        {
            // Features list initialization
            features = new List<Feature>(provider.GetFeatureCount());

            try
            {
                // Load all features from the given provider
                provider.Open();
                Collection<uint> ids = provider.GetObjectIDsInView(provider.GetExtents());
                foreach (uint id in ids)
                {
                    FeatureDataRow dataRow = provider.GetFeature(id);
                    GeoAPI.Geometries.IGeometry geometry = GeometryConverter.ToNTSGeometry(dataRow.Geometry, geometryFactory);
                    AttributesTable attributes = new AttributesTable();
                    foreach (DataColumn column in dataRow.Table.Columns)
                    {
                        if (dataRow[column] == null || dataRow[column].GetType() == typeof (DBNull))
                            throw new ApplicationException("Null values not supported");
                        attributes.AddAttribute(column.ColumnName, dataRow[column]);
                    }
                    features.Add(new Feature(geometry, attributes));
                }
            }
            finally
            {
                if (provider.IsOpen)
                    provider.Close();
            }
        }

        #endregion

        private int _SRID = -1;

        #region IProvider Members

        /// <summary>
        /// Gets the connection ID.
        /// </summary>
        /// <value>The connection ID.</value>
        [Obsolete("Does nothing at all")]
        public string ConnectionID
        {
            get { return String.Empty; }
        }

        /// <summary>
        /// Returns the BoundingBox of the dataset.
        /// </summary>
        /// <returns>BoundingBox</returns>
        public BoundingBox GetExtents()
        {
            Envelope envelope = new Envelope();
            foreach (Feature feature in features)
                envelope.ExpandToInclude(feature.Geometry.EnvelopeInternal);
            return GeometryConverter.ToSharpMapBoundingBox(envelope);
        }

        /// <summary>
        /// Gets the feature identified from the given <paramref name="rowID" />.
        /// </summary>
        /// <param name="rowID">The row ID.</param>
        /// <returns></returns>
        public FeatureDataRow GetFeature(uint rowID)
        {
            Feature feature = features[Convert.ToInt32(rowID)];
            FeatureDataTable dataTable = new FeatureDataTable();
            foreach (string columnName in feature.Attributes.GetNames())
                dataTable.Columns.Add(new DataColumn(columnName, feature.Attributes.GetType(columnName)));

            FeatureDataRow dataRow = dataTable.NewRow();
            dataRow.Geometry =
                GeometryConverter.ToSharpMapGeometry(
                    feature.Geometry as GisSharpBlog.NetTopologySuite.Geometries.Geometry);
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
        public Collection<Geometry> GetGeometriesInView(BoundingBox bbox)
        {
            // Identifies all the features within the given BoundingBox
            Envelope envelope = GeometryConverter.ToNTSEnvelope(bbox);
            Collection<Geometry> geoms = new Collection<Geometry>();
            foreach (Feature feature in features)
                if (envelope.Intersects(feature.Geometry.EnvelopeInternal))
                    geoms.Add(
                        GeometryConverter.ToSharpMapGeometry(
                            feature.Geometry as GisSharpBlog.NetTopologySuite.Geometries.Geometry));
            return geoms;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="box"></param>
        /// <param name="ds"></param>
        public void ExecuteIntersectionQuery(BoundingBox box, FeatureDataSet ds)
        {
            // Identifies all the features within the given BoundingBox
            Envelope envelope = GeometryConverter.ToNTSEnvelope(box);
            List<Feature> results = new List<Feature>(features.Count);
            foreach (Feature feature in features)
                if (envelope.Intersects(feature.Geometry.EnvelopeInternal))
                    results.Add(feature);

            // Fill DataSet
            FeatureDataTable dataTable = CreateFeatureDataTable();
            foreach (Feature feature in results)
                CreateNewRow(dataTable, feature);
            ds.Tables.Add(dataTable);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geom"></param>
        /// <param name="ds"></param>
        public void ExecuteIntersectionQuery(Geometry geom, FeatureDataSet ds)
        {
            GeoAPI.Geometries.IGeometry geometry = GeometryConverter.ToNTSGeometry(geom, geometryFactory);
            FeatureDataTable dataTable = CreateFeatureDataTable();

            foreach (Feature feature in features)
                if (feature.Geometry.Intersects(geometry))
                    CreateNewRow(dataTable, feature);

            ds.Tables.Add(dataTable);
        }

        /// <summary>
        /// Gets the geometry by ID.
        /// </summary>
        /// <param name="oid">The oid.</param>
        /// <returns></returns>
        public Geometry GetGeometryByID(uint oid)
        {
            Feature feature = features[Convert.ToInt32(oid)];
            return
                GeometryConverter.ToSharpMapGeometry(
                    feature.Geometry as GisSharpBlog.NetTopologySuite.Geometries.Geometry);
        }

        /// <summary>
        /// Gets the object IDs in the view.
        /// </summary>
        /// <param name="bbox">The bbox.</param>
        /// <returns></returns>
        public Collection<uint> GetObjectIDsInView(BoundingBox bbox)
        {
            // Identifies all the features within the given BoundingBox
            Envelope envelope = GeometryConverter.ToNTSEnvelope(bbox);
            Collection<uint> geoms = new Collection<uint>();
            for (int i = 0; i < features.Count; i++)
                if (envelope.Intersects(features[i].Geometry.EnvelopeInternal))
                    geoms.Add(Convert.ToUInt32(i));
            return geoms;
        }

        /// <summary>
        /// Opens this instance.
        /// </summary>
        [Obsolete("Does nothing at all")]
        public void Open()
        {
        }

        /// <summary>
        /// Gets a value indicating whether this instance is open.
        /// </summary>
        /// <value><c>true</c> if this instance is open; otherwise, <c>false</c>.</value>
        public bool IsOpen
        {
            get { return features.Count > 0; }
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        [Obsolete("Does nothing at all")]
        public void Close()
        {
        }

        /// <summary>
        /// The spatial reference ID (CRS)
        /// </summary>
        public int SRID
        {
            get { return _SRID; }
            set { _SRID = value; }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }

        #endregion

        /// <summary>
        /// Creates a new row in the given <see cref="SharpMap.Data.FeatureDataTable"/> <paramref name="dataTable"/>
        /// using data in <see cref="GisSharpBlog.NetTopologySuite.Features.Feature"/> <paramref name="feature"/>.
        /// </summary>
        /// <param name="dataTable">The <see cref="SharpMap.Data.FeatureDataTable"/> to fill.</param>
        /// <param name="feature">Data to insert in the <see cref="SharpMap.Data.FeatureDataTable"/>.</param>
        private void CreateNewRow(FeatureDataTable dataTable, Feature feature)
        {
            FeatureDataRow dataRow = dataTable.NewRow();
            dataRow.Geometry =
                GeometryConverter.ToSharpMapGeometry(
                    feature.Geometry as GisSharpBlog.NetTopologySuite.Geometries.Geometry);
            foreach (string columnName in feature.Attributes.GetNames())
                dataRow[columnName] = feature.Attributes[columnName];
            dataTable.AddRow(dataRow);
        }

        /// <summary>
        /// Creates a <see cref="SharpMap.Data.FeatureDataTable"/> using a stub feature (feature[0]).
        /// </summary>
        /// <returns><see cref="SharpMap.Data.FeatureDataTable"/></returns>
        private FeatureDataTable CreateFeatureDataTable()
        {
            FeatureDataTable dataTable = new FeatureDataTable();
            foreach (string columnName in features[0].Attributes.GetNames())
                dataTable.Columns.Add(new DataColumn(columnName, features[0].Attributes.GetType(columnName)));
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