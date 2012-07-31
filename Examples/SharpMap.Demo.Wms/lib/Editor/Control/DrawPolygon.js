/**
 * @copyright  2011 geOps
 * @license    https://github.com/geops/ole/blob/master/license.txt
 * @link       https://github.com/geops/ole
 */

/**
 * Class: OpenLayers.Editor.Control.DrawPolygon
 * The DeleteFeature provides a button to delete all selected features
 *     from a given layer.
 *
 * Inherits from:
 *  - <OpenLayers.Control.DrawFeature>
 */
OpenLayers.Editor.Control.DrawPolygon = OpenLayers.Class(OpenLayers.Control.DrawFeature, {

    /**
     * Property: minArea
     * {Number} Minimum area of new polygons.
     */
    minArea: 0,

    title: OpenLayers.i18n('oleDrawPolygon'),

    /**
     * Constructor: OpenLayers.Editor.Control.DrawPolygon
     * Create a new control for drawing polygons.
     *
     * Parameters:
     * layer - {<OpenLayers.Layer.Vector>} Polygons will be added to this layer.
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
            [layer, OpenLayers.Handler.Polygon, options]);

        this.title = OpenLayers.i18n('oleDrawPolygon');
    },

    /**
     * Method: draw polygon only if area greater than or equal to minArea
     */
    drawFeature: function (geometry) {
        var feature = new OpenLayers.Feature.Vector(geometry),
            proceed = this.layer.events.triggerEvent('sketchcomplete', {feature: feature});
        if (proceed !== false && geometry.getArea() >= this.minArea) {
            feature.state = OpenLayers.State.INSERT;
            this.layer.addFeatures([feature]);
            this.featureAdded(feature);
            this.events.triggerEvent('featureadded', {feature : feature});
        }
    },

    CLASS_NAME: 'OpenLayers.Editor.Control.DrawPolygon'
});