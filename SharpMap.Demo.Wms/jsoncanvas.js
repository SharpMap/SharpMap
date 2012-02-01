jQuery((function ($) {
    var lon = -73.9529, lat = 40.7723, zoom = 10

    var map = new L.Map('map');

    var cloudmadeUrl = 'http://{s}.tile.cloudmade.com/BC9A493B41014CAABB98F0471D759707/997/256/{z}/{x}/{y}.png',
        cloudmadeAttribution = 'Map data &copy; 2011 OpenStreetMap contributors, Imagery &copy; 2011 CloudMade',
        cloudmade = new L.TileLayer(cloudmadeUrl, { maxZoom: 18, attribution: cloudmadeAttribution });
    map.addLayer(cloudmade);

    var json = new L.GeoJSON(null, {
        pointToLayer: function(p) {
            return new L.CircleMarker(p);
        }
    });
    map.addLayer(json);
    var feature = {
        type: 'Feature',
        properties: {
            id: 1,
            name: 'Point'
        },
        geometry: {
            'type': "Point",
            'coordinates': [lon, lat]
        }
    };
    json.addGeoJSON(feature);

    var converter = new GlobalMercator();
    var tile = new L.TileLayer.Canvas();
    tile.drawTile = function (canvas, tp, tz) {
        var bounds = converter.TileLatLonBounds(tp.x, tp.y, tz);
        
        var ctx = canvas.getContext('2d');
        ctx.strokeStyle = "#000000";
        ctx.strokeRect(0, 0, 256, 256);        
    };
    map.addLayer(tile);

    var center = new L.LatLng(lat, lon);
    map.setView(center, zoom);
})(jQuery));