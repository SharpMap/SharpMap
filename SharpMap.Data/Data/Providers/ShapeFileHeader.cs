using System;
using System.IO;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;

namespace SharpMap.Data.Providers
{
    /// <summary>
    /// ShapeFile header class
    /// </summary>
    internal sealed class ShapeFileHeader
    {
        internal const int ShapeFileVersion = 1000;
        internal const double NoDataBorderValue = -10e38;
        internal const double NoDataValue = -10e39;

        /// <summary>
        /// Method to determine if a measure value is to be considered a no data value
        /// </summary>
        /// <param name="measure">The value to test.</param>
        /// <returns><value>true</value> if the value is a no data value</returns>
        public static bool IsNoDataValue(double measure)
        {
            return measure < NoDataBorderValue;
        }

        /// <summary>
        /// Method to set a no data measure value to <see cref="Coordinate.NullOrdinate"/>
        /// </summary>
        /// <param name="measure">The value to check</param>
        /// <returns><paramref name="measure"/> if it is valid, otherwise <see cref="Coordinate.NullOrdinate"/>.</returns>
        public static Double ParseNoDataValue(double measure)
        {
            if (IsNoDataValue(measure))
                return Coordinate.NullOrdinate;
            return measure;
        }

        private readonly Envelope _envelope;
        
        /// <summary>
        /// Creates an instance of this class using the provided <paramref name="header"/> buffer
        /// </summary>
        /// <param name="header">The buffer holding the header information</param>
        public ShapeFileHeader(byte[] header)
            :this(new MemoryStream(header))
        {
        }

        /// <summary>
        /// Creates an instance of this class using the provided <paramref name="header"/> stream
        /// </summary>
        /// <param name="header">The stream holding the header information</param>
        public ShapeFileHeader(Stream header)
            :this(new BinaryReader(header))
        {
        }

        /// <summary>
        /// Creates a shapefile header using the provided <see cref="shpPath"/>
        /// </summary>
        /// <param name="shpPath">The path to the shapefile</param>
        public ShapeFileHeader(string shpPath)
            : this(new FileStream(shpPath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {

        }

        /// <summary>
        /// Creates an instance of this class using the provided <paramref name="headerReader"/>.
        /// </summary>
        /// <param name="headerReader">The stream holding the header information</param>
        public ShapeFileHeader(BinaryReader headerReader)
        {
            // Check file header
            if (headerReader.ReadInt32() != 170328064)
            {
                //File Code is actually 9994, but in Little Endian Byte Order this is '170328064'
                throw (new ApplicationException("Invalid Shapefile (.shp)"));
            }

            // Get file length
            headerReader.BaseStream.Seek(20, SeekOrigin.Current);
            FileLength = 2*ShapeFile.SwapByteOrder(headerReader.ReadInt32());

            // Check file Version
            if (headerReader.ReadInt32() != ShapeFileVersion)
            {
                throw (new ApplicationException("Invalid Shapefile version"));
            }

            // Get the shape type
            ShapeType = (ShapeType) headerReader.ReadInt32();

            // Get the bounding box
            _envelope = new Envelope(new Coordinate(headerReader.ReadDouble(), headerReader.ReadDouble()),
                                     new Coordinate(headerReader.ReadDouble(), headerReader.ReadDouble()));

            // Get the Z-range
            ZRange = Interval.Create(headerReader.ReadDouble(), headerReader.ReadDouble());

            // Get the Z-range
            MRange = Interval.Create(ParseNoDataValue(headerReader.ReadDouble()),
                                     ParseNoDataValue(headerReader.ReadDouble()));
        }

        /// <summary>
        /// Gets the file length
        /// </summary>
        public int FileLength { get; private set; }

        /// <summary>
        /// Gets the shape type encoded in the shape file
        /// </summary>
        public ShapeType ShapeType { get; private set; }

        /// <summary>
        /// Gets the extent of the shape file
        /// </summary>
        public Envelope BoundingBox { get { return new Envelope(_envelope);} }

        /// <summary>
        /// Gets the range of Z-Values in the shape file, if there are any
        /// </summary>
        public Interval ZRange { get; private set; }

        /// <summary>
        /// Gets the range of M-values in the shape file, if there are any
        /// </summary>
        public Interval MRange { get; private set; }
    }
}