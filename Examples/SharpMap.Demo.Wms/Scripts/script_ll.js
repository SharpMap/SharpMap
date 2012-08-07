$(document).ready(function () {
    var lon = -73.9529, lat = 40.7723, zoom = 10,
        map = new L.Map('map'), center, url, cloudmade;

    map.on('load', function (e) {
        var bounds, jsurl, options, layer;
        bounds = e.target.getBounds();
        jsurl = [
            '/json.ashx?MAP_TYPE=PM&BBOX=',
            bounds._southWest.lat, ',',
            bounds._southWest.lng, ',',
            bounds._northEast.lat, ',',
            bounds._northEast.lng
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

    center = new L.LatLng(lat, lon);
    map.setView(center, zoom);

    url = ['http://{s}tile.cloudmade.com', '/1a235b638b614b458deeb77c7dae4f80', '/998/256/{z}/{x}/{y}.png'].join('');
    cloudmade = new L.TileLayer(url, {
        maxZoom: 18,
        subdomains: ['a.', 'b.', 'c.', '']
    });
    map.addLayer(cloudmade);


});