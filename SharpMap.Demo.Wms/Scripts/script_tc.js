$(document).ready(function () {
    var lon = -73.9529;
    var lat = 40.7723;
    var zoom = 10;

    var map = new L.Map('map');
    var cloudmade = new L.TileLayer('http://{s}.tile.cloudmade.com/1a1b06b230af4efdbb989ea99e9841af/997/256/{z}/{x}/{y}.png', { maxZoom: 18 });
    map.addLayer(cloudmade);

    var tile = new L.TileLayer.TileJSON({
        debug: true,
        point: {
            fill: 'rgb(252,146,114)'
        },
        linestring: {
            fill: 'rgb(161,217,155)'
        },
        polygon: {
            fill: 'rgb(43,140,190)'
        }
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
    map.addLayer(tile);

    var center = new L.LatLng(lat, lon);
    map.setView(center, zoom);
});