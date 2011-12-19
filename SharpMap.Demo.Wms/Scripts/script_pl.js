$(document).ready(function() {
    var po = org.polymaps, mercator, container, map, load, layer;

    mercator = new GlobalMercator();
    container = $('#map').get(0).appendChild(po.svg('svg'));
    map = po.map()
        .container(container)
        .center({ lat: 40.7723, lon: -73.9529 })
        .zoom(10)
        .add(po.interact())
        .add(po.hash());
    map.container().setAttribute('class', 'PuBu');

    map.add(po.image().url(
        po.url(['http://{S}tile.cloudmade.com', '/1a1b06b230af4efdbb989ea99e9841af', '/998/256/{Z}/{X}/{Y}.png'].join(''))
            .hosts(['a.', 'b.', 'c.', ''])));

    load = function(e) {
        var type, node, parent;
        $.each(e.features, function() {
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
            parent = po.svg('title').appendChild(node).parentNode;
            this.element.appendChild(parent);
        });
    };

    layer = po.geoJson().url(function(data) {
        var size, bounds, url;
        size = map.tileSize();
        bounds = mercator.TileLatLonBounds(data.column, data.row, data.zoom)
        url = ['/wms.ashx?MAP_TYPE=PM&STYLES=&',
            'CRS=EPSG%3A4326&FORMAT=text%2Fjson&SERVICE=WMS&VERSION=1.3.0&REQUEST=GetMap&',
            'EXCEPTIONS=application%2Fvnd.ogc.se_inimage&transparent=true&',
            '&WIDTH=', size.x, '&HEIGHT=', size.y,
            '&LAYERS=poly_landmarks,tiger_roads,poi',
            '&BBOX=', bounds[1], ',', -bounds[2], ',', bounds[3], ',', -bounds[0]
        ].join('');
        log(url);
        return url;
    }).on('load', load);
    layer.container().setAttribute('class', 'poly_landmarks');
    map.add(layer);

    map.add(po.grid());
    map.add(po.compass().pan('short'));
});






















