$(document).ready(function() {
    var po = org.polymaps;

    var mercator = new GlobalMercator();
    var container = $("#map").get(0).appendChild(po.svg("svg"));
    var layers = ['poly_landmarks', 'tiger_roads', 'poi'].join()    
    var map = po.map()
        .container(container)
        .center({ lat: 40.7723, lon: -73.9529 })
        .zoom(10)
        .add(po.interact())
        .add(po.hash());

    map.add(po.image().url(
        po.url(["http://{S}tile.cloudmade.com", "/1a1b06b230af4efdbb989ea99e9841af", "/998/256/{Z}/{X}/{Y}.png"].join(''))
        .hosts(["a.", "b.", "c.", ""])));    
    map.add(po.image().url(function(data) {
        var bounds = mercator.TileBounds(data.column, data.row, data.zoom)
        var url = ['/wms.ashx?HEIGHT=256&WIDTH=256&STYLES=&',
            'CRS=EPSG%3A900913&FORMAT=image%2Fpng&SERVICE=WMS&VERSION=1.3.0&REQUEST=GetMap&',
            'EXCEPTIONS=application%2Fvnd.ogc.se_inimage&transparent=true&',
            'LAYERS=', layers, '&', 'BBOX=', bounds[0], ',', -bounds[3], ',', bounds[2], ',', -bounds[1]
        ].join('')
        log(url);
        return url;
    }));
    map.add(po.grid());
    map.add(po.compass().pan('short'));
});






















