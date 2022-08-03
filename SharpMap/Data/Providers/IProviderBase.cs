using NetTopologySuite.Geometries;
using System;
using System.Collections.ObjectModel;

namespace SharpMap.Data.Providers
{
    /// <summary>
    /// Base interface for providers
    /// </summary>
    public interface IBaseProvider : IDisposable
    {
        /// <summary>
        /// Gets the connection ID of the datasource
        /// </summary>
        /// <remarks>
        /// <para>The ConnectionID should be unique to the datasource (for instance the filename or the
        /// connectionstring), and is meant to be used for connection pooling.</para>
        /// <para>If connection pooling doesn't apply to this datasource, the ConnectionID should return String.Empty</para>
        /// </remarks>
        string ConnectionID { get; }

        /// <summary>
        /// Returns true if the datasource is currently open
        /// </summary>
        bool IsOpen { get; }

        /// <summary>
        /// The spatial reference ID (CRS)
        /// </summary>
        int SRID { get; set; }

        /// <summary>
        /// Gets the features within the specified <see cref="NetTopologySuite.Geometries.Envelope"/>
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns>Features within the specified <see cref="NetTopologySuite.Geometries.Envelope"/></returns>
        Collection<Geometry> GetGeometriesInView(Envelope bbox);

        /// <summary>
        /// Returns the data associated with all the geometries that are intersected by 'geom'
        /// </summary>
        /// <param name="geom">Geometry to intersect with</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        void ExecuteIntersectionQuery(Geometry geom, FeatureDataSet ds);

        /// <summary>
        /// Returns the data associated with all the geometries that are intersected by 'geom'
        /// </summary>
        /// <param name="box">Geometry to intersect with</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        void ExecuteIntersectionQuery(Envelope box, FeatureDataSet ds);

        /// <summary>
        /// Returns the number of features in the dataset
        /// </summary>
        /// <returns>number of features</returns>
        int GetFeatureCount();

        /// <summary>
        /// <see cref="Envelope"/> of dataset
        /// </summary>
        /// <returns>The 2d extent of the layer</returns>
        Envelope GetExtents();

        /// <summary>
        /// Opens the datasource
        /// </summary>
        void Open();

        /// <summary>
        /// Closes the datasource
        /// </summary>
        void Close();
    }
}
