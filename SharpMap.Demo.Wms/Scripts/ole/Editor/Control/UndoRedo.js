/**
 * @copyright  2011 geOps
 * @license    https://github.com/geops/ole/blob/master/license.txt
 * @link       https://github.com/geops/ole
 */

/**
 *
 * Class: OpenLayers.Editor.Control.UndoRedo
 *
 * Inherits From:
 *  - <OpenLayers.Control>
 */
OpenLayers.Editor.Control.UndoRedo = OpenLayers.Class(OpenLayers.Control, {

    /**
     * Property: layer
     * {<OpenLayers.Layer.Vector>}
     */
    layer: null,

     /**
     * Property: handler
     * {<OpenLayers.Handler.Keyboard>}
     */
    handler: null,

     /**
     * APIProperty: autoActivate
     * {Boolean} Activate the control when it is added to a map.  Default is
     *     true.
     */
    autoActivate: true,

    /**
     * Constant: KEY_Z
     * {int}
     */
    KEY_Z: 90,

    /**
     * Constant: KEY_Y
     * {int}
     */
    KEY_Y: 89,

	/**
     * APIMethod: onUndo
     *
     * Called after a successful undo, passing in the feature that was altered.
     */
	onUndo: function(){},

	/**
     * APIMethod: onRedo
     *
     * Called after a successful redo, passing in the feature that was altered.
     */
	onRedo: function(){},

	/**
     * APIMethod: onRemoveFeature
     *
     * Called when the Undo/Redo control is about to remove a feature from the layer. This call happens before the feature is removed.
     */
	onRemoveFeature: function(){},

	/**
     * Property: undoStack
     * {<Array>}
     *
     * A stack containing states of a feature that can be undone. Objects on this stack are hashes, of the form {feature: ..., :geometry ...}.
     */
    undoStack: null,

 	/**
     * Property: redoStack
     * {<Array>}
     *
     * A stack containing states of a feature that can be redone. Objects on this stack are hashes, of the form {feature: ..., :geometry ...}.
     */
    redoStack: null,

    /**
     * Property: currentState
     */
    currentState: null,

    /**
     * Constructor: OpenLayers.Control.UndoRedo
     * Create a new Undo/Redo control.
     *
     * Parameters:
     * layer - {<OpenLayers.Layer.Vector>} The layer from which selected
     *     features will be deleted.
     * options - {Object} An optional object whose properties will be used
     *     to extend the control.
     */
    initialize: function(layer, options) {

        this.layer = layer;

        OpenLayers.Control.prototype.initialize.apply(this, [options]);

        this.layer.events.register('featureadded', this, this.register);
        this.layer.events.register('afterfeaturemodified', this, this.register);

        this.undoStack = new Array();
        this.redoStack = new Array();
    },

    /**
     * Method: draw
     * Activates the control.
     */
    draw: function() {
        this.handler = new OpenLayers.Handler.Keyboard( this, {
                "keydown": this.handleKeydown} );
    },

    /**
     * Method: handleKeydown
     * Called by the feature handler on keydown.
     *
     * Parameters:
     * {Integer} Key code corresponding to the keypress event.
     */
    handleKeydown: function(e) {
        if (e.keyCode === this.KEY_Z && e.ctrlKey === true && e.shiftKey === false) {
            this.undo();
        }
        else if (e.ctrlKey === true && ((e.keyCode === this.KEY_Y) || (e.keyCode === this.KEY_Z && e.shiftKey === true))) {
            this.redo();
        }
    },

    /**
     * APIMethod: undo
     * Causes an the Undo/Redo control to process an undo.
     */
    undo: function() {
        var feature = this.moveBetweenStacks(this.undoStack, this.redoStack, true);
        if (feature) this.onUndo(feature);
    },

    /**
     * APIMethod: redo
     * Causes an the Undo/Redo control to process an undo.
     */
    redo: function() {
        var feature = this.moveBetweenStacks(this.redoStack, this.undoStack, false);
        if (feature) this.onRedo(feature);
    },

    /**
     * Method: moveBetweenStacks
     * The "meat" of the Undo/Redo control -- it actually does the undoing/redoing. Although some idiosyncrasies exist, this function
     * handles moving states from the undo stack to the redo stack, and vice versa. It also handles adding and removing features from the map.
     *
     * Parameters: TODO
     */
    moveBetweenStacks: function(fromStack, toStack, undo) {

        if (fromStack.length > 0) {

            this.map.editor.editLayer.removeAllFeatures();
            var state = fromStack.pop();
            toStack.push(this.currentState);

            if (state) {
                var currentFeatures = new Array(len);
                var len = state.length;
                for(var i=0; i<len; ++i) {
                    currentFeatures[i] = state[i].clone();
                }
                this.currentState = currentFeatures;
                this.map.editor.editLayer.addFeatures(state, {silent: true});
            } else {
                this.currentState = null;
            }
        }
        else if (this.currentState && undo) {
            toStack.push(this.currentState);
            this.map.editor.editLayer.removeAllFeatures();
            this.currentState = null;
        }
    },

    /**
     * 
     */
    register: function() {

        var features = this.map.editor.editLayer.features;
        var len = features.length;
        var clonedFeatures = new Array(len);
        for(var i=0; i<len; ++i) {
            clonedFeatures[i] = features[i].clone();
        }

        if (this.currentState) {
            this.undoStack.push(this.currentState);
        }

        this.currentState = clonedFeatures;
        this.redoStack = new Array();

    },

    CLASS_NAME: "OpenLayers.Editor.Control.UndoRedo"
});
