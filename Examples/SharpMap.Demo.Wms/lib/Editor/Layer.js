OpenLayers.Editor.Layer = OpenLayers.Class(OpenLayers.Layer, {

    initialize: function (options) {
        OpenLayers.Control.prototype.initialize.apply(this, [options]);
    },

    CLASS_NAME: 'OpenLayers.Editor.Layer'
});
