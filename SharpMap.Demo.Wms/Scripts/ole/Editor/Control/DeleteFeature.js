/**
 * @copyright  2011 geOps
 * @license    https://github.com/geops/ole/blob/master/license.txt
 * @link       https://github.com/geops/ole
 */

/**
 * Class: OpenLayers.Editor.Control.DeleteFeature
 * The DeleteFeature provides a button to delete all selected features
 *     from a given layer.
 *
 * Inherits from:
 *  - <OpenLayers.Control.Button>
 */
OpenLayers.Editor.Control.DeleteFeature = OpenLayers.Class(OpenLayers.Control.Button, {

    /**
     * Property: layer
     * {<OpenLayers.Layer.Vector>}
     */
    layer: null,

    title: OpenLayers.i18n('oleDeleteFeature'),

    /**
     * Constructor: OpenLayers.Editor.Control.DeleteFeature
     * Create a new control for deleting features.
     *
     * Parameters:
     * layer - {<OpenLayers.Layer.Vector>} The layer from which selected
     *     features will be deleted.
     * options - {Object} An optional object whose properties will be used
     *     to extend the control.
     */
    initialize: function (layer, options) {

        this.layer = layer;

        this.title = OpenLayers.i18n('oleDeleteFeature');

        OpenLayers.Control.Button.prototype.initialize.apply(this, [options]);

        this.trigger = this.deleteFeature;

        this.displayClass = "oleControlDisabled " + this.displayClass;

    },

    /**
     * Method: deleteFeature
     */
    deleteFeature: function () {
        if (this.layer.selectedFeatures.length > 0) {
            this.layer.destroyFeatures(this.layer.selectedFeatures);
            this.layer.events.triggerEvent('featureunselected');
        }
    },

    CLASS_NAME: 'OpenLayers.Editor.Control.DeleteFeature'
});