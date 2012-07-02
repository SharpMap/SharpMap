/**
 * @copyright  2011 geOps
 * @license    https://github.com/geops/ole/blob/master/license.txt
 * @link       https://github.com/geops/ole
 */

/**
 * Class: OpenLayers.Editor.Control.CleanFeature
 * The Clean Feature control converts all selected features from a given layer 
 *     to a multipolygon and sends it as GeoJSON named "geo" to a server.
 *     The server whose url gets set by the contructor cleans the geometry and
 *     sends the result as GeoJSON named "geo" back to the client.
 *
 * Inherits from:
 *  - <OpenLayers.Control.Button>
 */
OpenLayers.Editor.Control.CleanFeature = OpenLayers.Class(OpenLayers.Control.Button, {

    proxy: null,

    title: OpenLayers.i18n('oleCleanFeature'),

    /**
     * Constructor: OpenLayers.Editor.Control.MergeFeature
     * Create a new control for merging features.
     *
     * Parameters:
     * layer - {<OpenLayers.Layer.Vector>}
     * options - {Object} An optional object whose properties will be used
     *     to extend the control.
     */
    initialize: function (layer, options) {
        this.layer = layer;
        OpenLayers.Control.Button.prototype.initialize.apply(this, [options]);
        this.trigger = this.cleanFeature;
        this.title = OpenLayers.i18n('oleCleanFeature');
        this.displayClass = "oleControlDisabled " + this.displayClass;
    },

    /**
     * Method: cleanFeature
     */
    cleanFeature: function () {
        if (this.layer.selectedFeatures.length > 0) {
            var wktFormat = new OpenLayers.Format.WKT(),
                geo = wktFormat.write(this.layer.selectedFeatures);
            this.map.editor.startWaiting(this.panel_div);
            OpenLayers.Request.POST({
                url: this.map.editor.oleUrl+'process/clean',
                data: OpenLayers.Util.getParameterString({geo: geo}),
                headers: {"Content-Type": "application/x-www-form-urlencoded"},
                callback: this.map.editor.requestComplete,
                proxy: this.proxy,
                scope: this.map.editor
            });
        }
    },
    
    CLASS_NAME: "OpenLayers.Editor.Control.CleanFeature"
});