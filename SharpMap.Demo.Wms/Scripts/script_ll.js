$(document).ready(function() {
    var lon = -73.9529;
    var lat = 40.7723;
    var zoom = 10;

    var map = new L.Map('map'), center, cloudmade;
    map.on('load', function(e) {
        var bounds, url, options, layer, type;
        bounds = e.target.getBounds();
        url = [
           '/wms.ashx?MAP_TYPE=PM&HEIGHT=256&WIDTH=256&STYLES=&',
            'CRS=EPSG%3A4326&FORMAT=text%2Fjson&SERVICE=WMS&VERSION=1.3.0&REQUEST=GetMap&',
            'EXCEPTIONS=application%2Fvnd.ogc.se_inimage&transparent=true&',
            'LAYERS=poly_landmarks,tiger_roads,poi',
            '&BBOX=', bounds._southWest.lng, ',', bounds._southWest.lat, ',', bounds._northEast.lng, ',', bounds._northEast.lat
        ].join('');

        options = {
            radius: 8,
            fillColor: "#ff7800",
            color: "#000",
            weight: 1,
            opacity: 1,
            fillOpacity: 0.8
        };
        layer = new L.GeoJSON(null, {
            pointToLayer: function(p) {
                return new L.CircleMarker(p, options);
            }
        });
        layer.on('featureparse', function(e) {
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
        $.getJSON(url, function(e) {
            $.each(e.features, function() {
                layer.addGeoJSON(this);
            })
            map.addLayer(layer);
        });
    });

    center = new L.LatLng(lat, lon);
    map.setView(center, zoom);

    cloudmade = new L.TileLayer('http://{s}.tile.cloudmade.com/1a1b06b230af4efdbb989ea99e9841af/997/256/{z}/{x}/{y}.png', { maxZoom: 18 });
    map.addLayer(cloudmade);


});






















