using System;

namespace SharpMap.Data.Providers.ODMatrix
{
    [Flags]
    public enum ODMatrixVector
    {
        None = 0,
        Origin = 1,
        Destination = 2,
        Both = Origin | Destination,
    }
}