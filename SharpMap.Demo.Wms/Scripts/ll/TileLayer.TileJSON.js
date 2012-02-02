L.TileLayer.TileJSON = L.TileLayer.Canvas.extend({
    options: {
        debug: false
    },

    tileSize: 256,

    initialize: function (options) {
        L.Util.setOptions(this, options);

        this.drawTile = function (canvas, tilePoint, zoom) {
            var ctx = {
                canvas: canvas,
                tile: tilePoint,
                zoom: zoom
            };

            if (this.options.debug) {
                this._drawDebugInfo(ctx);
            }
            this._draw(ctx);
        };
    },

    _drawDebugInfo: function (ctx) {
        var max = this.tileSize;
        var g = ctx.canvas.getContext('2d');
        g.beginPath();
        g.strokeStyle = '#000000';
        g.strokeRect(0, 0, max, max);
        g.fillStyle = '#FFFF00';
        g.fillRect(0, 0, 5, 5);
        g.fillRect(0, max - 5, 5, 5);
        g.fillRect(max - 5, 0, 5, 5);
        g.fillRect(max - 5, max - 5, 5, 5);
        g.fillRect(max / 2 - 5, max / 2 - 5, 10, 10);
        g.font = "12px Arial";
        g.strokeText(ctx.tile.x + ' ' + ctx.tile.y + ' ' + ctx.zoom, max / 2 - 30, max / 2 - 10);

        g.closePath();
    },

    _tilePoint: function (ctx, coords) {
        // start coords to tile 'space'
        var s = ctx.tile.multiplyBy(this.tileSize);

        // actual coords to tile 'space'
        var p = this._map.project(new L.LatLng(coords[1], coords[0]));

        // point to draw        
        var x = Math.round(p.x - s.x);
        var y = Math.round(p.y - s.y);
        return {
            x: x,
            y: y
        };
    },

    _drawPoint: function (ctx, geom) {
        var p = this._tilePoint(ctx, geom);
        var style = this.options.point;
        var c = ctx.canvas;
        var g = c.getContext('2d');
        g.beginPath();
        g.fillStyle = style.color;
        g.arc(p.x, p.y, style.radius, 0, Math.PI * 2);
        g.closePath();
        g.fill();
        g.restore();        
    },

    _clip: function (ctx, points) {
        var nw = ctx.tile.multiplyBy(this.tileSize);
        var se = nw.add(new L.Point(this.tileSize, this.tileSize));
        var bounds = new L.Bounds([nw, se]);
        var len = points.length;
        var out = [];

        for (var i = 0; i < len - 1; i++) {
            var seg = L.LineUtil.clipSegment(points[i], points[i + 1], bounds, i);
            if (!seg) {
                continue;
            }
            out.push(seg[0]);
            // if segment goes out of screen, or it's the last one, it's the end of the line part
            if ((seg[1] !== points[i + 1]) || (i === len - 2)) {
                out.push(seg[1]);
            }
        }
        return out;
    },

    _drawLineString: function (ctx, geom) {
        var coords = geom;
        coords = this._clip(ctx, coords);
        coords = L.LineUtil.simplify(coords, 1);
        var style = this.options.linestring;
        var g = ctx.canvas.getContext('2d');
        g.strokeStyle = style.color;
        g.lineWidth = style.size;
        g.beginPath();
        for (var i = 0; i < coords.length; i++) {
            var coord = coords[i];
            var p = this._tilePoint(ctx, coord);
            var method = (i === 0 ? 'move' : 'line') + 'To';
            g[method](p.x, p.y);
        }
        g.stroke();
        g.restore();
    },

    _drawPolygon: function (ctx, geom) {
        var style = this.options.polygon;
        var outline = style.outline;
        var g = ctx.canvas.getContext('2d');
        for (var i = 0; i < geom.length; i++) {
            g.fillStyle = style.color;
            if (outline) {
                g.strokeStyle = outline.color;
                g.lineWidth = outline.size;
            }
            g.beginPath();
            var coords = geom[i];
            coords = this._clip(ctx, coords);
            for (var j = 0; j < coords.length; j++) {
                var coord = coords[j];
                var p = this._tilePoint(ctx, coord);
                var method = (j === 0 ? 'move' : 'line') + 'To';
                g[method](p.x, p.y);
            }
            g.closePath();
            g.fill();
            if (outline) {
                g.stroke();
            }
            g.restore();
        }
    },

    _draw: function (ctx) {
        // NOTE: this is the only part of the code that depends from external libraries (actually, jQuery only).        
        var loader = $.getJSON;

        var nwPoint = ctx.tile.multiplyBy(this.tileSize);
        var sePoint = nwPoint.add(new L.Point(this.tileSize, this.tileSize));
        var nwCoord = this._map.unproject(nwPoint, ctx.zoom, true);
        var seCoord = this._map.unproject(sePoint, ctx.zoom, true);
        var bounds = [nwCoord.lng, seCoord.lat, seCoord.lng, nwCoord.lat];

        var url = this.createUrl(bounds);
        var self = this, j;
        loader(url, function (data) {
            for (var i = 0; i < data.features.length; i++) {
                var feature = data.features[i];
                var type = feature.geometry.type;
                var geom = feature.geometry.coordinates;
                var len = geom.length;

                switch (type) {
                    case 'Point':
                        self._drawPoint(ctx, geom);
                        break;

                    case 'MultiPoint':
                        for (j = 0; j < len; j++) {
                            self._drawPoint(ctx, geom[j]);
                        }
                        break;

                    case 'LineString':
                        self._drawLineString(ctx, geom);
                        break;

                    case 'MultiLineString':
                        for (j = 0; j < len; j++) {
                            self._drawLineString(ctx, geom[j]);
                        }
                        break;

                    case 'Polygon':
                        self._drawPolygon(ctx, geom);
                        break;

                    case 'MultiPolygon':
                        for (j = 0; j < len; j++) {
                            self._drawPolygon(ctx, geom[j]);
                        }
                        break;

                    default:
                        throw new Error('Unmanaged type: ' + type);
                }
            }
        });
    },

    // NOTE: a placeholder for a function that, given a tile context, returns a string to a GeoJSON service that retrieve features for that context
    createUrl: function (bounds) {
        // override with your code
    }
});