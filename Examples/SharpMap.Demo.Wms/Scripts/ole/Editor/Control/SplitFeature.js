/**
 * @copyright  2011 geOps
 * @license    https://github.com/geops/ole/blob/master/license.txt
 * @link       https://github.com/geops/ole
 */

OpenLayers.Editor.Control.SplitFeature = OpenLayers.Class(OpenLayers.Control.DrawFeature, {

    proxy: null,

    title: OpenLayers.i18n('oleSplitFeature'),

    initialize: function(layer, options) {

        OpenLayers.Control.DrawFeature.prototype.initialize.apply(this,
            [layer, OpenLayers.Handler.Path, options]);

        this.events.register('activate', this, this.test);

        this.title = OpenLayers.i18n('oleSplitFeature');

        this.displayClass = "oleControlDisabled " + this.displayClass;

    },

    test: function() {
        if (this.layer.selectedFeatures.length < 1) {
            this.deactivate();
        }
    },

    /**
     * Method: split Features
     */
    drawFeature: function(geometry) {
        var feature = new OpenLayers.Feature.Vector(geometry);
        var wktFormat = new OpenLayers.Format.WKT();
        var proceed = this.layer.events.triggerEvent(
            'sketchcomplete', {feature: feature}
        );
        this.deactivate();
        if (proceed !== false) {
            if (this.layer.selectedFeatures.length > 0) {
                var geo = wktFormat.write(this.layer.selectedFeatures),
                    cut = wktFormat.write(feature);
                this.map.editor.startWaiting(this.panel_div);
                OpenLayers.Request.POST({
                    url: this.map.editor.oleUrl+'process/split',
                    data: OpenLayers.Util.getParameterString({cut: cut, geo: geo}),
                    headers: {"Content-Type": "application/x-www-form-urlencoded"},
                    callback: this.map.editor.requestComplete,
                    proxy: this.proxy,
                    scope: this.map.editor
                });
            }
        }
    },

    CLASS_NAME: 'OpenLayers.Editor.Control.SplitFeature'
});