/**
 * @copyright  2011 geOps
 * @license    https://github.com/geops/ole/blob/master/license.txt
 * @link       https://github.com/geops/ole
 */

/**
 * Class: OpenLayers.Editor.Control.MergeFeature
 * ...
 *
 * Inherits from:
 *  - <OpenLayers.Control.Button>
 */
OpenLayers.Editor.Control.MergeFeature = OpenLayers.Class(OpenLayers.Control.Button, {

    proxy: null,

    title: OpenLayers.i18n('oleMergeFeature'),

    /**
     * Constructor: OpenLayers.Editor.Control.MergeFeature
     * Create a new control for merging features.
     *
     * Parameters:
     * layer - {<OpenLayers.Layer.Vector>}
     * options - {Object} An optional object whose properties will be used
     *     to extend the control.
     */
    initialize: function(layer, options) {

        this.layer = layer;

        OpenLayers.Control.Button.prototype.initialize.apply(this, [options]);

        this.trigger = this.mergeFeature;

        this.title = OpenLayers.i18n('oleMergeFeature');

        this.displayClass = "oleControlDisabled " + this.displayClass;

    },

    /**
     * Method: mergeFeature
     */
    mergeFeature: function () {
        if (this.layer.selectedFeatures.length < 2) {
            this.map.editor.showStatus('error', OpenLayers.i18n('oleMergeFeatureSelectFeature'));
        } else {
            var wktFormat = new OpenLayers.Format.WKT(),
                geo = wktFormat.write(this.layer.selectedFeatures);
            this.map.editor.startWaiting(this.panel_div);
            OpenLayers.Request.POST({
                url: this.map.editor.oleUrl+'process/merge',
                data: OpenLayers.Util.getParameterString({geo: geo}),
                headers: {"Content-Type": "application/x-www-form-urlencoded"},
                callback: this.map.editor.requestComplete,
                proxy: this.proxy,
                scope: this.map.editor
            });
        }
    },
    
    CLASS_NAME: "OpenLayers.Editor.Control.MergeFeature"
});