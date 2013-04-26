using System;
using GeoAPI.Geometries;
using GeoAPI.SpatialReference;

namespace GeoAPI.Features
{
    public interface IGeometryReader
    {
        bool HasRecords { get; }
        bool Read();
        IGeometry Geometry { get; }
    }

    public interface IFeatureReader : IGeometryReader
    {
        IFeatureAttributes Attributes { get; }
        IFeature Feature { get; }
    }

    public interface IFeatureProvider
    {
        ISpatialReference SpatialReference { get; }

        Envelope GetExtents();
        
        IGeometryReader ExecuteGeometryReader(SpatialPredicate predicate, Envelope envelope);
        
        IGeometryReader ExecuteGeometryReader(SpatialPredicate predicate, IGeometry geometry);
        
        IFeatureReader ExecuteFeatureReader(SpatialPredicate predicate, Envelope envelope);
        
        IFeatureReader ExecuteFeatureReader(SpatialPredicate predicate, IGeometry geometry);
    }

    public enum SpatialPredicate
    {
        Intersects,
        Contains,
        Within,
        Overlaps,
        Covers,
        Covered,
        CoveredBy
    }
}