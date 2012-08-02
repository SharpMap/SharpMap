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
        wmslayers: ['poly_landmarks', 'tiger_roads', 'poi'].join(),
        controls: [],
        projection: 'EPSG:900913',
        displayProjection: 'EPSG:4326',
        format: 'image/png',
        wmsparams: {
            'MAP_TYPE': 'OL'
        }
    };

    init = function () {
        var lon = -73.9529, lat = 40.7723, zoom = 10,
            map, sharpmap, utfgrid, callback, control, center, infoc = $('#info');

        map = new OpenLayers.Map('map', options);
        sharpmap = new OpenLayers.Layer.WMS(
            'SharpMap WMS',
            '/wms.ashx', {
                layers: options.wmslayers,
                service: options.wms,
                version: '1.3.0',
                format: options.format,
                transparent: true
            }, {
                isBaseLayer: false,
                transparent: true,
                visibility: true,
                buffer: 0,
                singleTile: false,
                ratio: 1.5,
                yx: []
            });
        sharpmap.mergeNewParams(options.wmsparams);

        utfgrid = new OpenLayers.Layer.UTFGrid({
            name: 'UTF Grid',
            url: "/UtfGrid/GetData/?layer=poly_landmarks&z=${z}&x=${x}&y=${y}",
            utfgridResolution: 2,
            displayInLayerSwitcher: true
        });
        map.addLayers([new OpenLayers.Layer.OSM(), sharpmap, utfgrid]);

        callback = function (lookup) {
            var info, msg;
            if (lookup) {
                for (var idx in lookup) {
                    info = lookup[idx];
                    if (info && info.data) {
                        msg = '[' + info.id + ']: LANAME: ' + info.data.LANAME + ', CFCC: ' + info.data.CFCC;
                        infoc.text(msg);
                    } else {
                        infoc.text('');
                    }
                }
            }
        };
        control = new OpenLayers.Control.UTFGrid({
            callback: callback,
            handlerMode: "move"
        });
        map.addControl(control);
        map.addControl(new OpenLayers.Control.PanZoom({
            position: new OpenLayers.Pixel(2, 10)
        }));
        map.addControl(new OpenLayers.Control.LayerSwitcher());
        map.addControl(new OpenLayers.Control.MousePosition());
        map.addControl(new OpenLayers.Control.LoadingPanel());

        center = new OpenLayers.LonLat(lon, lat);
        center.transform(options.displayProjection, options.projection);
        map.setCenter(center, zoom);
    };
    init();
});