/**
 * @copyright  2011 geOps
 * @license    https://github.com/geops/ole/blob/master/license.txt
 * @link       https://github.com/geops/ole
 */

/**
 * Class: OpenLayers.Editor.Control.ImportFeature
 * The ImportFeature provides a button to import all selected features
 *     to a given layer. Layers from which selected features will be imported
 *     must have a property exportFeature set to true.
 *
 * Inherits from:
 *  - <OpenLayers.Control.Button>
 */
OpenLayers.Editor.Control.ImportFeature = OpenLayers.Class(OpenLayers.Control.Button, {

    /**
     * Property: layer
     * {<OpenLayers.Layer.Vector>}
     */
    layer: null,

    title: OpenLayers.i18n('oleImportFeature'),

    /**
     * Constructor: OpenLayers.Editor.Control.DeleteFeature
     * Create a new control for importing features.
     *
     * Parameters:
     * layer - {<OpenLayers.Layer.Vector>} The layer to which selected
     *     features will be imported.
     * options - {Object} An optional object whose properties will be used
     *     to extend the control.
     */
    initialize: function (layer, options) {

        this.layer = layer;

        OpenLayers.Control.Button.prototype.initialize.apply(this, [options]);

        this.trigger = this.importFeature;

        this.title = OpenLayers.i18n('oleImportFeature');

        this.displayClass = "oleControlDisabled " + this.displayClass;

    },

    /**
     * Method: importFeature
     */
    importFeature: function () {
        
        var importFeatures = [];

        if (this.map.editor.sourceLayers.length > 0) {
            
            for (var i = 0, li = this.map.editor.sourceLayers.length; i < li; i++) {
                for (var j = 0, lj = this.map.editor.sourceLayers[i].selectedFeatures.length; j < lj; j++) {
                    this.map.editor.sourceLayers[i].selectedFeatures[j].renderIntent = 'default';
                    importFeatures.push(this.map.editor.sourceLayers[i].selectedFeatures[j]);
                }
                this.map.editor.sourceLayers[i].removeFeatures(this.map.editor.sourceLayers[i].selectedFeatures);
                this.map.editor.sourceLayers[i].events.triggerEvent('featureunselected');
            }

            if (importFeatures.length > 0) {

                this.layer.addFeatures(importFeatures);

            } else {
                return this.map.editor.showStatus('error', OpenLayers.i18n('oleImportFeatureSourceFeature'));
            }

        } else {
            return this.map.editor.showStatus('error', OpenLayers.i18n('oleImportFeatureSourceLayer'));
        }
    },

    CLASS_NAME: 'OpenLayers.Editor.Control.ImportFeature'
});