$(document).ready(function () {
    var lon = -73.9529;
    var lat = 40.7723;
    var zoom = 10;

    var map = new L.Map('map');
    var cloudmade = new L.TileLayer('http://{s}.tile.cloudmade.com/1a1b06b230af4efdbb989ea99e9841af/997/256/{z}/{x}/{y}.png', { maxZoom: 18 });
    map.addLayer(cloudmade);

    var tile = new L.TileLayer.TileJSON({
        debug: false
    });
    tile.createUrl = function (bounds) {
        var url = ['/json.ashx?MAP_TYPE=PM&BBOX=',
            bounds[0], ',',
            bounds[1], ',',
            bounds[2], ',',
            bounds[3]
        ].join('');
        return url;
    };
    tile.styleFor = function (feature) {
        var type = feature.geometry.type;
        switch (type) {
            case 'Point':
            case 'MultiPoint':
                return {
                    color: 'rgba(252,146,114,0.6)',
                    radius: 5
                };
                
            case 'LineString':
            case 'MultiLineString':
                return {
                    color: 'rgba(161,217,155,0.8)',
                    size: 3
                };

            case 'Polygon':
            case 'MultiPolygon':
                return {
                    color: 'rgba(43,140,190,0.4)',
                    outline: {
                        color: 'rgb(0,0,0)',
                        size: 1
                    }
                };

            default:
                return null;
        }
    };
    map.addLayer(tile);

    var center = new L.LatLng(lat, lon);
    map.setView(center, zoom);
});