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
        controls: [],
        projection: 'EPSG:900913',
        displayProjection: 'EPSG:4326',
        format: 'image/png'
    };

    init = function () {
        var lon = -73.9529, lat = 40.7723, zoom = 10,
            map, center, editor;

        map = new OpenLayers.Map('map', options);
        map.addLayers([new OpenLayers.Layer.OSM()]);

        map.addControl(new OpenLayers.Control.PanZoom({
            position: new OpenLayers.Pixel(2, 10)
        }));
        map.addControl(new OpenLayers.Control.MousePosition());
        map.addControl(new OpenLayers.Control.LoadingPanel());

        center = new OpenLayers.LonLat(lon, lat);
        center.transform(options.displayProjection, options.projection);
        map.setCenter(center, zoom);

        editor = new OpenLayers.Editor(map, {
            oleUrl: window.location.origin + '/',
            activeControls: [
                'Navigation',
                'SnappingSettings',
                'CADTools',
                'Separator',
                'SplitFeature',
                'MergeFeature',
                'CleanFeature',
                'DeleteFeature',
                'SelectFeature',
                'Separator',
                'DragFeature',
                'DrawHole',
                'ModifyFeature',
                'Separator'
            ],
            featureTypes: [
                'polygon',
                'path',
                'point'
            ],
            showStatus: function (type, message) {
                alert(message);
            }
        });
        editor.startEditMode();
    };
    init();
});