using System;
using System.Runtime.Serialization;

namespace SharpMap.Data.Providers
{
    [Serializable]
    public class GeoPackageException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public GeoPackageException()
        {
        }

        public GeoPackageException(string message) : base(message)
        {
        }

        public GeoPackageException(string message, Exception inner) : base(message, inner)
        {
        }

        protected GeoPackageException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}