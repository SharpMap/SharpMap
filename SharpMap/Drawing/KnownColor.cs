
#if NETSTANDARD2_0

namespace SharpMap.Drawing
{

    /// <summary>
    /// Attribute class to associate ARGB value with <see cref="KnownColor" enum member
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field)]
    internal class ArgbValueAttribute : System.Attribute
    {
        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="argb">The ARGB value</param>
        public ArgbValueAttribute(int argb)
        {
            Argb = argb;
        }

        /// <summary>
        /// Gets a value indicating the ARGB value
        /// </summary>
        public int Argb { get; }
    }

    /// <summary>
    /// Straight copy of <see cref="System.Drawing.KnownColor"/> names
    /// </summary>
    internal enum KnownColor
    {
        /// <summary>
        /// Color ActiveBorder (#B4B4B4)
        /// </summary>
        [ArgbValue(-4934476)] ActiveBorder = 1,

        /// <summary>
        /// Color ActiveCaption (#99B4D1)
        /// </summary>
        [ArgbValue(-6703919)] ActiveCaption = 2,

        /// <summary>
        /// Color ActiveCaptionText (#0)
        /// </summary>
        [ArgbValue(-16777216)] ActiveCaptionText = 3,

        /// <summary>
        /// Color AppWorkspace (#ABABAB)
        /// </summary>
        [ArgbValue(-5526613)] AppWorkspace = 4,

        /// <summary>
        /// Color Control (#F0F0F0)
        /// </summary>
        [ArgbValue(-986896)] Control = 5,

        /// <summary>
        /// Color ControlDark (#A0A0A0)
        /// </summary>
        [ArgbValue(-6250336)] ControlDark = 6,

        /// <summary>
        /// Color ControlDarkDark (#696969)
        /// </summary>
        [ArgbValue(-9868951)] ControlDarkDark = 7,

        /// <summary>
        /// Color ControlLight (#E3E3E3)
        /// </summary>
        [ArgbValue(-1842205)] ControlLight = 8,

        /// <summary>
        /// Color ControlLightLight (#FFFFFF)
        /// </summary>
        [ArgbValue(-1)] ControlLightLight = 9,

        /// <summary>
        /// Color ControlText (#0)
        /// </summary>
        [ArgbValue(-16777216)] ControlText = 10,

        /// <summary>
        /// Color Desktop (#0)
        /// </summary>
        [ArgbValue(-16777216)] Desktop = 11,

        /// <summary>
        /// Color GrayText (#6D6D6D)
        /// </summary>
        [ArgbValue(-9605779)] GrayText = 12,

        /// <summary>
        /// Color Highlight (#78D7)
        /// </summary>
        [ArgbValue(-16746281)] Highlight = 13,

        /// <summary>
        /// Color HighlightText (#FFFFFF)
        /// </summary>
        [ArgbValue(-1)] HighlightText = 14,

        /// <summary>
        /// Color HotTrack (#66CC)
        /// </summary>
        [ArgbValue(-16750900)] HotTrack = 15,

        /// <summary>
        /// Color InactiveBorder (#F4F7FC)
        /// </summary>
        [ArgbValue(-722948)] InactiveBorder = 16,

        /// <summary>
        /// Color InactiveCaption (#BFCDDB)
        /// </summary>
        [ArgbValue(-4207141)] InactiveCaption = 17,

        /// <summary>
        /// Color InactiveCaptionText (#0)
        /// </summary>
        [ArgbValue(-16777216)] InactiveCaptionText = 18,

        /// <summary>
        /// Color Info (#FFFFE1)
        /// </summary>
        [ArgbValue(-31)] Info = 19,

        /// <summary>
        /// Color InfoText (#0)
        /// </summary>
        [ArgbValue(-16777216)] InfoText = 20,

        /// <summary>
        /// Color Menu (#F0F0F0)
        /// </summary>
        [ArgbValue(-986896)] Menu = 21,

        /// <summary>
        /// Color MenuText (#0)
        /// </summary>
        [ArgbValue(-16777216)] MenuText = 22,

        /// <summary>
        /// Color ScrollBar (#C8C8C8)
        /// </summary>
        [ArgbValue(-3618616)] ScrollBar = 23,

        /// <summary>
        /// Color Window (#FFFFFF)
        /// </summary>
        [ArgbValue(-1)] Window = 24,

        /// <summary>
        /// Color WindowFrame (#646464)
        /// </summary>
        [ArgbValue(-10197916)] WindowFrame = 25,

        /// <summary>
        /// Color WindowText (#0)
        /// </summary>
        [ArgbValue(-16777216)] WindowText = 26,

        /// <summary>
        /// Color Transparent (#FFFFFF)
        /// </summary>
        [ArgbValue(16777215)] Transparent = 27,

        /// <summary>
        /// Color AliceBlue (#F0F8FF)
        /// </summary>
        [ArgbValue(-984833)] AliceBlue = 28,

        /// <summary>
        /// Color AntiqueWhite (#FAEBD7)
        /// </summary>
        [ArgbValue(-332841)] AntiqueWhite = 29,

        /// <summary>
        /// Color Aqua (#FFFF)
        /// </summary>
        [ArgbValue(-16711681)] Aqua = 30,

        /// <summary>
        /// Color Aquamarine (#7FFFD4)
        /// </summary>
        [ArgbValue(-8388652)] Aquamarine = 31,

        /// <summary>
        /// Color Azure (#F0FFFF)
        /// </summary>
        [ArgbValue(-983041)] Azure = 32,

        /// <summary>
        /// Color Beige (#F5F5DC)
        /// </summary>
        [ArgbValue(-657956)] Beige = 33,

        /// <summary>
        /// Color Bisque (#FFE4C4)
        /// </summary>
        [ArgbValue(-6972)] Bisque = 34,

        /// <summary>
        /// Color Black (#0)
        /// </summary>
        [ArgbValue(-16777216)] Black = 35,

        /// <summary>
        /// Color BlanchedAlmond (#FFEBCD)
        /// </summary>
        [ArgbValue(-5171)] BlanchedAlmond = 36,

        /// <summary>
        /// Color Blue (#FF)
        /// </summary>
        [ArgbValue(-16776961)] Blue = 37,

        /// <summary>
        /// Color BlueViolet (#8A2BE2)
        /// </summary>
        [ArgbValue(-7722014)] BlueViolet = 38,

        /// <summary>
        /// Color Brown (#A52A2A)
        /// </summary>
        [ArgbValue(-5952982)] Brown = 39,

        /// <summary>
        /// Color BurlyWood (#DEB887)
        /// </summary>
        [ArgbValue(-2180985)] BurlyWood = 40,

        /// <summary>
        /// Color CadetBlue (#5F9EA0)
        /// </summary>
        [ArgbValue(-10510688)] CadetBlue = 41,

        /// <summary>
        /// Color Chartreuse (#7FFF00)
        /// </summary>
        [ArgbValue(-8388864)] Chartreuse = 42,

        /// <summary>
        /// Color Chocolate (#D2691E)
        /// </summary>
        [ArgbValue(-2987746)] Chocolate = 43,

        /// <summary>
        /// Color Coral (#FF7F50)
        /// </summary>
        [ArgbValue(-32944)] Coral = 44,

        /// <summary>
        /// Color CornflowerBlue (#6495ED)
        /// </summary>
        [ArgbValue(-10185235)] CornflowerBlue = 45,

        /// <summary>
        /// Color Cornsilk (#FFF8DC)
        /// </summary>
        [ArgbValue(-1828)] Cornsilk = 46,

        /// <summary>
        /// Color Crimson (#DC143C)
        /// </summary>
        [ArgbValue(-2354116)] Crimson = 47,

        /// <summary>
        /// Color Cyan (#FFFF)
        /// </summary>
        [ArgbValue(-16711681)] Cyan = 48,

        /// <summary>
        /// Color DarkBlue (#8B)
        /// </summary>
        [ArgbValue(-16777077)] DarkBlue = 49,

        /// <summary>
        /// Color DarkCyan (#8B8B)
        /// </summary>
        [ArgbValue(-16741493)] DarkCyan = 50,

        /// <summary>
        /// Color DarkGoldenrod (#B8860B)
        /// </summary>
        [ArgbValue(-4684277)] DarkGoldenrod = 51,

        /// <summary>
        /// Color DarkGray (#A9A9A9)
        /// </summary>
        [ArgbValue(-5658199)] DarkGray = 52,

        /// <summary>
        /// Color DarkGreen (#6400)
        /// </summary>
        [ArgbValue(-16751616)] DarkGreen = 53,

        /// <summary>
        /// Color DarkKhaki (#BDB76B)
        /// </summary>
        [ArgbValue(-4343957)] DarkKhaki = 54,

        /// <summary>
        /// Color DarkMagenta (#8B008B)
        /// </summary>
        [ArgbValue(-7667573)] DarkMagenta = 55,

        /// <summary>
        /// Color DarkOliveGreen (#556B2F)
        /// </summary>
        [ArgbValue(-11179217)] DarkOliveGreen = 56,

        /// <summary>
        /// Color DarkOrange (#FF8C00)
        /// </summary>
        [ArgbValue(-29696)] DarkOrange = 57,

        /// <summary>
        /// Color DarkOrchid (#9932CC)
        /// </summary>
        [ArgbValue(-6737204)] DarkOrchid = 58,

        /// <summary>
        /// Color DarkRed (#8B0000)
        /// </summary>
        [ArgbValue(-7667712)] DarkRed = 59,

        /// <summary>
        /// Color DarkSalmon (#E9967A)
        /// </summary>
        [ArgbValue(-1468806)] DarkSalmon = 60,

        /// <summary>
        /// Color DarkSeaGreen (#8FBC8B)
        /// </summary>
        [ArgbValue(-7357301)] DarkSeaGreen = 61,

        /// <summary>
        /// Color DarkSlateBlue (#483D8B)
        /// </summary>
        [ArgbValue(-12042869)] DarkSlateBlue = 62,

        /// <summary>
        /// Color DarkSlateGray (#2F4F4F)
        /// </summary>
        [ArgbValue(-13676721)] DarkSlateGray = 63,

        /// <summary>
        /// Color DarkTurquoise (#CED1)
        /// </summary>
        [ArgbValue(-16724271)] DarkTurquoise = 64,

        /// <summary>
        /// Color DarkViolet (#9400D3)
        /// </summary>
        [ArgbValue(-7077677)] DarkViolet = 65,

        /// <summary>
        /// Color DeepPink (#FF1493)
        /// </summary>
        [ArgbValue(-60269)] DeepPink = 66,

        /// <summary>
        /// Color DeepSkyBlue (#BFFF)
        /// </summary>
        [ArgbValue(-16728065)] DeepSkyBlue = 67,

        /// <summary>
        /// Color DimGray (#696969)
        /// </summary>
        [ArgbValue(-9868951)] DimGray = 68,

        /// <summary>
        /// Color DodgerBlue (#1E90FF)
        /// </summary>
        [ArgbValue(-14774017)] DodgerBlue = 69,

        /// <summary>
        /// Color Firebrick (#B22222)
        /// </summary>
        [ArgbValue(-5103070)] Firebrick = 70,

        /// <summary>
        /// Color FloralWhite (#FFFAF0)
        /// </summary>
        [ArgbValue(-1296)] FloralWhite = 71,

        /// <summary>
        /// Color ForestGreen (#228B22)
        /// </summary>
        [ArgbValue(-14513374)] ForestGreen = 72,

        /// <summary>
        /// Color Fuchsia (#FF00FF)
        /// </summary>
        [ArgbValue(-65281)] Fuchsia = 73,

        /// <summary>
        /// Color Gainsboro (#DCDCDC)
        /// </summary>
        [ArgbValue(-2302756)] Gainsboro = 74,

        /// <summary>
        /// Color GhostWhite (#F8F8FF)
        /// </summary>
        [ArgbValue(-460545)] GhostWhite = 75,

        /// <summary>
        /// Color Gold (#FFD700)
        /// </summary>
        [ArgbValue(-10496)] Gold = 76,

        /// <summary>
        /// Color Goldenrod (#DAA520)
        /// </summary>
        [ArgbValue(-2448096)] Goldenrod = 77,

        /// <summary>
        /// Color Gray (#808080)
        /// </summary>
        [ArgbValue(-8355712)] Gray = 78,

        /// <summary>
        /// Color Green (#8000)
        /// </summary>
        [ArgbValue(-16744448)] Green = 79,

        /// <summary>
        /// Color GreenYellow (#ADFF2F)
        /// </summary>
        [ArgbValue(-5374161)] GreenYellow = 80,

        /// <summary>
        /// Color Honeydew (#F0FFF0)
        /// </summary>
        [ArgbValue(-983056)] Honeydew = 81,

        /// <summary>
        /// Color HotPink (#FF69B4)
        /// </summary>
        [ArgbValue(-38476)] HotPink = 82,

        /// <summary>
        /// Color IndianRed (#CD5C5C)
        /// </summary>
        [ArgbValue(-3318692)] IndianRed = 83,

        /// <summary>
        /// Color Indigo (#4B0082)
        /// </summary>
        [ArgbValue(-11861886)] Indigo = 84,

        /// <summary>
        /// Color Ivory (#FFFFF0)
        /// </summary>
        [ArgbValue(-16)] Ivory = 85,

        /// <summary>
        /// Color Khaki (#F0E68C)
        /// </summary>
        [ArgbValue(-989556)] Khaki = 86,

        /// <summary>
        /// Color Lavender (#E6E6FA)
        /// </summary>
        [ArgbValue(-1644806)] Lavender = 87,

        /// <summary>
        /// Color LavenderBlush (#FFF0F5)
        /// </summary>
        [ArgbValue(-3851)] LavenderBlush = 88,

        /// <summary>
        /// Color LawnGreen (#7CFC00)
        /// </summary>
        [ArgbValue(-8586240)] LawnGreen = 89,

        /// <summary>
        /// Color LemonChiffon (#FFFACD)
        /// </summary>
        [ArgbValue(-1331)] LemonChiffon = 90,

        /// <summary>
        /// Color LightBlue (#ADD8E6)
        /// </summary>
        [ArgbValue(-5383962)] LightBlue = 91,

        /// <summary>
        /// Color LightCoral (#F08080)
        /// </summary>
        [ArgbValue(-1015680)] LightCoral = 92,

        /// <summary>
        /// Color LightCyan (#E0FFFF)
        /// </summary>
        [ArgbValue(-2031617)] LightCyan = 93,

        /// <summary>
        /// Color LightGoldenrodYellow (#FAFAD2)
        /// </summary>
        [ArgbValue(-329006)] LightGoldenrodYellow = 94,

        /// <summary>
        /// Color LightGray (#D3D3D3)
        /// </summary>
        [ArgbValue(-2894893)] LightGray = 95,

        /// <summary>
        /// Color LightGreen (#90EE90)
        /// </summary>
        [ArgbValue(-7278960)] LightGreen = 96,

        /// <summary>
        /// Color LightPink (#FFB6C1)
        /// </summary>
        [ArgbValue(-18751)] LightPink = 97,

        /// <summary>
        /// Color LightSalmon (#FFA07A)
        /// </summary>
        [ArgbValue(-24454)] LightSalmon = 98,

        /// <summary>
        /// Color LightSeaGreen (#20B2AA)
        /// </summary>
        [ArgbValue(-14634326)] LightSeaGreen = 99,

        /// <summary>
        /// Color LightSkyBlue (#87CEFA)
        /// </summary>
        [ArgbValue(-7876870)] LightSkyBlue = 100,

        /// <summary>
        /// Color LightSlateGray (#778899)
        /// </summary>
        [ArgbValue(-8943463)] LightSlateGray = 101,

        /// <summary>
        /// Color LightSteelBlue (#B0C4DE)
        /// </summary>
        [ArgbValue(-5192482)] LightSteelBlue = 102,

        /// <summary>
        /// Color LightYellow (#FFFFE0)
        /// </summary>
        [ArgbValue(-32)] LightYellow = 103,

        /// <summary>
        /// Color Lime (#FF00)
        /// </summary>
        [ArgbValue(-16711936)] Lime = 104,

        /// <summary>
        /// Color LimeGreen (#32CD32)
        /// </summary>
        [ArgbValue(-13447886)] LimeGreen = 105,

        /// <summary>
        /// Color Linen (#FAF0E6)
        /// </summary>
        [ArgbValue(-331546)] Linen = 106,

        /// <summary>
        /// Color Magenta (#FF00FF)
        /// </summary>
        [ArgbValue(-65281)] Magenta = 107,

        /// <summary>
        /// Color Maroon (#800000)
        /// </summary>
        [ArgbValue(-8388608)] Maroon = 108,

        /// <summary>
        /// Color MediumAquamarine (#66CDAA)
        /// </summary>
        [ArgbValue(-10039894)] MediumAquamarine = 109,

        /// <summary>
        /// Color MediumBlue (#CD)
        /// </summary>
        [ArgbValue(-16777011)] MediumBlue = 110,

        /// <summary>
        /// Color MediumOrchid (#BA55D3)
        /// </summary>
        [ArgbValue(-4565549)] MediumOrchid = 111,

        /// <summary>
        /// Color MediumPurple (#9370DB)
        /// </summary>
        [ArgbValue(-7114533)] MediumPurple = 112,

        /// <summary>
        /// Color MediumSeaGreen (#3CB371)
        /// </summary>
        [ArgbValue(-12799119)] MediumSeaGreen = 113,

        /// <summary>
        /// Color MediumSlateBlue (#7B68EE)
        /// </summary>
        [ArgbValue(-8689426)] MediumSlateBlue = 114,

        /// <summary>
        /// Color MediumSpringGreen (#FA9A)
        /// </summary>
        [ArgbValue(-16713062)] MediumSpringGreen = 115,

        /// <summary>
        /// Color MediumTurquoise (#48D1CC)
        /// </summary>
        [ArgbValue(-12004916)] MediumTurquoise = 116,

        /// <summary>
        /// Color MediumVioletRed (#C71585)
        /// </summary>
        [ArgbValue(-3730043)] MediumVioletRed = 117,

        /// <summary>
        /// Color MidnightBlue (#191970)
        /// </summary>
        [ArgbValue(-15132304)] MidnightBlue = 118,

        /// <summary>
        /// Color MintCream (#F5FFFA)
        /// </summary>
        [ArgbValue(-655366)] MintCream = 119,

        /// <summary>
        /// Color MistyRose (#FFE4E1)
        /// </summary>
        [ArgbValue(-6943)] MistyRose = 120,

        /// <summary>
        /// Color Moccasin (#FFE4B5)
        /// </summary>
        [ArgbValue(-6987)] Moccasin = 121,

        /// <summary>
        /// Color NavajoWhite (#FFDEAD)
        /// </summary>
        [ArgbValue(-8531)] NavajoWhite = 122,

        /// <summary>
        /// Color Navy (#80)
        /// </summary>
        [ArgbValue(-16777088)] Navy = 123,

        /// <summary>
        /// Color OldLace (#FDF5E6)
        /// </summary>
        [ArgbValue(-133658)] OldLace = 124,

        /// <summary>
        /// Color Olive (#808000)
        /// </summary>
        [ArgbValue(-8355840)] Olive = 125,

        /// <summary>
        /// Color OliveDrab (#6B8E23)
        /// </summary>
        [ArgbValue(-9728477)] OliveDrab = 126,

        /// <summary>
        /// Color Orange (#FFA500)
        /// </summary>
        [ArgbValue(-23296)] Orange = 127,

        /// <summary>
        /// Color OrangeRed (#FF4500)
        /// </summary>
        [ArgbValue(-47872)] OrangeRed = 128,

        /// <summary>
        /// Color Orchid (#DA70D6)
        /// </summary>
        [ArgbValue(-2461482)] Orchid = 129,

        /// <summary>
        /// Color PaleGoldenrod (#EEE8AA)
        /// </summary>
        [ArgbValue(-1120086)] PaleGoldenrod = 130,

        /// <summary>
        /// Color PaleGreen (#98FB98)
        /// </summary>
        [ArgbValue(-6751336)] PaleGreen = 131,

        /// <summary>
        /// Color PaleTurquoise (#AFEEEE)
        /// </summary>
        [ArgbValue(-5247250)] PaleTurquoise = 132,

        /// <summary>
        /// Color PaleVioletRed (#DB7093)
        /// </summary>
        [ArgbValue(-2396013)] PaleVioletRed = 133,

        /// <summary>
        /// Color PapayaWhip (#FFEFD5)
        /// </summary>
        [ArgbValue(-4139)] PapayaWhip = 134,

        /// <summary>
        /// Color PeachPuff (#FFDAB9)
        /// </summary>
        [ArgbValue(-9543)] PeachPuff = 135,

        /// <summary>
        /// Color Peru (#CD853F)
        /// </summary>
        [ArgbValue(-3308225)] Peru = 136,

        /// <summary>
        /// Color Pink (#FFC0CB)
        /// </summary>
        [ArgbValue(-16181)] Pink = 137,

        /// <summary>
        /// Color Plum (#DDA0DD)
        /// </summary>
        [ArgbValue(-2252579)] Plum = 138,

        /// <summary>
        /// Color PowderBlue (#B0E0E6)
        /// </summary>
        [ArgbValue(-5185306)] PowderBlue = 139,

        /// <summary>
        /// Color Purple (#800080)
        /// </summary>
        [ArgbValue(-8388480)] Purple = 140,

        /// <summary>
        /// Color Red (#FF0000)
        /// </summary>
        [ArgbValue(-65536)] Red = 141,

        /// <summary>
        /// Color RosyBrown (#BC8F8F)
        /// </summary>
        [ArgbValue(-4419697)] RosyBrown = 142,

        /// <summary>
        /// Color RoyalBlue (#4169E1)
        /// </summary>
        [ArgbValue(-12490271)] RoyalBlue = 143,

        /// <summary>
        /// Color SaddleBrown (#8B4513)
        /// </summary>
        [ArgbValue(-7650029)] SaddleBrown = 144,

        /// <summary>
        /// Color Salmon (#FA8072)
        /// </summary>
        [ArgbValue(-360334)] Salmon = 145,

        /// <summary>
        /// Color SandyBrown (#F4A460)
        /// </summary>
        [ArgbValue(-744352)] SandyBrown = 146,

        /// <summary>
        /// Color SeaGreen (#2E8B57)
        /// </summary>
        [ArgbValue(-13726889)] SeaGreen = 147,

        /// <summary>
        /// Color SeaShell (#FFF5EE)
        /// </summary>
        [ArgbValue(-2578)] SeaShell = 148,

        /// <summary>
        /// Color Sienna (#A0522D)
        /// </summary>
        [ArgbValue(-6270419)] Sienna = 149,

        /// <summary>
        /// Color Silver (#C0C0C0)
        /// </summary>
        [ArgbValue(-4144960)] Silver = 150,

        /// <summary>
        /// Color SkyBlue (#87CEEB)
        /// </summary>
        [ArgbValue(-7876885)] SkyBlue = 151,

        /// <summary>
        /// Color SlateBlue (#6A5ACD)
        /// </summary>
        [ArgbValue(-9807155)] SlateBlue = 152,

        /// <summary>
        /// Color SlateGray (#708090)
        /// </summary>
        [ArgbValue(-9404272)] SlateGray = 153,

        /// <summary>
        /// Color Snow (#FFFAFA)
        /// </summary>
        [ArgbValue(-1286)] Snow = 154,

        /// <summary>
        /// Color SpringGreen (#FF7F)
        /// </summary>
        [ArgbValue(-16711809)] SpringGreen = 155,

        /// <summary>
        /// Color SteelBlue (#4682B4)
        /// </summary>
        [ArgbValue(-12156236)] SteelBlue = 156,

        /// <summary>
        /// Color Tan (#D2B48C)
        /// </summary>
        [ArgbValue(-2968436)] Tan = 157,

        /// <summary>
        /// Color Teal (#8080)
        /// </summary>
        [ArgbValue(-16744320)] Teal = 158,

        /// <summary>
        /// Color Thistle (#D8BFD8)
        /// </summary>
        [ArgbValue(-2572328)] Thistle = 159,

        /// <summary>
        /// Color Tomato (#FF6347)
        /// </summary>
        [ArgbValue(-40121)] Tomato = 160,

        /// <summary>
        /// Color Turquoise (#40E0D0)
        /// </summary>
        [ArgbValue(-12525360)] Turquoise = 161,

        /// <summary>
        /// Color Violet (#EE82EE)
        /// </summary>
        [ArgbValue(-1146130)] Violet = 162,

        /// <summary>
        /// Color Wheat (#F5DEB3)
        /// </summary>
        [ArgbValue(-663885)] Wheat = 163,

        /// <summary>
        /// Color White (#FFFFFF)
        /// </summary>
        [ArgbValue(-1)] White = 164,

        /// <summary>
        /// Color WhiteSmoke (#F5F5F5)
        /// </summary>
        [ArgbValue(-657931)] WhiteSmoke = 165,

        /// <summary>
        /// Color Yellow (#FFFF00)
        /// </summary>
        [ArgbValue(-256)] Yellow = 166,

        /// <summary>
        /// Color YellowGreen (#9ACD32)
        /// </summary>
        [ArgbValue(-6632142)] YellowGreen = 167,

        /// <summary>
        /// Color ButtonFace (#F0F0F0)
        /// </summary>
        [ArgbValue(-986896)] ButtonFace = 168,

        /// <summary>
        /// Color ButtonHighlight (#FFFFFF)
        /// </summary>
        [ArgbValue(-1)] ButtonHighlight = 169,

        /// <summary>
        /// Color ButtonShadow (#A0A0A0)
        /// </summary>
        [ArgbValue(-6250336)] ButtonShadow = 170,

        /// <summary>
        /// Color GradientActiveCaption (#B9D1EA)
        /// </summary>
        [ArgbValue(-4599318)] GradientActiveCaption = 171,

        /// <summary>
        /// Color GradientInactiveCaption (#D7E4F2)
        /// </summary>
        [ArgbValue(-2628366)] GradientInactiveCaption = 172,

        /// <summary>
        /// Color MenuBar (#F0F0F0)
        /// </summary>
        [ArgbValue(-986896)] MenuBar = 173,

        /// <summary>
        /// Color MenuHighlight (#3399FF)
        /// </summary>
        [ArgbValue(-13395457)] MenuHighlight = 174,
    }
}
#endif
