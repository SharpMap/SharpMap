using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SharpMap.CoordinateSystems
{
    /// <summary>
    /// A coordinate system services class
    /// </summary>
    public class CoordinateSystemServices : ProjNet.CoordinateSystemServices, ICoordinateSystemRepository
    {
        private readonly CoordinateSystemFactory _csFactory;

        /// <summary>
        /// Creates an instance of this class using the provided <paramref name="coordinateSystemFactory"/>, 
        /// <paramref name="coordinateTransformationFactory"/> and enumeration of 
        /// </summary>
        /// <param name="coordinateSystemFactory">The factory to use for creating a coordinate system.</param>
        /// <param name="coordinateTransformationFactory">The factory to use for creating a coordinate transformation.</param>
        public CoordinateSystemServices(
            CoordinateSystemFactory coordinateSystemFactory,
            CoordinateTransformationFactory coordinateTransformationFactory)
            : base(coordinateSystemFactory, coordinateTransformationFactory)
        {
            _csFactory = coordinateSystemFactory ?? throw new ArgumentNullException(nameof(coordinateSystemFactory));
        }

        /// <summary>
        /// Creates an instance of this class using the provided <paramref name="coordinateSystemFactory"/>, 
        /// <paramref name="coordinateTransformationFactory"/> and enumeration of 
        /// </summary>
        /// <param name="coordinateSystemFactory">The factory to use for creating a coordinate system.</param>
        /// <param name="coordinateTransformationFactory">The factory to use for creating a coordinate transformation.</param>
        /// <param name="enumerable">An enumeration if spatial reference ids and coordinate system definition strings pairs</param>
        public CoordinateSystemServices(
            CoordinateSystemFactory coordinateSystemFactory,
            CoordinateTransformationFactory coordinateTransformationFactory,
            IEnumerable<KeyValuePair<int, string>> enumerable)
            : this(coordinateSystemFactory, coordinateTransformationFactory)
        {
            AddCoordinateSystems(enumerable);
        }

        private void AddCoordinateSystems(IEnumerable<KeyValuePair<int, string>> coordinateSystems)
        {
            foreach (var keyPair in coordinateSystems)
            {
                var srid = keyPair.Key;
                var wellKnownText = keyPair.Value;

                var coordinateSystem = CreateCoordinateSystem(wellKnownText);
                if (coordinateSystem != null)
                    AddCoordinateSystem(srid, coordinateSystem);
            }
        }

        /// <summary>
        /// Method to create a coordinate system based on the <paramref name="wellKnownText"/> coordinate system definition.
        /// </summary>
        /// <param name="wellKnownText"></param>
        /// <returns>A coordinate system, <value>null</value> if no coordinate system could be created.</returns>
        public CoordinateSystem CreateCoordinateSystem(string wellKnownText)
        {
            try
            {
                return _csFactory.CreateFromWkt(wellKnownText.Replace("ELLIPSOID", "SPHEROID"));
            }
            catch (Exception)
            {
                // as a fallback we ignore projections not supported
                return null;
            }
        }

        /// <summary>
        /// Gets a value indicating that this coordinate system repository is readonly
        /// </summary>
        public virtual bool IsReadOnly
        {
            get { return false; }
        }

        int ICoordinateSystemRepository.Count => Count;

        bool ICoordinateSystemRepository.IsReadOnly => IsReadOnly;


        void ICoordinateSystemRepository.AddCoordinateSystem(int srid, CoordinateSystem coordinateSystem)
        {
            AddCoordinateSystem(srid, coordinateSystem);
        }

        void ICoordinateSystemRepository.Clear()
        {
            Clear();
        }

        bool ICoordinateSystemRepository.RemoveCoordinateSystem(int srid)
        {
            return RemoveCoordinateSystem(srid);
        }

        IEnumerator<KeyValuePair<int, CoordinateSystem>> IEnumerable<KeyValuePair<int, CoordinateSystem>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
