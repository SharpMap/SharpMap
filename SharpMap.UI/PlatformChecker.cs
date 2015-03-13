using System;

namespace SharpMap
{
    internal static class PlatformChecker
    {
        public static bool IsRunningOnWindows
        {
            get
            {
                var platformId = Environment.OSVersion.Platform;
                return (platformId == PlatformID.Win32NT);
            }
        }
        public static bool IsRunningOnUnix
        {
            get
            {
                var platformId = Environment.OSVersion.Platform;
                return (platformId == PlatformID.Unix);
            }
        }
        public static bool IsRunningOnMacOSX
        {
            get
            {
                var platformId = Environment.OSVersion.Platform;
                return (platformId == PlatformID.MacOSX);
            }
        }
    }
}