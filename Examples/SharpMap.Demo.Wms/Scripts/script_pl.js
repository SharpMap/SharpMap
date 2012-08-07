$(document).ready(function () {
    var po = org.polymaps, mercator, container, map, load, layer;

    mercator = new GlobalMercator();
    container = $('#map').get(0).appendChild(po.svg('svg'));
    map = po.map()
        .container(container)
        .center({ lat: 40.7723, lon: -73.9529 })
        .zoom(10)
        .add(po.interact())
        .add(po.hash());

    map.add(po.image().url(
        po.url(['http://{S}tile.cloudmade.com', '/1a235b638b614b458deeb77c7dae4f80', '/998/256/{Z}/{X}/{Y}.png'].join(''))
            .hosts(['a.', 'b.', 'c.', ''])));

    load = function (e) {
        $.each(e.features, function () {
            var type, node, parent;
            type = this.data.geometry.type;
            if (type === 'Polygon' || type === 'MultiPolygon') {
                this.element.setAttribute('class', 'poly');
                node = document.createTextNode(this.data.properties.LANAME);
            }
            else if (type === 'LineString' || type === 'MultiLineString') {
                this.element.setAttribute('class', 'line');
                node = document.createTextNode(this.data.properties.NAME);
            }
            else if (type === 'Point' || type === 'MultiPoint') {
                this.element.setAttribute('class', 'point');
                node = document.createTextNode(this.data.properties.NAME);
            }

            if (node != null) {
                parent = po.svg('title').appendChild(node).parentNode;
                this.element.appendChild(parent);
            }
        });
    };

    layer = po.geoJson().url(function (data) {
        var bounds, url;
        bounds = mercator.TileLatLonBounds(data.column, data.row, data.zoom);
        url = [
            '/json.ashx?MAP_TYPE=PM&BBOX=',
           -bounds[2], ',',
            bounds[1], ',',
           -bounds[0], ',',
            bounds[3]
        ].join('');
        return url;
    }).on('load', load);
    map.add(layer);

    map.add(po.grid());
    map.add(po.compass().pan('short'));
});






















