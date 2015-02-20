using System.IO;
using GeoAPI;
using GeoAPI.Geometries;
using NetTopologySuite.IO;
using SharpMap.Data.Providers.Geometry;

namespace SharpMap.Data.Providers.IO
{
    internal class GpkgStandardBinaryReader
    {
        
        private readonly WKBReader _wkbReader;

        internal GpkgStandardBinaryReader()
            :this(GeometryServiceProvider.Instance)
        {
        }

        internal GpkgStandardBinaryReader(IGeometryServices services)
        {
            _wkbReader = new WKBReader(services);
        }

        public bool HandleSRID
        {
            get { return _wkbReader.HandleSRID; }
            set { _wkbReader.HandleSRID = value; }
        }

        public Ordinates AllowedOrdinates
        {
            get { return _wkbReader.AllowedOrdinates; }
        }

        public Ordinates HandleOrdinates
        {
            get { return _wkbReader.HandleOrdinates; }
            set
            {
                _wkbReader.HandleOrdinates = value;
            }
        }

        public GpkgStandardBinary Read(byte[] source)
        {
            using (var ms = new MemoryStream(source))
                return Read(ms);
        }

        public GpkgStandardBinary Read(Stream stream)
        {
            using (var br = new BinaryReader(stream))
            {
                return GpkgStandardBinary.Read(br, _wkbReader);
            }
        }

        public bool RepairRings
        {
            get { return _wkbReader.RepairRings; }
            set { _wkbReader.RepairRings = value; }
        }
    }
}