﻿using System;
using GeoAPI;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

namespace SharpMap.CoordinateSystems
{
    /// <summary>
    /// A SharpMap Session class
    /// </summary>
    public class Session : ISession
    {
        static Session()
        {
            Instance = new Session();
        }

        public static ISession Instance { get; private set; }

        private static ICoordinateSystemRepository _repository;
        private ICoordinateSystemServices _coordinateSystemServices;

        public IGeometryServices GeometryServices { get; set; }

        public ICoordinateSystemServices CoordinateSystemServices
        {
            get
            {
                return _coordinateSystemServices ??
                       (_coordinateSystemServices = CoordinateSystems.CoordinateSystemServices.FromSpatialRefSys(
                           new CoordinateSystemFactory(),
                           new CoordinateTransformationFactory()));
            }
            set { _coordinateSystemServices = value; }
        }

        public ICoordinateSystemRepository CoordinateSystemRepository
        {
            get { return _repository ?? CoordinateSystemServices as ICoordinateSystemRepository; }
            set { _repository = value; }
        }

        public ISession SetGeometryServices(IGeometryServices geometryServices)
        {
            GeometryServices = geometryServices;
            return this;
        }

        public ISession SetCoordinateSystemServices(ICoordinateSystemServices coordinateSystemServices)
        {
            CoordinateSystemServices = coordinateSystemServices;
            return this;
        }

        public ISession SetCoordinateSystemRepository(ICoordinateSystemRepository coordinateSystemRepository)
        {
            CoordinateSystemRepository = coordinateSystemRepository;
            return this;
        }

        public ISession ReadConfiguration()
        {
            throw new NotSupportedException();
        }
    }
}