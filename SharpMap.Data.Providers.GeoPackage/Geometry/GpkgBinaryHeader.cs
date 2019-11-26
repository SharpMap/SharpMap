using System;
using System.IO;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;

namespace SharpMap.Data.Providers.Geometry
{
    internal class GpkgBinaryHeader
    {
        private const byte IsEmptyFlag = 0x01 << 4;
        private const byte EndianessFlag = 0x01;
        private const byte ExtentFlags = 0x07 << 1;

        internal enum GeoPackageBinaryType : byte
        {
            Standard, Extended
        }
        
        private byte[] _magic = new byte[]{ 0x47, 0x50 };
        private byte _version;
        private byte _flags;
        private int _srs_id;
        private Envelope _extent;
        private Interval _zrange;
        private Interval _mrange;


        public static GpkgBinaryHeader Read(BinaryReader reader)
        {
            var res = new GpkgBinaryHeader();
            res._magic = reader.ReadBytes(2);
            res._version = reader.ReadByte();
            res._flags = reader.ReadByte();

            switch (res.Endianess)
            {
                case 1:
                    ReadSridExtent(reader, res);
                    break;
                case 0:
                    ReadBESridExtent(reader, res);
                    break;
            }

            return res;
        }

        private static void ReadSridExtent(BinaryReader reader, GpkgBinaryHeader header)
        {
            header._srs_id = reader.ReadInt32();
            var ordinates = header.Ordinates;
            if (ordinates == Ordinates.None)
            {
                header._extent = new Envelope(double.MinValue, double.MaxValue, double.MinValue, double.MaxValue);
                header._zrange = Interval.Create(double.MinValue, double.MaxValue);
                header._mrange = Interval.Create(double.MinValue, double.MaxValue);
                return;
            }
            header._extent = new Envelope(reader.ReadDouble(), reader.ReadDouble(),
                reader.ReadDouble(), reader.ReadDouble());
            if ((ordinates & Ordinates.Z) == Ordinates.Z)
                header._zrange = Interval.Create(reader.ReadDouble(), reader.ReadDouble());

            if ((ordinates & Ordinates.M) == Ordinates.M)
                header._mrange = Interval.Create(reader.ReadDouble(), reader.ReadDouble());
        }

        private static void ReadBESridExtent(BinaryReader reader, GpkgBinaryHeader header)
        {
            header._srs_id = SwapByteOrder(reader.ReadInt32());
            var ordinates = header.Ordinates;
            if (ordinates == Ordinates.None)
            {
                header._extent = new Envelope(double.MinValue, double.MaxValue, double.MinValue, double.MaxValue);
                header._zrange = Interval.Create(double.MinValue, double.MaxValue);
                header._mrange = Interval.Create(double.MinValue, double.MaxValue);
                return;
            }
            header._extent = new Envelope(SwapByteOrder(reader.ReadDouble()), SwapByteOrder(reader.ReadDouble()),
                SwapByteOrder(reader.ReadDouble()), SwapByteOrder(reader.ReadDouble()));
            if ((ordinates & Ordinates.Z) == Ordinates.Z)
                header._zrange = Interval.Create(SwapByteOrder(reader.ReadDouble()), SwapByteOrder(reader.ReadDouble()));

            if ((ordinates & Ordinates.M) == Ordinates.M)
                header._mrange = Interval.Create(SwapByteOrder(reader.ReadDouble()), SwapByteOrder(reader.ReadDouble()));
        }

        private static int SwapByteOrder(int val)
        {
            var bytes = BitConverter.GetBytes(val);
            Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        /// <summary>
        /// Method to swap the byte order
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        private static double SwapByteOrder(double val)
        {
            var bytes = BitConverter.GetBytes(val);
            Array.Reverse(bytes);
            return BitConverter.ToDouble(bytes, 0);
        }

        /// <summary>
        /// Gets a value indicating the ordinates
        /// </summary>
        public Ordinates Ordinates
        {
            get
            {
                switch ((_flags & ExtentFlags) >> 1)
                {
                    case 0:
                        return Ordinates.None;
                    case 1:
                        return Ordinates.XY;
                    case 2:
                        return Ordinates.XYZ;
                    case 3:
                        return Ordinates.XYM;
                    case 4:
                        return Ordinates.XYZM;
                }
                throw new GeoPackageException("Invalid extent flags");
            }
        }

        internal int NumOrdinates
        {
            get
            {
                switch ((_flags & ExtentFlags) >> 1)
                {
                    case 0:
                        return 0;
                    case 1:
                        return 2;
                    case 2:
                    case 3:
                        return 3;
                    case 4:
                        return 4;
                }
                throw new GeoPackageException();
            }
        }

        /// <summary>
        /// Gets a value indicating that this geometry is empty
        /// </summary>
        public bool IsEmpty { get { return (_flags & IsEmptyFlag) == IsEmptyFlag; }}

        /// <summary>
        /// Gets a value indicating that this geometry is empty
        /// </summary>
        public int Endianess { get { return (_flags & EndianessFlag); } }

        /// <summary>
        /// Gets the magic number
        /// </summary>
        public byte[] Magic { get { return _magic; }}

        /// <summary>
        /// Gets a value indicating the version of the geometry data
        /// </summary>
        public byte Version { get { return _version; }}

        /// <summary>
        /// Gets a value indicating the spatial reference id
        /// </summary>
        public int SrsId { get { return _srs_id; } }

        public Envelope Extent
        {
            get { return _extent; }
            internal set { _extent = value; }
        }

        public Interval ZRange { get { return _zrange; }}
        public Interval MRange { get { return _mrange; } }
    }
}
