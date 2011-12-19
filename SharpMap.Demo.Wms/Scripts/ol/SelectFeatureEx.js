OpenLayers.Control.SelectFeatureEx = OpenLayers.Class(OpenLayers.Control.SelectFeature, {

    initialize: function(layers, options) {
        OpenLayers.Control.SelectFeature.prototype.initialize.apply(this, [layers, options]);
        this.events.addEventType('clickFeature');
        this.then();
    },

    clickFeature: function(feature) {
        OpenLayers.Control.SelectFeature.prototype.clickFeature.apply(this, [feature]);
        this.events.triggerEvent('clickFeature', { feature: feature });
    },

    then: function() {
        var parent = this;
        this.events.toObservable('activate')
            .Subscribe(function() {
                parent.events.toObservable('featurehighlighted')
                    .Select(function(e) {
                        return e.feature;
                    })
                    .Subscribe(function(f) {
                        f.popup = new OpenLayers.Popup.Anchored(
                            f.id,
                            parent.pos(),
                            parent.size(f),
                            parent.html(f));
                        parent.map.addPopup(f.popup);
                    });
                parent.events.toObservable('featureunhighlighted')
                    .Select(function(e) {
                        return e.feature;
                    })
                    .Subscribe(function(f) {
                        if (f && f.popup) {
                            parent.map.removePopup(f.popup);
                            f.popup.destroy();
                            f.popup = null;
                        }
                    });
            });
    },

    html: function(feature) {
        var html = ['<div class="featurebubble">', '<h1>', 'info', '</h1>'];
        html.push('<div class="featurebubblefid">', feature.fid, '</div>');
        html.push('<br />');
        for (var key in feature.attributes) {
            html.push('<div class="featurebubbleattribute">',
                '<strong>', key, '</strong>', ': ',
                feature.attributes[key], '</div>');
        }
        html.push('</div>');
        return html.join('');
    },

    size: function(feature) {
        var height = 50;
        for (var key in feature.attributes) {
            height += 15;
        }
        return new OpenLayers.Size(350, height);
    },

    pos: function() {
        var extent = this.map.getExtent();
        var res = this.map.getResolution();
        var pos = new OpenLayers.LonLat(
            extent.right + (100 * res),
            extent.bottom + (10 * res));
        return pos;
    },

    CLASS_NAME: 'OpenLayers.Control.SelectFeatureEx'
});