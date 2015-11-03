using System.IO;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using NetTopologySuite.IO;

namespace SharpMap.Data.Providers.Geometry
{
    /// <summary>
    /// A class implementing the standard geo package binary blob
    /// </summary>
    internal class GpkgStandardBinary
    {
        private static readonly  Interval _full = Interval.Create(double.MinValue, double.MaxValue);
        
        private WKBReader _wkbReader;
        public GpkgBinaryHeader Header;
        public byte[] WellKnownBytes;

        /// <summary>
        /// Method to read
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="wkbReader"></param>
        /// <returns></returns>
        public static GpkgStandardBinary Read(BinaryReader reader, WKBReader wkbReader)
        {
            var res = new GpkgStandardBinary
            {
                Header = GpkgBinaryHeader.Read(reader),
                WellKnownBytes = reader.ReadBytes((int) (reader.BaseStream.Length - reader.BaseStream.Position)),
                _wkbReader = wkbReader
            };

            return res;
        }

        /// <summary>
        /// Method to get the geometry
        /// </summary>
        /// <returns>
        /// The geometry
        /// </returns>
        internal IGeometry GetGeometry()
        {
            var geom = _wkbReader.Read(WellKnownBytes);
            geom.SRID = Header.SrsId;
            if (Header.MRange != _full)
                GeoAPIEx.SetExtent(geom as NetTopologySuite.Geometries.Geometry, Header.Extent);
            return geom;
        }
    }
}