// code adapted from amazing d3 sample: http://mbostock.github.com/d3/talk/20111018/azimuthal.html
$(document).ready(function () {
    var m0, o0, features,
        projection = d3.geo.azimuthal()
            .scale(200000)
            .origin([-74.00, 40.70])
            .mode('orthographic')
            .translate([250, 650]),
        circle = d3.geo.greatCircle()
            .origin(projection.origin()),
        path = d3.geo.path()
            .projection(projection),
        svg = d3.select('#map').append('svg:svg')
            .attr('width', 1280)
            .attr('height', 800)
            .on("mousedown", function () {
                m0 = [d3.event.pageX, d3.event.pageY];
                o0 = projection.origin();
                d3.event.preventDefault();
            }),
        clip, mousemove;

    d3.json('/json.ashx?MAP_TYPE=DEF&LAYERS=poly_landmarks&BBOX=40,-74,41,-73', function (coll) {
        features = svg.selectAll('path')
            .data(coll.features)
            .enter()
            .append('svg:path')
            .attr('d', clip);
        features
            .append('svg:title')
            .text(function (d) {
                return d.properties.LANAME;
            });
    });

    clip = function (d) {
        var c = circle.clip(d);
        return path(c);
    };
    mousemove = function () {
        if (!m0) {
            return;
        }

        var m1 = [d3.event.pageX, d3.event.pageY],
            o1 = [o0[0] + (m0[0] - m1[0]) / 4000, o0[1] + (m1[1] - m0[1]) / 4000];
        projection.origin(o1);
        circle.origin(o1);
        features.attr('d', clip);
    };
    
    d3.select(window)
        .on("mousemove", mousemove)
        .on("mouseup", function () {
            if (m0) {
                mousemove();
                m0 = null;    
            }
        });
});