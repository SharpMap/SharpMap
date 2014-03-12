using System;
using Oracle.DataAccess.Types;
using Oracle.DataAccess.Client;

namespace SharpMap.Data.Providers.OracleUDT
{   
    [Serializable]
    public abstract class OracleCustomTypeBase<T> : INullable, IOracleCustomType, IOracleCustomTypeFactory
    where T : OracleCustomTypeBase<T>, new()
    {
        private static string errorMessageHead = "Error converting Oracle User Defined Type to .Net Type " + typeof(T) + ", oracle column is null, failed to map to . NET valuetype, column ";
        [NonSerialized]
        private OracleConnection _connection;
        private IntPtr _pUdt;
        private bool _isNull;

        public virtual bool IsNull
        {
            get
            {
                return _isNull;
            }
        }

        public static T Null
        {
            get
            {
                var t = new T();
                t._isNull = true;
                return t;
            }
        }

        public IOracleCustomType CreateObject()
        {
            return new T();
        }

        protected void SetConnectionAndPointer(OracleConnection connection, IntPtr pUdt)
        {
            _connection = connection;
            _pUdt = pUdt;
        }

        public abstract void MapFromCustomObject();
        public abstract void MapToCustomObject();

        public void FromCustomObject(OracleConnection con, IntPtr pUdt)
        {
            SetConnectionAndPointer(con, pUdt);
            MapFromCustomObject();
        }
        public void ToCustomObject(OracleConnection con, IntPtr pUdt)
        {
            SetConnectionAndPointer(con, pUdt);
            MapToCustomObject();
        }

        protected void SetValue(string oracleColumnName, object value)
        {
            if (value != null)
            {
                OracleUdt.SetValue(_connection, _pUdt, oracleColumnName, value);
            }
        }
        protected void SetValue(int oracleColumnId, object value)
        {
            if (value != null)
            {
                OracleUdt.SetValue(_connection, _pUdt, oracleColumnId, value);
            }
        }

        protected U GetValue<U>(string oracleColumnName)
        {
            if (OracleUdt.IsDBNull(_connection, _pUdt, oracleColumnName))
            {
                if (default(U) is ValueType)
                {
                    throw new Exception(errorMessageHead + oracleColumnName + " of value type " + typeof(U));
                }
                return default(U);
            }
            return (U)OracleUdt.GetValue(_connection, _pUdt, oracleColumnName);
        }

        protected U GetValue<U>(int oracleColumnId)
        {
            if (OracleUdt.IsDBNull(_connection, _pUdt, oracleColumnId))
            {
                if (default(U) is ValueType)
                {
                    throw new Exception(errorMessageHead + oracleColumnId + " of value type " + typeof(U));
                }
                return default(U);
            }
            return (U)OracleUdt.GetValue(_connection, _pUdt, oracleColumnId);
        }
    }
}
