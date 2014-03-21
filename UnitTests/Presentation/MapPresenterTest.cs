using System;
using System.Drawing;
using GeoAPI.Geometries;
using NUnit.Framework;
using SharpMap;
using SharpMap.Data.Providers;
using SharpMap.Layers;
using SharpMap.Presentation;
using SharpMap.Fluent;
namespace UnitTests.Presentation
{
    public class MapPresenterTest<T, TOut> where T: IMapPresenter<T>, new()
    {
        //private static Map CreateMap()
        //{
        //    var m = new Map();
        //    m.Layers.Add(new VectorLayer("1", new ShapeFile()));
        //    return m;
        //}

        //public void GetMap()
        //{
        //    var t = new T().SetEnvelope(new Envelope()).Map.Layers.AddLayer();
        //    t.Map = CreateMap();
        //    Assert.DoesNotThrow(t.GetMapImage());
        //    Assert.DoesNotThrow(t.GetMapImage(LayerCollectionType.Static));
        //}
    }
}

namespace SharpMap.Fluent
{
    public static class MapExtensions
    {
        public static IMapPresenter<T> SetEnvelope<T>(this IMapPresenter<T> mapPresenter, Envelope env)
        {
            try
            {
                mapPresenter.Map.ZoomToBox(env);
            }
            catch (Exception)
            {
                
            }
            return mapPresenter;
        }
        public static IMapPresenter<T> SetSize<T>(this IMapPresenter<T> mapPresenter, Size size)
        {
            try
            {
                mapPresenter.Map.Size = size;
            }
            catch (Exception)
            {

            }
            return mapPresenter;
        }
    }

    public static class LayerCollectionsExtensions
    {
        public static LayerCollection AddLayer(this LayerCollection self, ILayer layer)
        {
            self.Add(layer);
            return self;
        }
        public static LayerCollection InsertLayer(this LayerCollection self, int index, ILayer layer)
        {
            self.Insert(index, layer);
            return self;
        }
    }
}