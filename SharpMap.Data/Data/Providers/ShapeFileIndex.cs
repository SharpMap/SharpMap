using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace SharpMap.Data.Providers
{
    internal class ShapeFileIndex
    {

        #region Shapefile iterator
        private class ShapeFileEnumerator : IEnumerator<ShapeFileIndexEntry>
        {
            private readonly Stream _shpStream;
            private readonly ShapeFileHeader _shpHeader;
            private readonly BinaryReader _shpReader;
            private int _lastOid;

            private ShapeFileIndexEntry _current;
            
            public ShapeFileEnumerator(string shpPath)
                : this(new FileStream(shpPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
            }

            private ShapeFileEnumerator(Stream stream)
            {
                _shpStream = stream;
                var headerBuffer = new byte[100];
                _shpStream.Read(headerBuffer, 0, 100);

                _shpHeader = new ShapeFileHeader(headerBuffer);
                _shpReader = new BinaryReader(_shpStream);
            }

            public void Dispose()
            {
                _shpReader.Dispose();
                _shpStream.Dispose();
            }

            public bool MoveNext()
            {
                if (_shpStream.Position >= _shpHeader.FileLength)
                {
                    _current = new ShapeFileIndexEntry();
                    return false;
                }

                // Get the record offset
                var recordOffset = (int)_shpStream.Position;
                
                // Get the oid
                var oid = ShapeFile.SwapByteOrder(_shpReader.ReadInt32());
                Debug.Assert(oid == _lastOid + 1);
                _lastOid = oid;

                // Get the record length
                var recordLength = 2 * ShapeFile.SwapByteOrder(_shpReader.ReadInt32());
                
                // Set the current ShapeFileIndexEntry
                _current = new ShapeFileIndexEntry(recordOffset, recordLength);
                
                // Adjust the streams position
                _shpStream.Seek(recordLength , SeekOrigin.Current);

                return true;
            }

            public void Reset()
            {
                _shpStream.Seek(100, SeekOrigin.Begin);
                _lastOid = 0;
                _current = new ShapeFileIndexEntry();
            }

            public ShapeFileIndexEntry Current { get { return _current; } }

            object IEnumerator.Current
            {
                get { return Current; }
            }
        }
        #endregion

        /// <summary>
        /// Method to create a SHX index from a given ShapeFile
        /// </summary>
        /// <param name="shpPath">The path to the shapefile</param>
        public static void Create(string shpPath)
        {
            if (string.IsNullOrEmpty(shpPath))
            {
                throw new ArgumentNullException(shpPath);
            }

            if (!File.Exists(shpPath))
            {
                throw new FileNotFoundException("The specified path does not lead to a file", shpPath);
            }
            
            var shxPath = Path.ChangeExtension(shpPath, ".shx");
            if (File.Exists(shxPath))
            {
                File.Delete(shxPath);
            }

            using (var it = new ShapeFileEnumerator(shpPath))
            {
                var fs = new FileStream(shxPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
                fs.Seek(100, SeekOrigin.Begin);
                
                using (var bw = new BinaryWriter(fs))
                {
                    var count = 0;
                    while (it.MoveNext())
                    {
                        count++;
                        bw.Write(ShapeFile.SwapByteOrder(it.Current.Offset / 2));
                        bw.Write(ShapeFile.SwapByteOrder(it.Current.Length / 2));
                    }

                    var length = (int)bw.BaseStream.Position;
                    Debug.Assert(100 + count * 8 == length);
                    bw.BaseStream.Seek(24, SeekOrigin.Begin);
                    bw.Write(ShapeFile.SwapByteOrder(length / 2));
                    bw.BaseStream.Seek(length, SeekOrigin.Begin);

                }
            }
        }

        /// <summary>
        /// A structure that contains a SHX Record
        /// </summary>
        private struct ShapeFileIndexEntry
        {
            /// <summary>
            /// The offset in the file
            /// </summary>
            public readonly int Offset;

            /// <summary>
            /// The length of the geometry buffer
            /// </summary>
            public readonly int Length;

            /// <summary>
            /// Initializes this structure
            /// </summary>
            /// <param name="recordOffset">The offset of the record</param>
            /// <param name="dataLength">The length of the record</param>
            public ShapeFileIndexEntry(int recordOffset, int dataLength)
            {
                Offset = recordOffset;
                Length = dataLength;
            }
        }

        private readonly byte[] _shxBuffer;

        public ShapeFileIndex(string shxPath) 
            : this (new FileStream(shxPath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {}

        public ShapeFileIndex(Stream stream)
        {
            stream.Seek(24, SeekOrigin.Begin);
            var buf = new byte[4];
            stream.Read(buf, 0, 4);
            var bufferSize = 2*ShapeFile.SwapByteOrder(BitConverter.ToInt32(buf, 0)) - 100;
            _shxBuffer = new byte[bufferSize];
            stream.Seek(100, SeekOrigin.Begin);
            stream.Read(_shxBuffer, 0, bufferSize);

            for (var i = 0; i < bufferSize; i+= 4)
            {
                var value = 2*ShapeFile.SwapByteOrder(BitConverter.ToInt32(_shxBuffer, i));
                var tmp = BitConverter.GetBytes(value);
                Buffer.BlockCopy(tmp, 0, _shxBuffer, i, 4);
            }

            FeatureCount = (bufferSize) / 8;
        }

        public int FeatureCount { get; private set; }

        /// <summary>
        /// Gets the offset of the record at index <paramref name="oid"/>.
        /// </summary>
        /// <param name="oid">The (1-based) record index</param>
        /// <returns>The offset of the record</returns>
        public long GetOffset(uint oid)
        {
            //oid--;
            return BitConverter.ToInt32(_shxBuffer, (int)oid * 8);
        }

        /// <summary>
        /// Gets the length of the record at index <paramref name="oid"/>.
        /// </summary>
        /// <param name="oid">The (1-based) record index</param>
        /// <returns>The length of the record</returns>
        public int GetLength(uint oid)
        {
            //oid--;
            return BitConverter.ToInt32(_shxBuffer, 4 + (int)oid * 8);
        }
    }
}