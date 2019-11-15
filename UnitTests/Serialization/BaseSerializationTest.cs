using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using SharpMap.Utilities;

namespace UnitTests.Serialization
{
    public abstract class BaseSerializationTest
    {
        protected static T SandD<T>(T input, IFormatter formatter)
        {
            using (var ms = new MemoryStream())
            {
                formatter.Serialize(ms, input);
                ms.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(ms);
            }
        }

        protected static IFormatter GetFormatter()
        {
            var formatter = new BinaryFormatter();
            if (formatter.SurrogateSelector == null)
                formatter.SurrogateSelector = new SurrogateSelector();
            formatter.SurrogateSelector.ChainSelector(SharpMap.Utilities.Surrogates.GetSurrogateSelectors());
            Utility.AddBruTileSurrogates(formatter);
            return formatter;
        }
    }
}