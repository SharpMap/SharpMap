// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

// Note:
// Good stuff on DBase format: http://www.clicketyclick.dk/databases/xbase/format/

using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using SharpMap.Utilities.Indexing;

namespace SharpMap.Data.Providers
{
    /// <summary>
    /// Straight forward Dbase file reader
    /// </summary>
    public class DbaseReader : IDisposable
    {
        /// <summary>
        /// Event raised when the <see cref="IncludeOid"/> property has changed
        /// </summary>
        public event EventHandler IncludeOidChanged;

        /// <summary>
        /// Event raised when the <see cref="Encoding"/> property has changed
        /// </summary>
        public event EventHandler EncodingChanged;
        
        private struct DbaseField
        {
            public int Address;
            public string ColumnName;
            //public Type DataType;
            public TypeCode DataTypeCode;
            public int Decimals;
            public int Length;
        }

        private DateTime _lastUpdate;
        private int _numberOfRecords;
        private Int16 _headerLength;
        private Int16 _recordLength;
        private readonly string _filename;
        private DbaseField[] _dbaseColumns;
        
        private Stream _dbfStream;
        //private BinaryReader br;
        private bool _headerIsParsed;

#if USE_MEMORYMAPPED_FILE
        private static readonly System.Collections.Generic.Dictionary<string,System.IO.MemoryMappedFiles.MemoryMappedFile> MemMappedFiles;
        private static readonly System.Collections.Generic.Dictionary<string, int> MemMappedFilesRefConter;
        
        private bool _haveRegistredForUsage;
        
        static DbaseReader()
        {
            MemMappedFiles = new System.Collections.Generic.Dictionary<string, System.IO.MemoryMappedFiles.MemoryMappedFile>();
            MemMappedFilesRefConter = new System.Collections.Generic.Dictionary<string, int>();
        }
#endif


        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="filename">The shapefile to open</param>
        /// <exception cref="FileNotFoundException">Thrown if the file is not present.</exception>
        public DbaseReader(string filename)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException(String.Format("Could not find file \"{0}\"", filename));
            _filename = filename;
            _headerIsParsed = false;
            _includeOid = true;
        }

        /// <summary>
        /// Gets a value indicating whether the Stream to the Dbase file is open or not.
        /// </summary>
        public bool IsOpen { get; private set; }

        /// <summary>
        /// Opens the dbase stream
        /// </summary>
        public void Open()
        {
#if USE_MEMORYMAPPED_FILE
            if (!MemMappedFiles.ContainsKey(_filename))
            {
                System.IO.MemoryMappedFiles.MemoryMappedFile memMappedFile = System.IO.MemoryMappedFiles.MemoryMappedFile.CreateFromFile(_filename, FileMode.Open);
                MemMappedFiles.Add(_filename, memMappedFile);
            }

            if (!_haveRegistredForUsage)
            {
                if (MemMappedFilesRefConter.ContainsKey(_filename))
                    MemMappedFilesRefConter[_filename]++;
                else
                    MemMappedFilesRefConter.Add(_filename, 1);

                _haveRegistredForUsage = true;
            }

            _dbfStream = MemMappedFiles[_filename].CreateViewStream();
#else
            _dbfStream = new FileStream(_filename, FileMode.Open, FileAccess.Read);
#endif
            //br = new BinaryReader(_dbfStream);
            IsOpen = true;
            if (!_headerIsParsed) //Don't read the header if it's already parsed
                ParseDbfHeader(_filename);
        }

//        private Stream GetStream()
//        {
//#if USE_MEMORYMAPPED_FILE
//            return MemMappedFiles[_filename].CreateViewStream();
//#else
//            return new FileStream(_filename, FileMode.Open, FileAccess.Read);
//#endif
//        }

        /// <summary>
        /// Closes the dbase file stream
        /// </summary>
        public void Close()
        {

            //br.Close();
            _dbfStream.Close();
            IsOpen = false;
        }


        /// <summary>
        /// Method to perform cleanup work for unmanaged resources
        /// </summary>
        public void Dispose()
        {
            if (IsOpen)
                Close();
            //br = null;
            _dbfStream = null;
#if USE_MEMORYMAPPED_FILE
            if (MemMappedFilesRefConter.ContainsKey(_filename))
            {
                MemMappedFilesRefConter[_filename]--;
                if (MemMappedFilesRefConter[_filename] <= 0)
                {
                    MemMappedFiles[_filename].Dispose();
                    MemMappedFiles.Remove(_filename);
                    MemMappedFilesRefConter.Remove(_filename);
                }
            }
#endif
        }

        // **** 
        // **** 
        // **** 
        // **** ToDo Evaluate if anyone is using this?
        // **** 
        // **** 
        // **** 
        // ****
        // Binary Tree not working yet on Mono 
        // see bug: http://bugzilla.ximian.com/show_bug.cgi?id=78502
#if !MONO
        /// <summary>
        /// Indexes a DBF column in a binary tree [NOT COMPLETE]
        /// </summary>
        /// <typeparam name="T">datatype to be indexed</typeparam>
        /// <param name="columnId">Column to index</param>
        /// <returns></returns>
        public BinaryTree<T, UInt32> CreateDbfIndex<T>(int columnId) where T : IComparable<T>
        {
            var tree = new BinaryTree<T, uint>();
            for (uint i = 0; i < ((_numberOfRecords > 10000) ? 10000 : _numberOfRecords); i++)
                tree.Add(new BinaryTree<T, uint>.ItemValue((T) GetValue(i, columnId), i));
            return tree;
        }
#endif
        /*
		/// <summary>
		/// Creates an index on the columns for faster searching [EXPERIMENTAL - Requires Lucene dependencies]
		/// </summary>
		/// <returns></returns>
		public string CreateLuceneIndex()
		{
			string dir = this._filename + ".idx";
			if (!System.IO.Directory.Exists(dir))
				System.IO.Directory.CreateDirectory(dir);
			Lucene.Net.Index.IndexWriter iw = new Lucene.Net.Index.IndexWriter(dir,new Lucene.Net.Analysis.Standard.StandardAnalyzer(),true);

			for (uint i = 0; i < this._NumberOfRecords; i++)
			{
				FeatureDataRow dr = GetFeature(i,this.NewTable);
				Lucene.Net.Documents.Document doc = new Lucene.Net.Documents.Document();
				// Add the object-id as a field, so that index can be maintained.
				// This field is not stored with document, it is indexed, but it is not
	            // tokenized prior to indexing.
				//doc.Add(Lucene.Net.Documents.Field.UnIndexed("SharpMap_oid", i.ToString())); //Add OID index

				foreach(System.Data.DataColumn col in dr.Table.Columns) //Add and index values from DBF
				{
					if(col.DataType.Equals(typeof(string)))
						// Add the contents as a valued Text field so it will get tokenized and indexed.
						doc.Add(Lucene.Net.Documents.Field.UnStored(col.ColumnName,(string)dr[col]));
					else
						doc.Add(Lucene.Net.Documents.Field.UnStored(col.ColumnName, dr[col].ToString()));
				}
				iw.AddDocument(doc);
			}
			iw.Optimize();
			iw.Close();
			return this._filename + ".idx";
		}
		*/

        /// <summary>
        /// Gets the date this file was last updated.
        /// </summary>
        public DateTime LastUpdate
        {
            get { return _lastUpdate; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oid"></param>
        /// <returns></returns>
        /// <exception cref="ApplicationException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public Boolean RecordDeleted(uint oid)
        {
            CurrentRecordOid = oid;
            return _currentRecordBuffer[0] == '*';
            /*
            if (!_isOpen)
                throw (new ApplicationException("An attempt was made to read from a closed DBF file"));
            if (oid >= _numberOfRecords)
                throw (new ArgumentException("Invalid record requested at index " + oid));
            
            using (var s = GetStream())
            using (var br = new BinaryReader(s))
            {
                s.Seek(_headerLength + oid*_recordLength, 0);
                return br.ReadChar() == '*';
                //return BitConverter.ToChar(new[] {(byte) s.ReadByte()}, 0) == '*';
            }
             */
        }

        /// <summary>
        /// Gets or sets the current object id
        /// </summary>
        public uint CurrentRecordOid
        {
            get { return _currentRecordOid; }
            set
            {
                if (!IsOpen)
                    throw (new ApplicationException("An attempt was made to read from a closed DBF file"));

                if (_currentRecordOid == value)
                    return;

                if (value >= _numberOfRecords)
                    throw (new ArgumentException("Invalid record requested at index " + value));

                _dbfStream.Seek(_headerLength + value * _recordLength, 0);
                _dbfStream.Read(_currentRecordBuffer, 0, _recordLength);
                _currentRecordOid = value;
            }
        }

        private void ParseDbfHeader(string filename)
        {
            var buffer32 = new byte[32];
            _dbfStream.Seek(0, SeekOrigin.Begin);
            _dbfStream.Read(buffer32, 0, 32);

            if (buffer32[0] != 0x03)
                throw new NotSupportedException("Unsupported DBF Type");
            
            //Get last modified date
            _lastUpdate = new DateTime(buffer32[1] + 1900, buffer32[2], buffer32[3]);

            //Get number of records
            _numberOfRecords = BitConverter.ToInt32(buffer32, 4);

            //Get the header length
            _headerLength = BitConverter.ToInt16(buffer32, 8);

            //Get the record length
            _recordLength = BitConverter.ToInt16(buffer32, 10);

            //Get and parse the language driver code
            _fileEncoding = GetDbaseLanguageDriver(buffer32[29], filename);

            var numberOfColumns = (_headerLength - 31) / 32; // calculate the number of DataColumns in the header
            _dbaseColumns = new DbaseField[numberOfColumns];
            for (var i = 0; i < numberOfColumns; i++)
            {
                _dbfStream.Read(buffer32, 0, 32);
                using (var br = new BinaryReader(new MemoryStream(buffer32)))
                {
                    _dbaseColumns[i] = new DbaseField();
                    _dbaseColumns[i].ColumnName = Encoding.UTF7.GetString((br.ReadBytes(11))).Replace("\0", "").Trim();
                    var fieldtype = br.ReadChar();
                    switch (fieldtype)
                    {
                        case 'L':
                            //_dbaseColumns[i].DataType = typeof (bool);
                            _dbaseColumns[i].DataTypeCode = TypeCode.Boolean;
                            break;
                        case 'C':
                            //_dbaseColumns[i].DataType = typeof (string);
                            _dbaseColumns[i].DataTypeCode = TypeCode.String;
                            break;
                        case 'D':
                            //_dbaseColumns[i].DataType = typeof (DateTime);
                            _dbaseColumns[i].DataTypeCode = TypeCode.DateTime;
                            break;
                        case 'N':
                            //_dbaseColumns[i].DataType = typeof (double);
                            _dbaseColumns[i].DataTypeCode = TypeCode.Double;
                            break;
                        case 'F':
                            //_dbaseColumns[i].DataType = typeof (float);
                            _dbaseColumns[i].DataTypeCode = TypeCode.Single;
                            break;
                        case 'B':
                            //_dbaseColumns[i].DataType = typeof (byte[]);
                            _dbaseColumns[i].DataTypeCode = TypeCode.Object; //Hack for
                            break;
                        default:
                            throw (new NotSupportedException("Invalid or unknown DBase field type '" + fieldtype +
                                                             "' in column '" + _dbaseColumns[i].ColumnName + "'"));
                    }
                    _dbaseColumns[i].Address = br.ReadInt32();
                    if (i > 0) _dbaseColumns[i].Address = _dbaseColumns[i - 1].Address + _dbaseColumns[i - 1].Length;

                    var length = (int)br.ReadByte();
                    if (length < 0) length = length + 256;
                    _dbaseColumns[i].Length = length;
                    _dbaseColumns[i].Decimals = br.ReadByte();


                    //If the double-type doesn't have any decimals, make the type an integer
                    //if (_dbaseColumns[i].Decimals == 0 && _dbaseColumns[i].DataType == typeof (double))
                    //    if (_dbaseColumns[i].Length <= 2)
                    //        _dbaseColumns[i].DataType = typeof (Int16);
                    //    else if (_dbaseColumns[i].Length <= 4)
                    //        _dbaseColumns[i].DataType = typeof (Int32);
                    //    else
                    //        _dbaseColumns[i].DataType = typeof (Int64);
                    if (_dbaseColumns[i].Decimals == 0 && _dbaseColumns[i].DataTypeCode == TypeCode.Double)
                    {
                        if (_dbaseColumns[i].Length < 3) //Range [-9, 99]
                            _dbaseColumns[i].DataTypeCode = TypeCode.Byte;
                        else if (_dbaseColumns[i].Length < 5) //Range [-999, 9999]
                            _dbaseColumns[i].DataTypeCode = TypeCode.Int16;
                        else if (_dbaseColumns[i].Length < 10)
                            _dbaseColumns[i].DataTypeCode = TypeCode.Int32;
                        else if (_dbaseColumns[i].Length < 19)
                            _dbaseColumns[i].DataTypeCode = TypeCode.Int64;
                        else
                            _dbaseColumns[i].DataTypeCode = TypeCode.Double;
                    }
                }
            }

            //using (var s = GetStream())
            //using(var br = new BinaryReader(s))
            //{
            //    if (br.ReadByte() != 0x03)
            //        throw new NotSupportedException("Unsupported DBF Type");

            //    _lastUpdate = new DateTime(br.ReadByte() + 1900, br.ReadByte(), br.ReadByte());
            //    //Read the last update date
            //    _numberOfRecords = br.ReadInt32(); // read number of records.
            //    _headerLength = br.ReadInt16(); // read length of header structure.
            //    _recordLength = br.ReadInt16(); // read length of a record
            //    s.Seek(29, SeekOrigin.Begin); //Seek to encoding flag
            //    _fileEncoding = GetDbaseLanguageDriver(br.ReadByte(), filename); //Read and parse Language driver
            //    s.Seek(32, SeekOrigin.Begin); //Move past the reserved bytes

            //    var numberOfColumns = (_headerLength - 31)/32; // calculate the number of DataColumns in the header
            //    _dbaseColumns = new DbaseField[numberOfColumns];
            //    for (var i = 0; i < _dbaseColumns.Length; i++)
            //    {
            //        _dbaseColumns[i] = new DbaseField();
            //        _dbaseColumns[i].ColumnName = Encoding.UTF7.GetString((br.ReadBytes(11))).Replace("\0", "").Trim();
            //        var fieldtype = br.ReadChar();
            //        switch (fieldtype)
            //        {
            //            case 'L':
            //                //_dbaseColumns[i].DataType = typeof (bool);
            //                _dbaseColumns[i].DataTypeCode = TypeCode.Boolean;
            //                break;
            //            case 'C':
            //                //_dbaseColumns[i].DataType = typeof (string);
            //                _dbaseColumns[i].DataTypeCode = TypeCode.String;
            //                break;
            //            case 'D':
            //                //_dbaseColumns[i].DataType = typeof (DateTime);
            //                _dbaseColumns[i].DataTypeCode = TypeCode.DateTime;
            //                break;
            //            case 'N':
            //                //_dbaseColumns[i].DataType = typeof (double);
            //                _dbaseColumns[i].DataTypeCode = TypeCode.Double;
            //                break;
            //            case 'F':
            //                //_dbaseColumns[i].DataType = typeof (float);
            //                _dbaseColumns[i].DataTypeCode = TypeCode.Single;
            //                break;
            //            case 'B':
            //                //_dbaseColumns[i].DataType = typeof (byte[]);
            //                _dbaseColumns[i].DataTypeCode = TypeCode.Object; //Hack for
            //                break;
            //            default:
            //                throw (new NotSupportedException("Invalid or unknown DBase field type '" + fieldtype +
            //                                                 "' in column '" + _dbaseColumns[i].ColumnName + "'"));
            //        }
            //        _dbaseColumns[i].Address = br.ReadInt32();
            //        if (i > 0) _dbaseColumns[i].Address = _dbaseColumns[i - 1].Address + _dbaseColumns[i - 1].Length;

            //        var length = (int) br.ReadByte();
            //        if (length < 0) length = length + 256;
            //        _dbaseColumns[i].Length = length;
            //        _dbaseColumns[i].Decimals = br.ReadByte();


            //        //If the double-type doesn't have any decimals, make the type an integer
            //        //if (_dbaseColumns[i].Decimals == 0 && _dbaseColumns[i].DataType == typeof (double))
            //        //    if (_dbaseColumns[i].Length <= 2)
            //        //        _dbaseColumns[i].DataType = typeof (Int16);
            //        //    else if (_dbaseColumns[i].Length <= 4)
            //        //        _dbaseColumns[i].DataType = typeof (Int32);
            //        //    else
            //        //        _dbaseColumns[i].DataType = typeof (Int64);
            //        if (_dbaseColumns[i].Decimals == 0 && _dbaseColumns[i].DataTypeCode == TypeCode.Double)
            //        {
            //            if (_dbaseColumns[i].Length < 3) //Range [-9, 99]
            //                _dbaseColumns[i].DataTypeCode = TypeCode.Byte;
            //            else if (_dbaseColumns[i].Length < 5) //Range [-999, 9999]
            //                _dbaseColumns[i].DataTypeCode = TypeCode.Int16;
            //            else if (_dbaseColumns[i].Length < 10) 
            //                _dbaseColumns[i].DataTypeCode = TypeCode.Int32;
            //            else if (_dbaseColumns[i].Length < 19)
            //                _dbaseColumns[i].DataTypeCode = TypeCode.Int64;
            //            else
            //                _dbaseColumns[i].DataTypeCode = TypeCode.double;
            //        }

            //        br.BaseStream.Seek(s.Position + 14, 0);
            //    }
            //}

            _currentRecordBuffer = new byte[_recordLength];
            _headerIsParsed = true;
            
            CreateBaseTable();
        }

        private static Encoding GetDbaseLanguageDriver(byte dbasecode)
        {
            return GetDbaseLanguageDriver(dbasecode, null);
        }
        private static Encoding GetDbaseLanguageDriver(byte dbasecode, string fileName)
        {
            switch (dbasecode)
            {
                case 0x01:
                    return Encoding.GetEncoding(437); //DOS USA code page 437 
                case 0x02:
                    return Encoding.GetEncoding(850); // DOS Multilingual code page 850 
                case 0x03:
                    return Encoding.GetEncoding(1252); // Windows ANSI code page 1252 
                case 0x04:
                    return Encoding.GetEncoding(10000); // Standard Macintosh 
                case 0x08:
                    return Encoding.GetEncoding(865); // Danish OEM
                case 0x09:
                    return Encoding.GetEncoding(437); // Dutch OEM
                case 0x0A:
                    return Encoding.GetEncoding(850); // Dutch OEM Secondary codepage
                case 0x0B:
                    return Encoding.GetEncoding(437); // Finnish OEM
                case 0x0D:
                    return Encoding.GetEncoding(437); // French OEM
                case 0x0E:
                    return Encoding.GetEncoding(850); // French OEM Secondary codepage
                case 0x0F:
                    return Encoding.GetEncoding(437); // German OEM
                case 0x10:
                    return Encoding.GetEncoding(850); // German OEM Secondary codepage
                case 0x11:
                    return Encoding.GetEncoding(437); // Italian OEM
                case 0x12:
                    return Encoding.GetEncoding(850); // Italian OEM Secondary codepage
                case 0x13:
                    return Encoding.GetEncoding(932); // Japanese Shift-JIS
                case 0x14:
                    return Encoding.GetEncoding(850); // Spanish OEM secondary codepage
                case 0x15:
                    return Encoding.GetEncoding(437); // Swedish OEM
                case 0x16:
                    return Encoding.GetEncoding(850); // Swedish OEM secondary codepage
                case 0x17:
                    return Encoding.GetEncoding(865); // Norwegian OEM
                case 0x18:
                    return Encoding.GetEncoding(437); // Spanish OEM
                case 0x19:
                    return Encoding.GetEncoding(437); // English OEM (Britain)
                case 0x1A:
                    return Encoding.GetEncoding(850); // English OEM (Britain) secondary codepage
                case 0x1B:
                    return Encoding.GetEncoding(437); // English OEM (U.S.)
                case 0x1C:
                    return Encoding.GetEncoding(863); // French OEM (Canada)
                case 0x1D:
                    return Encoding.GetEncoding(850); // French OEM secondary codepage
                case 0x1F:
                    return Encoding.GetEncoding(852); // Czech OEM
                case 0x22:
                    return Encoding.GetEncoding(852); // Hungarian OEM
                case 0x23:
                    return Encoding.GetEncoding(852); // Polish OEM
                case 0x24:
                    return Encoding.GetEncoding(860); // Portuguese OEM
                case 0x25:
                    return Encoding.GetEncoding(850); // Portuguese OEM secondary codepage
                case 0x26:
                    return Encoding.GetEncoding(866); // Russian OEM
                case 0x37:
                    return Encoding.GetEncoding(850); // English OEM (U.S.) secondary codepage
                case 0x40:
                    return Encoding.GetEncoding(852); // Romanian OEM
                case 0x4D:
                    return Encoding.GetEncoding(936); // Chinese GBK (PRC)
                case 0x4E:
                    return Encoding.GetEncoding(949); // Korean (ANSI/OEM)
                case 0x4F:
                    return Encoding.GetEncoding(950); // Chinese Big5 (Taiwan)
                case 0x50:
                    return Encoding.GetEncoding(874); // Thai (ANSI/OEM)
                case 0x57:
                    return Encoding.GetEncoding(1252); // ANSI
                case 0x58:
                    return Encoding.GetEncoding(1252); // Western European ANSI
                case 0x59:
                    return Encoding.GetEncoding(1252); // Spanish ANSI
                case 0x64:
                    return Encoding.GetEncoding(852); // Eastern European MS–DOS
                case 0x65:
                    return Encoding.GetEncoding(866); // Russian MS–DOS
                case 0x66:
                    return Encoding.GetEncoding(865); // Nordic MS–DOS
                case 0x67:
                    return Encoding.GetEncoding(861); // Icelandic MS–DOS
                case 0x68:
                    return Encoding.GetEncoding(895); // Kamenicky (Czech) MS-DOS 
                case 0x69:
                    return Encoding.GetEncoding(620); // Mazovia (Polish) MS-DOS 
                case 0x6A:
                    return Encoding.GetEncoding(737); // Greek MS–DOS (437G)
                case 0x6B:
                    return Encoding.GetEncoding(857); // Turkish MS–DOS
                case 0x6C:
                    return Encoding.GetEncoding(863); // French–Canadian MS–DOS
                case 0x78:
                    return Encoding.GetEncoding(950); // Taiwan Big 5
                case 0x79:
                    return Encoding.GetEncoding(949); // Hangul (Wansung)
                case 0x7A:
                    return Encoding.GetEncoding(936); // PRC GBK
                case 0x7B:
                    return Encoding.GetEncoding(932); // Japanese Shift-JIS
                case 0x7C:
                    return Encoding.GetEncoding(874); // Thai Windows/MS–DOS
                case 0x7D:
                    return Encoding.GetEncoding(1255); // Hebrew Windows 
                case 0x7E:
                    return Encoding.GetEncoding(1256); // Arabic Windows 
                case 0x86:
                    return Encoding.GetEncoding(737); // Greek OEM
                case 0x87:
                    return Encoding.GetEncoding(852); // Slovenian OEM
                case 0x88:
                    return Encoding.GetEncoding(857); // Turkish OEM
                case 0x96:
                    return Encoding.GetEncoding(10007); // Russian Macintosh 
                case 0x97:
                    return Encoding.GetEncoding(10029); // Eastern European Macintosh 
                case 0x98:
                    return Encoding.GetEncoding(10006); // Greek Macintosh 
                case 0xC8:
                    return Encoding.GetEncoding(1250); // Eastern European Windows
                case 0xC9:
                    return Encoding.GetEncoding(1251); // Russian Windows
                case 0xCA:
                    return Encoding.GetEncoding(1254); // Turkish Windows
                case 0xCB:
                    return Encoding.GetEncoding(1253); // Greek Windows
                case 0xCC:
                    return Encoding.GetEncoding(1257); // Baltic Windows
                default:
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        fileName = Path.ChangeExtension(fileName, "cpg");
                        if (!File.Exists(fileName)) 
                            fileName = Path.ChangeExtension(fileName, "cst");
                        if (File.Exists(fileName))
                        {
                            var encoding = File.ReadAllText(fileName);
                            try
                            {
                                return Encoding.GetEncoding(encoding);
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                    return Encoding.UTF8;
            }
        }

        /// <summary>
        /// Returns a DataTable that describes the column metadata of the DBase file.
        /// </summary>
        /// <returns>A DataTable that describes the column metadata.</returns>
        public DataTable GetSchemaTable()
        {
            var tab = new DataTable();
            // all of common, non "base-table" fields implemented
            tab.Columns.Add("ColumnName", typeof (String));
            tab.Columns.Add("ColumnSize", typeof (Int32));
            tab.Columns.Add("ColumnOrdinal", typeof (Int32));
            tab.Columns.Add("NumericPrecision", typeof (Int16));
            tab.Columns.Add("NumericScale", typeof (Int16));
            tab.Columns.Add("DataType", typeof (Type));
            tab.Columns.Add("AllowDBNull", typeof (bool));
            tab.Columns.Add("IsReadOnly", typeof (bool));
            tab.Columns.Add("IsUnique", typeof (bool));
            tab.Columns.Add("IsRowVersion", typeof (bool));
            tab.Columns.Add("IsKey", typeof (bool));
            tab.Columns.Add("IsAutoIncrement", typeof (bool));
            tab.Columns.Add("IsLong", typeof (bool));
            
            //Why do we need to add the dbase columns?
            //foreach (DbaseField dbf in _dbaseColumns)
            //    tab.Columns.Add(dbf.ColumnName, Type. dbf.DataType);

            var offset = 0;
            if (IncludeOid)
            {
                var r = tab.NewRow();
                r["ColumnName"] = "Oid";
                r["ColumnSize"] = 4;
                r["ColumnOrdinal"] = 0;
                r["NumericPrecision"] = DBNull.Value;
                r["NumericScale"] = DBNull.Value;
                r["DataType"] = typeof(uint);
                r["AllowDBNull"] = false;
                r["IsReadOnly"] = true;
                r["IsUnique"] = true;
                r["IsRowVersion"] = false;
                r["IsKey"] = true;
                r["IsAutoIncrement"] = false;
                r["IsLong"] = false;

                tab.Rows.Add(r);
                offset = 1;
            }

            for (var i = 0; i < _dbaseColumns.Length; i++)
            {
                var r = tab.NewRow();
                r["ColumnName"] = _dbaseColumns[i].ColumnName;
                r["ColumnSize"] = _dbaseColumns[i].Length;
                r["ColumnOrdinal"] = offset++;
                r["NumericPrecision"] = _dbaseColumns[i].Decimals;
                r["NumericScale"] = 0;
                r["DataType"] = TypeByTypeCode(_dbaseColumns[i].DataTypeCode);
                r["AllowDBNull"] = true;
                r["IsReadOnly"] = true;
                r["IsUnique"] = false;
                r["IsRowVersion"] = false;
                r["IsKey"] = false;
                r["IsAutoIncrement"] = false;
                r["IsLong"] = false;

                tab.Rows.Add(r);
            }

            return tab;
        }

        private static Type TypeByTypeCode(TypeCode dataTypeCode)
        {
            switch (dataTypeCode)
            {
                case TypeCode.Byte:
                    return typeof (byte);
                case TypeCode.Boolean:
                    return typeof (bool);
                case TypeCode.Int16:
                    return typeof (short);
                case TypeCode.Int32:
                    return typeof (int);
                case TypeCode.Int64:
                    return typeof (long);
                case TypeCode.Single:
                    return typeof (float);
                case TypeCode.Double:
                    return typeof (double);
                case TypeCode.String:
                    return typeof (string);
                case TypeCode.DateTime:
                    return typeof (DateTime);
                case TypeCode.Object: // HACK
                    return typeof(byte[]);
                default:
                    throw new InvalidOperationException("TypeCode '" + dataTypeCode + "' has no matched by Dbase.");
            }
        }


        private FeatureDataTable _baseTable;
        private bool _includeOid;

        private void CreateBaseTable()
        {
            _baseTable = new FeatureDataTable();
            if (IncludeOid)
                _baseTable.Columns.Add("Oid", typeof(UInt32));

            foreach (var dbf in _dbaseColumns)
            {
                //_baseTable.Columns.Add(dbf.ColumnName, dbf.DataType);
                int suffix = 0;
                string colname = suffix > 0 ? string.Format("{0}_{1}", dbf.ColumnName, suffix) : dbf.ColumnName;
                while (_baseTable.Columns.Contains(colname))
                {
                    suffix++;
                    colname = suffix > 0 ? string.Format("{0}_{1}", dbf.ColumnName, suffix) : dbf.ColumnName;
                }
                _baseTable.Columns.Add(colname, TypeByTypeCode(dbf.DataTypeCode));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the object's id should be included in attribute data or not. <para>The default value is <c>false</c></para>
        /// </summary>
        public bool IncludeOid
        {
            get { return _includeOid; }
            set
            {
                if (value != _includeOid)
                {
                    _includeOid = value;
                    OnIncludeOidChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Event invoker for <see cref="IncludeOidChanged"/> event.
        /// </summary>
        /// <remarks>When overridden, make sure to call <c>base.OnIncludeOidChanged</c> in order to make sure that subscribers are notified.</remarks>
        /// <param name="e">The event's arguments</param>
        protected virtual void OnIncludeOidChanged(EventArgs e)
        {
            CreateBaseTable();

            if (IncludeOidChanged != null)
                IncludeOidChanged(this, e);
        }

        /// <summary>
        /// Gets an empty table that matches the dbase structure
        /// </summary>
        public FeatureDataTable NewTable
        {
            get { return _baseTable.Clone(); }
        }


        private byte[] _currentRecordBuffer;
        private uint _currentRecordOid = 0xffffffff;

        internal object GetValue(uint oid, int colid)
        {
            CurrentRecordOid = oid;
            return ReadDbfValue(_dbaseColumns[colid]);
        }

        private Encoding _encoding;
        private Encoding _fileEncoding;

        /// <summary>
        /// Gets or sets the <see cref="System.Text.Encoding"/> used for parsing strings from the DBase DBF file.
        /// </summary>
        /// <remarks>
        /// If the encoding type isn't set, the dbase driver will try to determine the correct <see cref="System.Text.Encoding"/>.
        /// </remarks>
        public Encoding Encoding
        {
            get { return _encoding; }
            set
            {
                //Test if encoding is the same as the one gathered by GetLanguageDriverId
                if (value == _fileEncoding)
                    value = null;

                //Since objects are not the same instances, try comparison
                if (value != null && value.Equals(_fileEncoding))
                    value = null;

                if (value != _encoding)
                {
                    _encoding = value;
                    OnEncodingChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Event invoker for <see cref="EncodingChanged"/> event.
        /// </summary>
        /// <remarks>When overridden, make sure to call <c>base.OnEncodingChanged</c> in order to make sure that subscribers are notified.</remarks>
        /// <param name="e">The event's arguments</param>
        protected virtual void OnEncodingChanged(EventArgs e)
        {
            if (EncodingChanged != null)
                EncodingChanged(this, e);
        }


        /// <summary>
        /// Gets the feature at the specified Object ID
        /// </summary>
        /// <param name="oid">the object Id</param>
        /// <param name="table">the table to add the feature to</param>
        /// <returns></returns>
        internal FeatureDataRow GetFeature(uint oid, FeatureDataTable table)
        {
            if (RecordDeleted(oid))
                return null;

            var dr = table.NewRow();

            if (IncludeOid) dr["Oid"] = oid;

            for (var i = 0; i < _dbaseColumns.Length; i++)
            {
                var dbf = _dbaseColumns[i];
                dr[dbf.ColumnName] = GetValue(oid, i);
            }
            return dr;
        }

        private static readonly NumberFormatInfo Nfi = NumberFormatInfo.InvariantInfo;
        
        private object ReadDbfValue(DbaseField dbf)
        {
            var tmpBuffer = new byte[dbf.Length];
            Buffer.BlockCopy(_currentRecordBuffer, dbf.Address+1, tmpBuffer, 0, dbf.Length);

            string temp;

            switch (dbf.DataTypeCode)
            {
                case TypeCode.String:
                    return _encoding == null
                        ? _fileEncoding.GetString(tmpBuffer).Replace("\0", "").Trim()
                        : _encoding.GetString(tmpBuffer).Replace("\0", "").Trim();
                
                case TypeCode.Double:
                    temp = Encoding.UTF7.GetString(tmpBuffer).Replace("\0", "").Trim();
                    double dbl;
                    if (double.TryParse(temp, NumberStyles.Float, Nfi, out dbl))
                        return dbl;
                    return DBNull.Value;

                case TypeCode.Byte:
                    temp = Encoding.UTF7.GetString(tmpBuffer).Replace("\0", "").Trim();
                    Byte i8;
                    if (Byte.TryParse(temp, NumberStyles.Integer, Nfi, out i8))
                        return i8;
                    return DBNull.Value;

                case TypeCode.Int16:
                    temp = Encoding.UTF7.GetString(tmpBuffer).Replace("\0", "").Trim();
                    Int16 i16;
                    if (Int16.TryParse(temp, NumberStyles.Integer, Nfi, out i16))
                        return i16;
                    return DBNull.Value;
                
                case TypeCode.Int32:
                    temp = Encoding.UTF7.GetString(tmpBuffer).Replace("\0", "").Trim();
                    Int32 i32;
                    if (Int32.TryParse(temp, NumberStyles.Integer, Nfi, out i32))
                        return i32;
                    return DBNull.Value;
                
                case TypeCode.Int64:
                    temp = Encoding.UTF7.GetString(tmpBuffer).Replace("\0", "").Trim();
                    Int64 i64;
                    if (Int64.TryParse(temp, NumberStyles.Integer, Nfi, out i64))
                        return i64;
                    return DBNull.Value;
                
                //case "System.Single":
                case TypeCode.Single:
                    temp = Encoding.UTF8.GetString(tmpBuffer);
                    float f;
                    if (float.TryParse(temp, NumberStyles.Float, Nfi, out f))
                        return f;
                    return DBNull.Value;
                
                //case "System.Boolean":
                case TypeCode.Boolean:
                    var tempChar = BitConverter.ToChar(tmpBuffer, 0);
                    return ((tempChar == 'T') || (tempChar == 't') || (tempChar == 'Y') || (tempChar == 'y'));
                
                //case "System.DateTime":
                case TypeCode.DateTime:
                    DateTime date;
                    // Mono has not yet implemented DateTime.TryParseExact
#if !MONO
                    if (DateTime.TryParseExact(Encoding.UTF7.GetString(tmpBuffer),
                                               "yyyyMMdd", Nfi, DateTimeStyles.None, out date))
                        return date;
                    return DBNull.Value;
#else
					try 
					{
						return date = DateTime.ParseExact ( System.Text.Encoding.UTF7.GetString(tmpBuffer), 	
						"yyyyMMdd", Nfi, System.Globalization.DateTimeStyles.None );
					}
					catch ( Exception e )
					{
						return DBNull.Value;
					}
#endif
                default:
                    throw (new NotSupportedException("Cannot parse DBase field '" + dbf.ColumnName + "' of type '" +
//                                                     dbf.DataType + "'"));
                                                     TypeByTypeCode(dbf.DataTypeCode) + "'"));
            }

//            switch (dbf.DataTypeCode)
//            {
//                case TypeCode.String:
//                    return _encoding == null
//                        ? _fileEncoding.GetString(br.ReadBytes(dbf.Length)).Replace("\0", "").Trim()
//                        : _encoding.GetString(br.ReadBytes(dbf.Length)).Replace("\0", "").Trim();
//                case TypeCode.Double:
//                    temp = Encoding.UTF7.GetString(br.ReadBytes(dbf.Length)).Replace("\0", "").Trim();
//                    double dbl;
//                    if (double.TryParse(temp, NumberStyles.Float, Nfi, out dbl))
//                        return dbl;
//                    return DBNull.Value;
//                case TypeCode.Int16:
//                    temp = Encoding.UTF7.GetString((br.ReadBytes(dbf.Length))).Replace("\0", "").Trim();
//                    Int16 i16;
//                    if (Int16.TryParse(temp, NumberStyles.Integer, Nfi, out i16))
//                        return i16;
//                    return DBNull.Value;
//                case TypeCode.Int32:
//                    temp = Encoding.UTF7.GetString((br.ReadBytes(dbf.Length))).Replace("\0", "").Trim();
//                    Int32 i32;
//                    if (Int32.TryParse(temp, NumberStyles.Integer, Nfi, out i32))
//                        return i32;
//                    return DBNull.Value;
//                case TypeCode.Int64:
//                    temp = Encoding.UTF7.GetString((br.ReadBytes(dbf.Length))).Replace("\0", "").Trim();
//                    Int64 i64;
//                    if (Int64.TryParse(temp, NumberStyles.Integer, Nfi, out i64))
//                        return i64;
//                    return DBNull.Value;
//                //case "System.Single":
//                case TypeCode.Single:
//                    temp = Encoding.UTF8.GetString((br.ReadBytes(dbf.Length)));
//                    float f;
//                    if (float.TryParse(temp, NumberStyles.Float, Nfi, out f))
//                        return f;
//                    return DBNull.Value;
//                //case "System.Boolean":
//                case TypeCode.Boolean:
//                    var tempChar = br.ReadChar();
//                    return ((tempChar == 'T') || (tempChar == 't') || (tempChar == 'Y') || (tempChar == 'y'));
//                //case "System.DateTime":
//                case TypeCode.DateTime:
//                    DateTime date;
//                    // Mono has not yet implemented DateTime.TryParseExact
//#if !MONO
//                    if (DateTime.TryParseExact(Encoding.UTF7.GetString((br.ReadBytes(8))),
//                                               "yyyyMMdd", Nfi, DateTimeStyles.None, out date))
//                        return date;
//                    return DBNull.Value;
//#else
//                    try 
//                    {
//                        return date = DateTime.ParseExact ( System.Text.Encoding.UTF7.GetString((br.ReadBytes(8))), 	
//                        "yyyyMMdd", SharpMap.Map.numberFormat_EnUS, System.Globalization.DateTimeStyles.None );
//                    }
//                    catch ( Exception e )
//                    {
//                        return DBNull.Value;
//                    }
//#endif
//                default:
//                    throw (new NotSupportedException("Cannot parse DBase field '" + dbf.ColumnName + "' of type '" +
//                        //                                                     dbf.DataType + "'"));
//                                                     TypeByTypeCode(dbf.DataTypeCode) + "'"));
//            }

        }

        /// <summary>
        /// Gets all attribute values for data record <paramref name="rowid"/>
        /// </summary>
        /// <returns></returns>
        public object[] GetValues(uint rowid)
        {
            object[] result;
            var offset = 0;

            //Preprare result buffer
            if (IncludeOid)
            {
                offset = 1;
                result = new object[1 + _dbaseColumns.Length];
                result[0] = rowid;
            }
            else
            {
                result = new object[_dbaseColumns.Length];
            }

            //Fill result buffer
            if (!RecordDeleted(rowid))
            {
                for (var i = 0; i < _dbaseColumns.Length; i++)
                    result[i + offset] = GetValue(rowid, i);
            }

            return result;
        }
    }
}