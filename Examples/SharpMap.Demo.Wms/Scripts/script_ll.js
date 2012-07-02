$(document).ready(function () {
    var lon = -73.9529;
    var lat = 40.7723;
    var zoom = 10;

    var map = new L.Map('map'), center, url, cloudmade;
    map.on('load', function (e) {
        var bounds, url, options, layer, type;
        bounds = e.target.getBounds();
        url = [
           '/json.ashx?MAP_TYPE=PM&BBOX=',
            bounds._southWest.lng, ',',
            bounds._southWest.lat, ',',
            bounds._northEast.lng, ',',
            bounds._northEast.lat
        ].join('');

        options = {
            radius: 8,
            fillColor: "#FF7800",
            color: "#000000",
            weight: 1,
            opacity: 1,
            fillOpacity: 0.8
        };
        layer = new L.GeoJSON(null, {
            pointToLayer: function (p) {
                return new L.CircleMarker(p, options);
            }
        });
        layer.on('featureparse', function (e) {
            type = e.geometryType;
            if (type === 'Polygon' || type === 'MultiPolygon') {
                e.layer.setStyle({
                    color: 'rgb(0,0,180)',
                    weight: 4,
                    opacity: 0.6
                });
            }
            else if (type === 'LineString' || type === 'MultiLineString') {
                e.layer.setStyle({
                    color: 'rgb(180,0,0)',
                    weight: 1,
                    opacity: 0.9
                });
            }
        });
        $.getJSON(url, function (e) {
            $.each(e.features, function () {
                layer.addGeoJSON(this);
            });
            map.addLayer(layer);
        });
    });

    center = new L.LatLng(lat, lon);
    map.setView(center, zoom);

    url = ['http://{s}tile.cloudmade.com', '/1a235b638b614b458deeb77c7dae4f80', '/998/256/{z}/{x}/{y}.png'].join('');
    cloudmade = new L.TileLayer(url, {
         maxZoom: 18,
         subdomains: ['a.', 'b.', 'c.', '']
    });
    map.addLayer(cloudmade);


});






















