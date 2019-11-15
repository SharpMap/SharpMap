// Copyright (c) BruTile developers team. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using BruTile;
using BruTile.Wmts;

namespace SharpMap.Utilities
{
    internal class TileSchemaSurrogate : ISerializationSurrogate
    {
        #region Implementation of ISerializationSurrogate

        public virtual void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var ts = (TileSchema) obj;
            info.AddValue("name", ts.Name);
            info.AddValue("srs", ts.Srs);
            info.AddValue("extent", ts.Extent);
            info.AddValue("originX", ts.OriginX);
            info.AddValue("originY", ts.OriginY);
            //info.AddValue("width", ts.Width);
            //info.AddValue("height", ts.Height);
            info.AddValue("format", ts.Format);
            info.AddValue("resolutionsType", ts.Resolutions.GetType());
            info.AddValue("resolutionsCount", ts.Resolutions.Count);

            var counter = 0;
            foreach (var item in ts.Resolutions)
            {
                info.AddValue(string.Format("rk{0}", counter), item.Key); // we should store 
                info.AddValue(string.Format("rv{0}", counter++), item.Value); // we should store 
            }

            info.AddValue("axis", ts.YAxis);
        }

        public virtual object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            var ts = (TileSchema) obj;
            ts.Name = info.GetString("name");
            ts.Srs = info.GetString("srs");
            var extentRef = (ExtentSurrogate.ExtentRef)info.GetValue("extent", typeof(ExtentSurrogate.ExtentRef));
            ts.Extent = (Extent) ((IObjectReference) extentRef).GetRealObject(context);
            ts.OriginX = info.GetDouble("originX");
            ts.OriginY = info.GetDouble("originY");
            //ts.Width = info.GetInt32("width");
            //ts.Height = info.GetInt32("height");
            ts.Format = info.GetString("format");
            
            var type = (Type) info.GetValue("resolutionsType", typeof (Type));
            var list = (IDictionary<string, Resolution>) Activator.CreateInstance(type);
            var count = info.GetInt32("resolutionsCount");
            for (var i = 0; i < count; i++)
            {
                var key = info.GetString(string.Format("rk{0}", i));
                var valueRef = (IObjectReference)
                        info.GetValue(string.Format("rv{0}", i), typeof(ResolutionSurrogate.ResolutionRef));
                var value = (Resolution) valueRef.GetRealObject(context);
                list.Add(key, value);
            }
            Utility.SetFieldValue(ref obj, "_resolutions", BindingFlags.NonPublic | BindingFlags.Instance, list);
            
            ts.YAxis = (YAxis)info.GetInt32("axis");
            return ts;
        }

        #endregion
    }

    namespace Predefined
    {
        internal class WmtsTileSchemaSurrogate : ISerializationSurrogate
        {
            public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
            {
                var ts = (WmtsTileSchema) obj;
                info.AddValue("name", ts.Name);
                info.AddValue("srs", ts.Srs);
                info.AddValue("extent", ts.Extent);
                info.AddValue("format", ts.Format);
                info.AddValue("resolutionsType", ts.Resolutions.GetType());
                info.AddValue("resolutionsCount", ts.Resolutions.Count);

                var counter = 0;
                foreach (var item in ts.Resolutions)
                {
                    info.AddValue(string.Format("rk{0}", counter), item.Key); // we should store 
                    info.AddValue(string.Format("rv{0}", counter++), item.Value); // we should store 
                }
                info.AddValue("axis", ts.YAxis);

                //info.AddValue("identifier", ts.Identifier);
                info.AddValue("layer", ts.Layer);
                info.AddValue("title", ts.Title);
                info.AddValue("abstract", ts.Abstract);
                info.AddValue("style", ts.Style);
                info.AddValue("tileMatrixSet", ts.TileMatrixSet);
                info.AddValue("supportedSRS", ts.SupportedSRS.ToString());
            }

            public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
            {
                var ts = (WmtsTileSchema)obj;
                ts.Name = info.GetString("name");
                ts.Srs = info.GetString("srs");
                var extentRef = (ExtentSurrogate.ExtentRef)info.GetValue("extent", typeof(ExtentSurrogate.ExtentRef));
                ts.Extent = (Extent)((IObjectReference)extentRef).GetRealObject(context);
                ts.Format = info.GetString("format");

                var type = (Type)info.GetValue("resolutionsType", typeof(Type));
                var list = (IDictionary<string, Resolution>)Activator.CreateInstance(type);
                var count = info.GetInt32("resolutionsCount");
                for (var i = 0; i < count; i++)
                {
                    var key = info.GetString(string.Format("rk{0}", i));
                    var valueRef = (IObjectReference)
                        info.GetValue(string.Format("rv{0}", i), typeof(ResolutionSurrogate.ResolutionRef));
                    var value = (Resolution) valueRef.GetRealObject(context);
                    list.Add(key,value);
                }
                Utility.SetPropertyValue(ref obj, "Resolutions", BindingFlags.Public | BindingFlags.Instance, list);

                ts.YAxis = (YAxis)info.GetInt32("axis");

                //Utility.SetPropertyValue(ref obj, "Identifier", BindingFlags.Instance | BindingFlags.Public, info.GetString("identifier"));
                Utility.SetPropertyValue(ref obj, "Layer", BindingFlags.Instance | BindingFlags.Public, info.GetString("layer"));
                Utility.SetPropertyValue(ref obj, "Title", BindingFlags.Instance | BindingFlags.Public, info.GetString("title"));
                Utility.SetPropertyValue(ref obj, "Abstract", BindingFlags.Instance | BindingFlags.Public, info.GetString("abstract"));
                Utility.SetPropertyValue(ref obj, "Style", BindingFlags.Instance | BindingFlags.Public, info.GetString("style"));
                Utility.SetPropertyValue(ref obj, "TileMatrixSet", BindingFlags.Instance | BindingFlags.Public, info.GetString("tileMatrixSet"));

                CrsIdentifier tmp;
                if (CrsIdentifier.TryParse(info.GetString("supportedSRS"), out tmp))
                    Utility.SetPropertyValue(ref obj, "SupportedSRS", BindingFlags.Instance | BindingFlags.Public, tmp);
                
                return ts;
            }
        }
        
        internal class BingSchemaSurrogate : TileSchemaSurrogate
        {
        }

        internal class GlobalMercatorSchemaSurrogate : TileSchemaSurrogate
        {
        }

        internal class GlobalSphericalMercatorSchemaSurrogate : TileSchemaSurrogate
        {
        }

        internal class WkstNederlandSchemaSurrogate : TileSchemaSurrogate
        {
        }

        internal class SphericalMercatorWorldSchemaSurrogate : TileSchemaSurrogate
        {
        }

        internal class SphericalMercatorWorldInvertedSchemaSurrogate : TileSchemaSurrogate
        {
        }
    }
}
