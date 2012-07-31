/**
 * @copyright  2011 geOps
 * @license    https://github.com/geops/ole/blob/master/license.txt
 * @link       https://github.com/geops/ole
 */

/**
 * Class: OpenLayers.Editor.Control.SnappingSettings
 * ...
 *
 * Inherits from:
 *  - <OpenLayers.Control.Button>
 */
OpenLayers.Editor.Control.SnappingSettings = OpenLayers.Class(OpenLayers.Control.Button, {

    title: OpenLayers.i18n('oleSnappingSettings'),

    layer: null,

    snapping: new OpenLayers.Control.Snapping(),

    tolerance: 10,

    /**
     * @var {Array.<String>} Identifiers of checkboxes to enable snapping for individual layers
     */
    snappingLayers: null,

    /**
     * Layer that displays guide lines and snapping points
     * @var OpenLayers.Editor.Layer.Snapping
     */
    snappingGuideLayer: null,

    layerListDiv: null,

    toleranceInput: null,

    initialize: function(layer, options) {
        this.snappingLayers = [];
        this.layer = layer;

        OpenLayers.Control.Button.prototype.initialize.apply(this, [options]);
        
        this.trigger = OpenLayers.Function.bind(this.openSnappingDialog, this);

        this.events.register("deactivate", this, this.onDeactivate);

        this.title = OpenLayers.i18n('oleSnappingSettings');
    },

    onDeactivate: function() {
        if(this.snapping.active) {
            this.activate();
        }
    },

    openSnappingDialog: function() {

        var content, toleranceHeader, layerHeader;

        this.activate();

        this.layerListDiv = document.createElement('div');
        
        content = document.createElement('div');

        toleranceHeader = document.createElement('h4');
        toleranceHeader.innerHTML = OpenLayers.i18n('oleSnappingSettingsTolerance');
        content.appendChild(toleranceHeader);

        this.toleranceInput = document.createElement('input');
        this.toleranceInput.type = 'text';
        this.toleranceInput.size = 4;
        this.toleranceInput.value = this.tolerance;
        content.appendChild(this.toleranceInput);

        layerHeader = document.createElement('h4');
        layerHeader.innerHTML = OpenLayers.i18n('oleSnappingSettingsLayer');
        content.appendChild(layerHeader);

        content.appendChild(this.layerListDiv);

        this.map.editor.dialog.show({
            content: content,
            title: OpenLayers.i18n('oleSnappingSettings'),
            close: OpenLayers.Function.bind(this.changeSnapping, this)
        });

        this.redraw();
    },

    redraw: function() {

        var layer, element, content;

        this.layerListDiv.innerHTML = '';

        for (var i = 0; i <  this.map.layers.length; i++) {
            
            layer = this.map.layers[i];

            if(!(layer instanceof OpenLayers.Layer.Vector.RootContainer) &&
                 layer instanceof OpenLayers.Layer.Vector &&
                 !(layer instanceof OpenLayers.Editor.Layer.Snapping) &&
                 layer.name.search(/OpenLayers.Handler.+/) == -1) {

                content = document.createElement('div');

                element = document.createElement('input');
                element.type = 'checkbox';
                element.name = 'snappingLayer';
                element.id = 'Snapping.'+layer.id;
                element.value = 'true';
                if(this.snappingLayers.indexOf(layer) >= 0) {
                    element.checked = 'checked';
                    element.defaultChecked = 'selected'; // IE7 hack
                }
                content.appendChild(element);
                OpenLayers.Event.observe(element, 'click',
                    OpenLayers.Function.bind(this.setLayerSnapping, this, layer, element.checked));

                element = document.createElement('label');
                element.setAttribute('for', 'Snapping.'+layer.id);
                element.innerHTML = layer.name;
                OpenLayers.Event.observe(element, 'click', OpenLayers.Function.bind(function(event) {
                    // Allow to check checkbox by clicking its label even when drawing tools are active
                    OpenLayers.Event.stop(event, true);
                }, this));
                content.appendChild(element);

                this.layerListDiv.appendChild(content);
            }
        }
    },

    /**
     * Enables or disables a layer for snapping
     * @param {OpenLayers.Layer} layer
     * @param {Boolean} snappingEnabled Set TRUE to enable snapping to this layer's objects
     */
    setLayerSnapping: function(layer, snappingEnabled) {
        if(snappingEnabled) {
            this.snappingLayers.splice(this.snappingLayers.indexOf(layer), 1);
        } else {
            this.snappingLayers.push(layer);
        }
        this.redraw();
    },

    changeSnapping: function() {

        this.tolerance = parseInt(this.toleranceInput.value);

        if(this.snappingLayers.length > 0) {

            this.snapping.deactivate();
            var targets = [];
            for (var i = 0; i <  this.snappingLayers.length; i++) {
                targets.push({
                    layer: this.snappingLayers[i],
                    tolerance: this.tolerance
                });
            }
            this.snapping = new OpenLayers.Control.Snapping({
                layer: this.layer,
                targets: targets
            });
            for (var i = 0; i <  targets.length; i++) {
                // moveTo call is to trigger loading of layer contents
                targets[i].layer.moveTo(this.map.getExtent(), false, false);
            }
            this.snapping.activate();
        } else {
            if (this.snapping.active) {
                this.snapping.deactivate();
                this.snapping.targets = null;
            }
        }
        if (!this.snapping.active) this.deactivate();
    },

    setMap: function(map){
        OpenLayers.Control.Button.prototype.setMap.apply(this, arguments);

        if(this.snappingGuideLayer===null){
            this.snappingGuideLayer = this.createSnappingGuideLayer();
        }
    },

    /**
     * Adds a layer for guidelines to the map
     * @return {OpenLayers.Editor.Layer.Snapping}
     */
    createSnappingGuideLayer: function(){
        var snappingGuideLayer = new OpenLayers.Editor.Layer.Snapping(OpenLayers.i18n('Snapping Layer'), {
            visibility: false
        });
        this.map.addLayer(snappingGuideLayer);
        
        return snappingGuideLayer;
    },

    CLASS_NAME: "OpenLayers.Editor.Control.SnappingSettings"
});