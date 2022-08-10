$(document).ready(function () {
    var lon = -73.9529, lat = 40.7723, zoom = 10,
        map = new L.Map('map'), center, url, cloudmade;

    map.on('load', function (e) {
        var bounds = e.target.getBounds();
        var jsurl = [
            '/json.ashx?MAP_TYPE=DEF&BBOX=',
            bounds._southWest.lat, ',',
            bounds._southWest.lng, ',',
            bounds._northEast.lat, ',',
            bounds._northEast.lng
        ].join('');

        var options = {
            radius: 8,
            fillColor: "#FF7800",
            color: "#000000",
            weight: 1,
            opacity: 1,
            fillOpacity: 0.8
        };
        var layer = new L.GeoJSON(null, {
            pointToLayer: function (f) {
                var raw = f.geometry.coordinates;
                var ll = new L.LatLng(raw[1], raw[0]);
                return new L.CircleMarker(ll, options);
            },
            style: function (f) {
                var geom = f.geometry, type = geom.type;
                if (type === 'Polygon' || type === 'MultiPolygon') {
                    return {
                        color: 'rgb(0,0,180)',
                        weight: 4,
                        opacity: 0.6
                    };
                } else if (type === 'LineString' || type === 'MultiLineString') {
                    return {
                        color: 'rgb(180,0,0)',
                        weight: 1,
                        opacity: 0.9
                    };
                }
            }
        });
        $.getJSON(jsurl, function (evt) {
            layer.addData(evt.features);
            map.addLayer(layer);
        });
    });

    var center = new L.LatLng(lat, lon);
    map.setView(center, zoom);

    var osm = new L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    })
    map.addLayer(osm);
});
