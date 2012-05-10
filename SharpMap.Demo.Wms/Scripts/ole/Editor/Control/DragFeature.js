/**
 * @copyright  2011 geOps
 * @license    https://github.com/geops/ole/blob/master/license.txt
 * @link       https://github.com/geops/ole
 */

/**
 * Class: OpenLayers.Editor.Control.DragFeature
 * 
 * Inherits from:
 *  - <OpenLayers.Control.DragFeature>
 */
OpenLayers.Editor.Control.DragFeature = OpenLayers.Class(OpenLayers.Control.DragFeature, {
    title: OpenLayers.i18n('oleDragFeature'),
    EVENT_TYPES: ["activate", "deactivate", 'dragstart', 'dragdrag', 'dragcomplete', 'dragenter', 'dragleave'],
    
    initialize: function(layer, options) {
        OpenLayers.Control.DragFeature.prototype.initialize.apply(this, [layer, options]);
        // allow changing the layer title by using translations
        this.title = OpenLayers.i18n('oleDragFeature');
    },
    
    // Add events corresponding to callbacks of OpenLayers.Control.DragFeature
    onStart: function(feature, pixel){
        this.events.triggerEvent('dragstart', {
            feature: feature,
            pixel: pixel
        });
    },
    onDrag: function(feature, pixel){
        this.events.triggerEvent('dragdrag', {
            feature: feature,
            pixel: pixel
        });
    },
    onComplete: function(feature, pixel) {
        this.events.triggerEvent('dragcomplete', {
            feature: feature,
            pixel: pixel
        });
        // General event is there so that undo-redo control works for all controls
        this.layer.events.triggerEvent('afterfeaturemodified', {
            feature: feature
        });
    },
    onEnter: function(feature){
        this.events.triggerEvent('dragenter', {
            feature: feature
        });
    },
    onLeave: function(feature){
        this.events.triggerEvent('dragleave', {
            feature: feature
        });
    },
    
    CLASS_NAME: "OpenLayers.Editor.Control.DragFeature"
});
