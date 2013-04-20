using System;
using System.Collections.Generic;
using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Geometries;
using GeoAPI.SpatialReference;
using GeoAPI.Utilities;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

namespace SharpMap.SpatialReference
{
    /// <summary>
    /// 
    /// </summary>
    public class ProjNetReprojector : IReprojectorCore
    {
        private static readonly 
            ThreadSafeStore<KeyValuePair<ICoordinateSystem, ICoordinateSystem>, IMathTransform> Infos =
                new ThreadSafeStore<KeyValuePair<ICoordinateSystem, ICoordinateSystem>, IMathTransform>(CreateTransformation);

        private static readonly ThreadSafeStore<ISpatialReference, ICoordinateSystem> CoordinateSystems =
            new ThreadSafeStore<ISpatialReference, ICoordinateSystem>(CreateCoordinateSystem);

        private static ICoordinateSystem CreateCoordinateSystem(ISpatialReference spatialReference)
        {
            var pisr = spatialReference as ProjNetSpatialReference;
            if (pisr != null && pisr.CoordinateSystem != null)
                return pisr.CoordinateSystem;

            switch (spatialReference.DefinitionType)
            {
                case SpatialReferenceDefinitionType.WellKnownText:
                    var csFactory = new CoordinateSystemFactory();
                    return csFactory.CreateFromWkt(spatialReference.Definition);

                default:
                    throw new NotSupportedException();

            }
        }

        private static IMathTransform CreateTransformation(KeyValuePair<ICoordinateSystem, ICoordinateSystem> fromTo)
        {
            var ctFactory = new CoordinateTransformationFactory();
            return ctFactory.CreateFromCoordinateSystems(fromTo.Key, fromTo.Value).MathTransform;
        }

        private static IMathTransform GetMathTransform(ISpatialReference @from, ISpatialReference to)
        {
            var fromCS = CoordinateSystems.Get(@from);
            var toCS = CoordinateSystems.Get(to);
            return Infos.Get(new KeyValuePair<ICoordinateSystem, ICoordinateSystem>(fromCS, toCS));
        }

        public ProjNetReprojector()
        {
            Factory = new ProjNetSpatialReferenceFactory();
        }

        public Coordinate Reproject(Coordinate coordinate, ISpatialReference @from, ISpatialReference to)
        {
            var transformation = GetMathTransform(@from, to);
            return transformation.Transform(coordinate);
        }


        public Envelope Reproject(Envelope envelope, ISpatialReference @from, ISpatialReference to)
        {
            var transformation = GetMathTransform(@from, to);
            
            var res = new Envelope(transformation.Transform(new Coordinate(envelope.MinX, envelope.MinY)));
            res.ExpandToInclude(transformation.Transform(new Coordinate(envelope.MinX, envelope.MaxY)));
            res.ExpandToInclude(transformation.Transform(new Coordinate(envelope.MaxX, envelope.MaxY)));
            res.ExpandToInclude(transformation.Transform(new Coordinate(envelope.MaxX, envelope.MinY)));
            
            return res;
        }

        public ICoordinateSequence Reproject(ICoordinateSequence sequence, ISpatialReference @from, ISpatialReference to)
        {
            var transformation = GetMathTransform(@from, to);
            return transformation.Transform(sequence);
        }

        public ISpatialReferenceFactory Factory { get; private set; }
    }
}