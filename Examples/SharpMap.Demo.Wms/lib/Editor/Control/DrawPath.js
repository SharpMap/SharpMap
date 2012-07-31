/**
 * @copyright  2011 geOps
 * @license    https://github.com/geops/ole/blob/master/license.txt
 * @link       https://github.com/geops/ole
 */

/**
 * Class: OpenLayers.Editor.Control.DrawPath
 *
 * Inherits from:
 *  - <OpenLayers.Control.DrawFeature>
 */
OpenLayers.Editor.Control.DrawPath = OpenLayers.Class(OpenLayers.Control.DrawFeature, {

    /**
     * Property: minLength
     * {Number} Minimum length of new paths.
     */
    minLength: 0,

    title: OpenLayers.i18n('oleDrawPath'),

    /**
     * Constructor: OpenLayers.Editor.Control.DrawPath
     * Create a new control for drawing paths.
     *
     * Parameters:
     * layer - {<OpenLayers.Layer.Vector>} Paths will be added to this layer.
     * options - {Object} An optional object whose properties will be used
     *     to extend the control.
     */
    initialize: function (layer, options) {
        this.callbacks = OpenLayers.Util.extend(this.callbacks, {
            point: function(point) {
                this.layer.events.triggerEvent('pointadded', {point: point});
            }
        });
        
        OpenLayers.Control.DrawFeature.prototype.initialize.apply(this,
            [layer, OpenLayers.Handler.Path, options]);
        
        this.title = OpenLayers.i18n('oleDrawPath');
    },

    /**
     * Method: draw path only if area greater than or equal to minLength
     */
    drawFeature: function (geometry) {
        var feature = new OpenLayers.Feature.Vector(geometry),
            proceed = this.layer.events.triggerEvent('sketchcomplete', {feature: feature});
        if (proceed !== false && geometry.getLength() >= this.minLength) {
            feature.state = OpenLayers.State.INSERT;
            this.layer.addFeatures([feature]);
            this.featureAdded(feature);
            this.events.triggerEvent('featureadded', {feature : feature});
        }
    },

    CLASS_NAME: 'OpenLayers.Editor.Control.DrawPath'
});