using System.ComponentModel.Design;
using System.Linq;
using GeoAPI;
using GeoAPI.Geometries;
using Oracle.DataAccess.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpMap.Data.Providers.OracleUDT
{
    [Serializable]
    public static class SdoGeometryTypes
    {
        /// <summary>
        /// Oracle Documentation for SDO_ETYPE - SIMPLE
        /// Point//Line//Polygon//exterior counterclockwise - polygon ring = 1003//interior clockwise  polygon ring = 2003
        /// </summary>
        public enum EtypeSimple { Point = 1, Line = 2, Polygon = 3, PolygonExterior = 1003, PolygonInterior = 2003 }
       
        /// <summary>
        ///  Oracle Documentation for SDO_ETYPE - COMPOUND
        /// 1005: exterior polygon ring (must be specified in counterclockwise order)
        /// 2005: interior polygon ring (must be specified in clockwise order)
        /// </summary>
        public enum EtypeCompound { FourDigit = 4, PolygonExterior = 1005, PolygonInterior = 2005 }
        
        /// <summary>
        /// Oracle Documentation for SDO_GTYPE.
        /// This represents the last two digits in a GTYPE, where the first item is dimension(ality) and the second is LRS
        /// </summary>
        public enum Gtype { UnknownGeometry = 00, Point = 01, Line = 02, Curve = 02, Polygon = 03, Collection = 04, Multipoint = 05, Multiline = 06, Multicurve = 06, Multipolygon = 07 }
        public enum Dimension { Dim2D = 2, Dim3D = 3, LRSDim3 = 3, LRSDim4 = 4 }
    }
    [Serializable]
    [OracleCustomTypeMapping("MDSYS.SDO_GEOMETRY")]
    public class SdoGeometry : OracleCustomTypeBase<SdoGeometry>
    {
        private enum OracleObjectColumns { SDO_GTYPE, SDO_SRID, SDO_POINT, SDO_ELEM_INFO, SDO_ORDINATES }

        private decimal? _sdoGtype;
        [OracleObjectMapping(0)]
        public decimal? SdoGtype
        {
            get { return _sdoGtype; }
            set { _sdoGtype = value;}
        }
        public int SdoGtypeAsInt
        {
            get { return Convert.ToInt32(SdoGtype); }
        }

        private decimal? _sdoSRID;
        [OracleObjectMapping(1)]
        public decimal? SdoSRID
        {
            get { return _sdoSRID; }
            set { _sdoSRID = value; }
        }
        public int SdoSRIDAsInt
        {
            get { return Convert.ToInt32(SdoSRID); }
            set { SdoSRID = Convert.ToDecimal(value); }
        }

        private SDOPOINT _sdopoint;
        [OracleObjectMapping(2)]
        public SDOPOINT SdoPoint
        {
            get { return _sdopoint; }
            set { _sdopoint = value; }
        }

        private decimal[] _elemArray;
        [OracleObjectMapping(3)]
        public decimal[] ElemArray
        {
            get { return _elemArray; }
            set { _elemArray = value; }
        }

        private decimal[] _ordinatesArray;
        [OracleObjectMapping(4)]
        public decimal[] OrdinatesArray
        {
            get { return _ordinatesArray; }
            set { _ordinatesArray = value; }
        }

        [OracleCustomTypeMapping("MDSYS.SDO_ELEM_INFO_ARRAY")]
        public class ElemArrayFactory : OracleArrayTypeFactoryBase<decimal> { }

        [OracleCustomTypeMapping("MDSYS.SDO_ORDINATE_ARRAY")]
        public class OrdinatesArrayFactory : OracleArrayTypeFactoryBase<decimal> { }

        public override void MapFromCustomObject()
        {
            SetValue((int)OracleObjectColumns.SDO_GTYPE, SdoGtype);
            SetValue((int)OracleObjectColumns.SDO_SRID, SdoSRID);
            SetValue((int)OracleObjectColumns.SDO_POINT, SdoPoint);
            SetValue((int)OracleObjectColumns.SDO_ELEM_INFO, ElemArray);
            SetValue((int)OracleObjectColumns.SDO_ORDINATES, OrdinatesArray);
        }

        public override void MapToCustomObject()
        {
            SdoGtype = GetValue<decimal?>((int)OracleObjectColumns.SDO_GTYPE);
            SdoSRID = GetValue<decimal?>((int)OracleObjectColumns.SDO_SRID);
            SdoPoint = GetValue<SDOPOINT>((int)OracleObjectColumns.SDO_POINT);
            ElemArray = GetValue<decimal[]>((int)OracleObjectColumns.SDO_ELEM_INFO);
            OrdinatesArray = GetValue<decimal[]>((int)OracleObjectColumns.SDO_ORDINATES);
        }

        public int[] ElemArrayOfInts
        {
            get
            {
                int[] elems = null;
                if (_elemArray != null)
                {
                    elems = new int[_elemArray.Length];
                    for (int k = 0; k < _elemArray.Length; k++)
                    {
                        elems[k] = Convert.ToInt32(_elemArray[k]);
                    }
                }
                return elems;
            }
            set
            {
                if (value != null)
                {
                    int c = value.GetLength(0);
                    _elemArray = new decimal[c];
                    for (int k = 0; k < c; k++)
                    {
                        _elemArray[k] = Convert.ToDecimal(value[k]);
                    }
                }
                else
                {
                    _elemArray = null;
                }
            }
        }
        public double[] OrdinatesArrayOfDoubles
        {
            get
            {
                double[] elems = null;
                if (OrdinatesArray != null)
                {
                    elems = new double[_ordinatesArray.Length];
                    for (int k = 0; k < _ordinatesArray.Length; k++)
                    {
                        elems[k] = Convert.ToDouble(_ordinatesArray[k]);
                    }
                }
                return elems;
            }
            set
            {
                if (value != null)
                {
                    int c = value.GetLength(0);
                    _ordinatesArray = new decimal[c];
                    for (int k = 0; k < c; k++)
                    {
                        _ordinatesArray[k] = Convert.ToDecimal(value[k]);
                    }
                }
                else
                {
                    _ordinatesArray = null;
                }
            }
        }
        private int _dimensionality;
        public int Dimensionality
        {
            get { return _dimensionality; }
            set { _dimensionality = value; }
        }
        private int _lrs;
        public int LRS
        {
            get { return _lrs; }
            set { _lrs = value; }
        }
        private int _geometryType;
        public int GeometryType
        {
            get { return _geometryType; }
            set { _geometryType = value; }
        }
        public int PropertiesFromGtype()
        {
            if (_sdoGtype != null && (int)_sdoGtype != 0)
            {
                var v = (int)_sdoGtype;
                int dim = v / 1000;
                Dimensionality = dim;
                v -= dim * 1000;
                int lrsDim = v / 100;
                LRS = lrsDim;
                v -= lrsDim * 100;
                GeometryType = v;
                return (Dimensionality * 1000) + (LRS * 100) + GeometryType;
            }
            return 0;
        }

        public int PropertiesToGtype()
        {
            int v = Dimensionality * 1000;
            v = v + (LRS * 100);
            v = v + GeometryType;
            _sdoGtype = Convert.ToDecimal(v);
            return (v);
        }

        /// <summary>
        /// Get geometry as GeoAPI IGeometry
        /// </summary>
        /// <returns></returns>
        public IGeometry AsGeometry()
        {
            if (!SdoGtype.HasValue)
                return null;

            int dimension = SdoGtypeAsInt / 1000;
            int gType = SdoGtypeAsInt%100;

            if (gType == (int) SdoGeometryTypes.EtypeSimple.Point && SdoPoint != null)
            {
                return
                    GeometryServiceProvider.Instance.CreateGeometryFactory()
                        .CreatePoint(new Coordinate(Convert.ToDouble(SdoPoint.X), Convert.ToDouble(SdoPoint.Y)));
            }
            
            if (_ordinatesArray != null)
            {
                var coords = new List<Coordinate>();

                

                for (int i = 0; i < _ordinatesArray.Length; i += dimension)
                {
                    coords.Add(new Coordinate(Convert.ToDouble(_ordinatesArray[i]), Convert.ToDouble(_ordinatesArray[i + 1])));
                }


                if (_elemArray[1] == 2) //Line
                {
                    return GeometryServiceProvider.Instance.CreateGeometryFactory().CreateLineString(coords.ToArray());
                }

                if (_elemArray[1] == 1003)
                {
                    if (_elemArray[2] == 1)
                    {
                        //Check that the polygon is self-enclosing, else fix
                        var seq = GeometryServiceProvider.Instance.DefaultCoordinateSequenceFactory.Create(coords.ToArray());
                        
                        coords.EnsureValidRing();
                        return GeometryServiceProvider.Instance.CreateGeometryFactory().CreatePolygon(seq);
                    }

                    if (_elemArray[2] == 3)
                    {
                        //Only two coords LL and UR in coordlist, add others
                        coords.Add(new Coordinate(coords[1].X, coords[0].Y));
                        coords.Add(coords[0]);
                        coords.Insert(1, new Coordinate(coords[0].X, coords[1].Y));

                        return GeometryServiceProvider.Instance.CreateGeometryFactory().CreatePolygon(coords.ToArray());
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Return as Text
        /// </summary>
        public string AsText
        {
            get
            {
                var sb = new StringBuilder();
                sb.Append("MDSYS.SDO_GEOMETRY(");
                sb.Append((SdoGtype != null) ? SdoGtype.ToString() : "null");
                sb.Append(",");
                sb.Append((SdoSRID != null) ? SdoSRID.ToString() : "null");
                sb.Append(",");
                // begin point
                if (SdoPoint != null)
                {
                    sb.Append("MDSYS.SDO_POINT_TYPE(");
                    string tmp = string.Format("{0:#.##########},{1:#.##########}{2}{3:#.##########}",
                                                    SdoPoint.X,
                                                    SdoPoint.Y,
                                                    (SdoPoint.Z == null) ? null : ",",
                                                    SdoPoint.Z);
                    
                    sb.Append(tmp.Trim());
                    sb.Append(")");
                }
                else
                {
                    sb.Append("null");
                }
                sb.Append(",");
                // begin element array
                if (_elemArray != null)
                {
                    sb.Append("MDSYS.SDO_ELEM_INFO_ARRAY(");
                    for (int i = 0; i < _elemArray.Length; i++)
                    {
                        string tmp = string.Format("{0}", _elemArray[i]);
                        sb.Append(tmp);
                        if (i < (_elemArray.Length - 1))
                            sb.Append(",");
                    }
                    sb.Append(")");
                }
                else
                {
                    sb.Append("null");
                }
                sb.Append(",");
                // begin ordinates array
                if (_ordinatesArray != null)
                {
                    sb.Append("MDSYS.SDO_ORDINATE_ARRAY(");
                    for (int i = 0; i < _ordinatesArray.Length; i++)
                    {
                        string tmp = string.Format("{0:#.##########}", _ordinatesArray[i]);
                        sb.Append(tmp);
                        if (i < (_ordinatesArray.Length - 1))
                            sb.Append(",");
                    }
                    sb.Append(")");
                }
                else
                {
                    sb.Append("null");
                }
                sb.Append(")");
                return sb.ToString();
            }
        }
        public override string ToString()
        {
            return AsText;
        }
    }
}