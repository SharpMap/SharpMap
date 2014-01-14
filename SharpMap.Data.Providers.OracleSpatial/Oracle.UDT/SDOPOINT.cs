using Oracle.DataAccess.Types;
using System;

namespace SharpMap.Data.Providers.OracleUDT
{
    [Serializable]
    [OracleCustomTypeMappingAttribute("MDSYS.SDO_POINT_TYPE")]
    public class SDOPOINT : OracleCustomTypeBase<SDOPOINT>
    {
        private decimal? _x;
        [OracleObjectMappingAttribute("X")]
        public decimal? X
        {
            get { return _x; }
            set { _x = value; }
        }
        public double? XD
        {
            get { return Convert.ToDouble(_x); }
            set { _x = Convert.ToDecimal(value); }
        }


        private decimal? _y;
        [OracleObjectMappingAttribute("Y")]
        public decimal? Y
        {
            get { return _y; }
            set { _y = value; }
        }
        public double? YD
        {
            get { return Convert.ToDouble(_y); }
            set { _y = Convert.ToDecimal(value); }
        }


        private decimal? _z;
        [OracleObjectMappingAttribute("Z")]
        public decimal? Z
        {
            get { return _z; }
            set { _z = value; }
        }
        public double? ZD
        {
            get { return Convert.ToDouble(_z); }
            set { _z = Convert.ToDecimal(value); }
        }


        public override void MapFromCustomObject()
        {
            SetValue("X", _x);
            SetValue("Y", _y);
            SetValue("Z", _z);
        }

        public override void MapToCustomObject()
        {
            X = GetValue<decimal?>("X");
            Y = GetValue<decimal?>("Y");
            Z = GetValue<decimal?>("Z");
        }
    }
}
