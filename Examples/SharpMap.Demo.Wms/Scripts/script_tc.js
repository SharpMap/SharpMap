$(document).ready(function () {
    var lon = -73.9529;
    var lat = 40.7723;
    var zoom = 10;

    var map = new L.Map('map');
    var url = ['http://{s}tile.cloudmade.com', '/1a235b638b614b458deeb77c7dae4f80', '/997/256/{z}/{x}/{y}.png'].join('');
    var cloudmade = new L.TileLayer(url, {
         maxZoom: 18,
         subdomains: ['a.', 'b.', 'c.', '']
    });
    map.addLayer(cloudmade);

    var tile = new L.TileLayer.TileJSON({
        debug: false,
        // this value should be equal to 'radius' of your points        
        buffer: 10
    });
    tile.createUrl = function (bounds) {
        var url = ['/json.ashx?MAP_TYPE=DEF&BBOX=',
            bounds[1], ',',
            bounds[0], ',',
            bounds[3], ',',
            bounds[2]
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
                    radius: 10
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