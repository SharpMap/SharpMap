using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SharpMap.Utilities
{
    internal class Utilities
    {
        public static string ReadVersion(BinaryReader reader)
        {
            var bytes = reader.ReadBytes(6);
            return Win1252.Value.GetString(bytes);
        }

        public static Lazy<Encoding> Win1252 { get { return new Lazy<Encoding>( () => Encoding.Default);} }
    }
}
