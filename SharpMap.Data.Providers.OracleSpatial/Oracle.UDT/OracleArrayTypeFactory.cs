using Oracle.DataAccess.Types;
using System;

#pragma warning disable 1591

namespace SharpMap.Data.Providers.OracleUDT
{
    public abstract class OracleArrayTypeFactoryBase<T> : IOracleArrayTypeFactory
    {
        public Array CreateArray(int numElems)
        {
            return new T[numElems];
        }

        public Array CreateStatusArray(int numElems)
        {
            return new OracleUdtStatus[numElems];
        }
    }
}