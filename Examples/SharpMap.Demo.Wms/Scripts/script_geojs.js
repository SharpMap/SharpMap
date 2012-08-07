$(document).ready(function () {
    var options, init;

    OpenLayers.DOTS_PER_INCH = 25.4 / 0.28;
    OpenLayers.IMAGE_RELOAD_ATTEMPTS = 5;
    OpenLayers.Util.onImageLoadErrorColor = 'transparent';
    OpenLayers.Util.onImageLoadError = function () {
        this.src = '/Content/Images/sorry.jpg';
        this.style.backgroundColor = OpenLayers.Util.onImageLoadErrorColor;
    };

    options = {
        wms: 'WMS',
        controls: [],
        projection: 'EPSG:900913',
        displayProjection: 'EPSG:4326',
        format: 'text/json'
    };

    init = function () {
        var lon = -73.9529, lat = 40.7723, zoom = 10,
            map, url, layers = [], center, highlight;

        map = new OpenLayers.Map('map', options);        
        map.addLayer(new OpenLayers.Layer.OSM());

        url = [
            '/wms.ashx',
            '?SERVICE=', options.wms,
            '&FORMAT=', options.format,
            '&CRS=', options.projection,
            '&REQUEST=GETFEATUREINFO&VERSION=1.3.0&STYLES=&WIDTH=20&HEIGHT=20&I=10&J=10&MAP_TYPE=SPH&LAYERS=poi,tiger_roads,poly_landmarks',
            '&FEATURE_COUNT=0', // NOTE: ignored for json requests
            '&INFO_FORMAT=', options.format
        ].join('');

        layers.push(
            new OpenLayers.Layer.Vector(
                'POI', {
                    strategies: [new OpenLayers.Strategy.BBOX()],
                    protocol: new OpenLayers.Protocol.HTTP({
                        url: [url, '&QUERY_LAYERS=poi'].join(''),
                        format: new OpenLayers.Format.GeoJSON()
                    }),
                    styleMap: OpenLayers.Resources.Styles.Layers.editable,
                    visibility: false
                }));
        layers.push(
            new OpenLayers.Layer.Vector(
                'Roads', {
                    strategies: [new OpenLayers.Strategy.BBOX()],
                    protocol: new OpenLayers.Protocol.HTTP({
                        url: [url, '&QUERY_LAYERS=tiger_roads'].join(''),
                        format: new OpenLayers.Format.GeoJSON()
                    }),
                    styleMap: OpenLayers.Resources.Styles.Layers.editable,
                    visibility: false
                }));
        layers.push(
            new OpenLayers.Layer.Vector(
                'Landmarks', {
                    strategies: [new OpenLayers.Strategy.BBOX()],
                    protocol: new OpenLayers.Protocol.HTTP({
                        url: [url, '&QUERY_LAYERS=poly_landmarks'].join(''),
                        format: new OpenLayers.Format.GeoJSON()
                    }),
                    styleMap: OpenLayers.Resources.Styles.Layers.editable,
                    visibility: false
                }));
        map.addLayers(layers.reverse());

        highlight = new OpenLayers.Control.SelectFeatureEx(layers, {
            hover: true,
            highlightOnly: true,
            renderIntent: 'temporary'
        });        
        map.addControl(new OpenLayers.Control.LayerSwitcher());
        map.addControl(new OpenLayers.Control.NavToolbar());
        map.addControl(new OpenLayers.Control.PanZoom({
            position: new OpenLayers.Pixel(2, 10)
        }));
        map.addControl(new OpenLayers.Control.MousePosition());
        map.addControl(new OpenLayers.Control.LoadingPanel());
        map.addControl(highlight);
        highlight.activate();

        center = new OpenLayers.LonLat(lon, lat);
        center.transform(options.displayProjection, options.projection);
        map.setCenter(center, zoom);
    };
    init();
});
