$(document).ready(function() {
    var options, init;

    OpenLayers.DOTS_PER_INCH = 25.4 / 0.28;
    OpenLayers.IMAGE_RELOAD_ATTEMPTS = 5;
    OpenLayers.Util.onImageLoadErrorColor = 'transparent';
    OpenLayers.Util.onImageLoadError = function() {
        this.src = '/Content/Images/sorry.jpg';
        this.style.backgroundColor = OpenLayers.Util.onImageLoadErrorColor;
    };

    options = {
        wms: 'WMS',
        wmslayers: ['BruTile-OSM'].join(),
        controls: [],
        projection: 'EPSG:900913',
        displayProjection: 'EPSG:4326',
        format: 'image/png',
        wmsparams: {
            'MAP_TYPE': 'BRU'
        }
    };

    init = function() {
        var lon = -73.9529, lat = 40.7723, zoom = 10,
            map, sharpmap, center;

        map = new OpenLayers.Map('map', options);
        sharpmap = new OpenLayers.Layer.WMS(
            'SharpMap WMS',
            '/wms.ashx', {
                layers: options.wmslayers,
                service: options.wms,
                version: '1.3.0',
                format: options.format,
                transparent: false
            }, {
                isBaseLayer: true,
                transparent: false,
                visibility: true,
                buffer: 0,
                singleTile: false,
                ratio: 1.5
            });
        sharpmap.mergeNewParams(options.wmsparams);
        map.addLayers([sharpmap]);

        map.addControl(new OpenLayers.Control.LayerSwitcher());
        map.addControl(new OpenLayers.Control.PanZoom({
            position: new OpenLayers.Pixel(2, 10)
        }));
        map.addControl(new OpenLayers.Control.MousePosition());
        map.addControl(new OpenLayers.Control.LoadingPanel());
        map.addControl(new OpenLayers.Control.NavToolbar());

        center = new OpenLayers.LonLat(lon, lat);
        center.transform(options.displayProjection, options.projection);
        map.setCenter(center, zoom);
    };
    init();
});
