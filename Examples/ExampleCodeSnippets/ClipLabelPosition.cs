namespace ExampleCodeSnippets
{
    /// <summary>
    /// A clipping utility that ensures every geometry is labeled within the current map viewport
    /// </summary>
    public class ClipLabelPosition
    {
        private GeoAPI.Geometries.Geometry _clip;
        private GeoAPI.Geometries.Prepared.IPreparedGeometry _prepClip;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="map">A map</param>
        public ClipLabelPosition(SharpMap.Map map)
        {
            map.MapViewOnChange += () =>
            {
                _clip = map.Factory.ToGeometry(map.Envelope);
                _prepClip = NetTopologySuite.Geometries.Prepared.PreparedGeometryFactory.Prepare(_clip);
            };
        }

        /// <summary>
        /// A <see cref="SharpMap.Layers.LabelLayer.GetLocationMethod"/> implementation that clips
        /// a feature's geometry to the current viewport of the map
        /// </summary>
        /// <param name="row">A feature</param>
        /// <returns>A label position</returns>
        public GeoAPI.Geometries.Coordinate GetClippedPosition(SharpMap.Data.FeatureDataRow row)
        {
            var g = _prepClip.Contains(row.Geometry)
                ? row.Geometry
                : _clip.Intersection(row.Geometry);

            return g.InteriorPoint.Coordinate;
        }
    }
}
