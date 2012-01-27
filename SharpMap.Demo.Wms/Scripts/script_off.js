$(document).ready(function () {
    var po = org.polymaps, mercator, container, map, load, queue, layer;

    $('#cleardb').click(function () {
        webdb.open();
        webdb.dropTable();
        webdb.createTable();
    });

    mercator = new GlobalMercator();
    container = $('#map').get(0).appendChild(po.svg('svg'));
    map = po.map()
        .container(container)
        .center({ lat: 40.7723, lon: -73.9529 })
        .zoom(10)
        .add(po.interact())
        .add(po.hash());

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

    queue = sharpmap.queue;
    queue.init();

    layer = po.geoJson(queue.json).url(function (data) {
        var bounds, url;
        bounds = mercator.TileLatLonBounds(data.column, data.row, data.zoom);
        url = ['/json.ashx?MAP_TYPE=PM&BBOX=', bounds[1], ',', -bounds[2], ',', bounds[3], ',', -bounds[0]
        ].join('');
        return url;
    }).on('load', load);
    map.add(layer);

    map.add(po.grid());
    map.add(po.compass().pan('short'));
});






















