using System;
using System.Globalization;
using System.Reflection;
using System.Xml;

namespace SharpMap.Web
{
    internal class HttpCacheUtility
    {
        private static bool _systemWebTested;
        private static readonly DateTime NoAbsoluteExpiration = DateTime.MaxValue;
        private static readonly object LockContext = new object();

        /*
        private static Type _cacheDependencyType;
        private static PropertyInfo _cachePropertyInfo;
        private static PropertyInfo _httpContextPropertyInfo;
         */
        private static MethodInfo _insertMethodInfo;
        private static MethodInfo _getMethodInfo;

        private static object _context;
        private static object _cache;

        private static void TestSystemWeb()
        {
            if (_systemWebTested) return;

            lock (LockContext)
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.GetName().Name != "System.Web") continue;

                    try
                    {
                        var httpContextType = 
                            assembly.GetType("System.Web.HttpContext");
                         var httpContextPropertyInfo = 
                            httpContextType.GetProperty("Current",
                                                        BindingFlags.Public | BindingFlags.Static);
                        _context = httpContextPropertyInfo.GetValue(null, null);

                        var cachePropertyInfo = 
                            httpContextType.GetProperty("Cache",
                                                        BindingFlags.Public | BindingFlags.Instance);
                        if (_context != null)
                        {
                            _cache = cachePropertyInfo.GetValue(_context, null);

                            var cacheType = assembly.GetType("System.Web.Caching.Cache");
                            var cacheDependencyType =
                                assembly.GetType("System.Web.Caching.CacheDependency");
                            _insertMethodInfo = cacheType.GetMethod("Insert",
                                                                    new[]
                                                                    {
                                                                        typeof (string), typeof (object),
                                                                        cacheDependencyType, typeof (DateTime),
                                                                        typeof (TimeSpan)
                                                                    });
                            _getMethodInfo = cacheType.GetMethod("Get", new[] { typeof(string) });
                        }
                        //Found it, tested it, continue with useful stuff
                        break;
                    }
                    catch (Exception /*ex*/)
                    {
                    }
                }
            }

            _systemWebTested = true;
        }

        private static object GetCurrentHttpContext()
        {
            return _context;
            /*
            if (_httpContextPropertyInfo == null)
                return null;
            return _httpContextPropertyInfo.GetValue(null, null);
             */
        }

        private static object GetHttpContextCache(/*object httpContext*/)
        {
            return _cache;
            /*
            if (_cachePropertyInfo == null)
                return null;
            return _cachePropertyInfo.GetValue(httpContext, null);
             */
        }

        private static object GetCurrentCache()
        {
            if (!_systemWebTested)
            {
                lock(LockContext)
                {
                    if (!_systemWebTested) 
                        TestSystemWeb();
                }
            }
            var httpContext = GetCurrentHttpContext();
            if (httpContext == null)
                return null;

            return GetHttpContextCache(/*httpContext*/);
        }

        internal static bool TryGetValue<T>(string key, out T instance)
            where T: class
        {
            var cache = GetCurrentCache();
            if (cache == null)
            {
                instance = default(T);
                return false;
            }

            instance = (T)_getMethodInfo.Invoke(cache, new object[] {key});
            return instance != null;
        }

        /// <summary>
        /// Gets a value indicating that the code is being run in a web context
        /// </summary>
        internal static bool IsWebContext { get { return GetCurrentCache() != null; } }

        /// <summary>
        /// Function that tries to get an object from the WebCache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        internal static bool TryAddValue<T>(string key, T instance)
            where T: class
        {
            return TryAddValue(key, instance, TimeSpan.FromDays(1));
        }

        internal static bool TryAddValue<T>(string key, T instance, TimeSpan timeSpan)
            where T : class
        {
            var cache = GetCurrentCache();
            if (cache == null)
                return false;

            _insertMethodInfo.Invoke(cache, new object[] { key, instance, null, NoAbsoluteExpiration, timeSpan });
            return true;
        }
    }
    
    class WebUtilities
    {
        #region Reusable XML Parsing

        public static XmlNode FindEpsgNode(XmlNode bbox)
        {
            if (bbox == null || bbox.Attributes == null)
                throw new ArgumentNullException("bbox");

            XmlNode epsgNode = ((bbox.Attributes["srs"] ?? bbox.Attributes["crs"]) ?? bbox.Attributes["SRS"]) ?? bbox.Attributes["CRS"];
            return epsgNode;
        }

        public static bool TryParseNodeAsEpsg(XmlNode node, out int epsg)
        {
            epsg = default(int);
            if (node == null) return false;
            string epsgString = node.Value;
            if (String.IsNullOrEmpty(epsgString)) return false;
            const string prefix = "EPSG:";
            int index = epsgString.IndexOf(prefix, StringComparison.InvariantCulture);
            if (index < 0) return false;
            return (Int32.TryParse(epsgString.Substring(index + prefix.Length), NumberStyles.Any, Map.NumberFormatEnUs, out epsg));
        }

        public static double ParseNodeAsDouble(XmlNode node, double defaultValue)
        {
            if (node == null) return defaultValue;
            if (String.IsNullOrEmpty(node.InnerText)) return defaultValue;
            double value;
            if (Double.TryParse(node.InnerText, NumberStyles.Any, Map.NumberFormatEnUs, out value))
                return value;
            return defaultValue;
        }

        public static bool TryParseNodeAsDouble(XmlNode node, out double value)
        {
            value = default(double);
            if (node == null) return false;
            if (String.IsNullOrEmpty(node.InnerText)) return false;
            return Double.TryParse(node.InnerText, NumberStyles.Any, Map.NumberFormatEnUs, out value);
        }

        #endregion
    }
}
