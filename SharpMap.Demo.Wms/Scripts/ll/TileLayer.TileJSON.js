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
        var g = ctx.canvas.getContext('2d');
        g.beginPath();
        g.fillStyle = this.options.point.fill;
        g.fillRect(p.x - 5, p.y - 5, 10, 10);
        g.closePath();
    },

    _drawLineString: function (ctx, geom) {
        var g = ctx.canvas.getContext('2d');
        g.strokeStyle = this.options.linestring.fill;
        g.beginPath();
        for (var i = 0; i < geom.length; i++) {
            var coord = geom[i];
            var p = this._tilePoint(ctx, coord);
            g.lineTo(p.x, p.y);
        }
        g.stroke();
        g.closePath();
    },

    _drawPolygon: function (ctx, geom) {
        var g = ctx.canvas.getContext('2d');
        g.fillStyle = this.options.polygon.fill;
        g.beginPath();
        for (var i = 0; i < geom.length; i++) {
            var coords = geom[i];
            for (var j = 0; j < coords.length; j++) {
                var coord = coords[j];
                var p = this._tilePoint(ctx, coord);
                g.lineTo(p.x, p.y);
            }
            g.fill();
            g.closePath();
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
        var self = this;        
        loader(url, function (data) {
            for (var i = 0; i < data.features.length; i++) {
                var feature = data.features[i];
                var type = feature.geometry.type;
                if (type == 'Point') {
                    self._drawPoint(ctx, feature.geometry.coordinates);
                }
                else if (type == 'MultiPoint') {
                    for (var j1 = 0; j1 < feature.geometry.coordinates.length; j1++) {
                        var point = feature.geometry.coordinates[j1];
                        self._drawPoint(ctx, point);
                    }
                }
                else if (type == 'LineString') {
                    self._drawLineString(ctx, feature.geometry.coordinates);
                }
                else if (type == 'MultiLineString') {
                    for (var j2 = 0; j2 < feature.geometry.coordinates.length; j2++) {
                        var ls = feature.geometry.coordinates[j2];
                        self._drawLineString(ctx, ls);
                    }
                }
                else if (type == 'Polygon') {
                    self._drawPolygon(ctx, feature.geometry.coordinates);
                }
                else if (type == 'MultiPolygon') {
                    for (var j3 = 0; j3 < feature.geometry.coordinates.length; j3++) {
                        var pol = feature.geometry.coordinates[j3];
                        self._drawPolygon(ctx, pol);
                    }
                }
                else {
                    console.log('Unmanaged type: ' + type);
                }
            }
        });
    },

    // NOTE: a placeholder for a function that, given a tile context, returns a string to a GeoJSON service that retrieve features for that context
    createUrl: function (bounds) {
        // override with your code
    }
});