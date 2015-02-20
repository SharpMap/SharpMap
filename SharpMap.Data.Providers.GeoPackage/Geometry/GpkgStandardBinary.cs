using System.IO;
using GeoAPI.Geometries;
using NetTopologySuite.IO;

namespace SharpMap.Data.Providers.Geometry
{
    /// <summary>
    /// A class implementing the standard geo package binary blob
    /// </summary>
    internal class GpkgStandardBinary
    {
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
            return new GpkgStandardBinary
            {
                Header = GpkgBinaryHeader.Read(reader),
                WellKnownBytes = reader.ReadBytes((int) (reader.BaseStream.Length - reader.BaseStream.Position)),
                _wkbReader = wkbReader
            };
        }

        internal IGeometry GetGeometry()
        {
            var geom = _wkbReader.Read(WellKnownBytes);
            geom.SRID = Header.SrsId;
            GeoAPIEx.SetExtent(geom as NetTopologySuite.Geometries.Geometry, Header.Extent);
            return geom;
        }
    }
}