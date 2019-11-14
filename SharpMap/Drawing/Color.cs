using System.Collections.Generic;
using System.Reflection;

#if NETSTANDARD2_0
namespace SharpMap.Drawing
{
    public struct Color
    {
        private static readonly Dictionary<KnownColor, int> KnownColorLookup;

        static Color()
        {
            KnownColorLookup = new Dictionary<KnownColor, int>();
            var type = typeof(KnownColor);
            foreach (KnownColor knownColor in System.Enum.GetValues(typeof(KnownColor)))
            {
                var fldInfo = type.GetField(knownColor.ToString());
                var argb = (ArgbValueAttribute) fldInfo.GetCustomAttribute(typeof(ArgbValueAttribute));
                KnownColorLookup.Add(knownColor, argb.Argb);

            }
        }

        internal static System.Drawing.Color FromKnownColor(KnownColor knownColor)
        {
            return System.Drawing.Color.FromArgb(KnownColorLookup[knownColor]);
        }
    }
}
#endif
