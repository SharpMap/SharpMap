namespace WinFormSamples.Samples
{
    public static class GeometryFeatureProviderSample
    {
        public static SharpMap.Map InitializeMap(float angle)
        {
            var dataSource = new SharpMap.Data.Providers.ShapeFile(
                string.Format("{0}/roads.shp", ShapefileSample.PathOsm), true);
            
            var fds = new SharpMap.Data.FeatureDataSet();
            dataSource.Open();
            dataSource.ExecuteIntersectionQuery(dataSource.GetExtents(), fds);
            dataSource.Close();

            var gfp = new SharpMap.Data.Providers.GeometryFeatureProvider(fds.Tables[0]);
            var vl = new SharpMap.Layers.VectorLayer("roads", gfp)
                         {
                             CoordinateTransformation = LayerTools.Dhdn2ToWgs84
                         };
            var ll = new SharpMap.Layers.LabelLayer("labels")
                         {
                             DataSource = gfp,
                             CoordinateTransformation = LayerTools.Dhdn2ToWgs84,
                             LabelColumn = "name",
                             MultipartGeometryBehaviour =
                                 SharpMap.Layers.LabelLayer.MultipartGeometryBehaviourEnum.Largest,
                         };
            ll.Style.Halo = new System.Drawing.Pen(System.Drawing.Color.Red);
            //ll.Style.IgnoreLength = true;

            var map = new SharpMap.Map();
            map.Layers.Add(vl);
            map.Layers.Add(ll);

            map.Layers.Add(vl);
            map.ZoomToExtents();
            return map;
        }
    }
}
