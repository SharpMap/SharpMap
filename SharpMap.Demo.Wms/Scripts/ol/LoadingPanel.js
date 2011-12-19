/* Copyright (c) 2006-2008 MetaCarta, Inc., published under the Clear BSD
 * license.  See http://svn.openlayers.org/trunk/openlayers/license.txt for the
 * full text of the license. */

/**
 * @requires OpenLayers/Control.js
 *
 * Class: OpenLayers.Control.LoadingPanel
 * In some applications, it makes sense to alert the user that something is 
 * happening while tiles are loading. This control displays a div across the 
 * map when this is going on.
 *
 * Inherits from:
 *  - <OpenLayers.Control>
 */
OpenLayers.Control.LoadingPanel = OpenLayers.Class(OpenLayers.Control, {

    /**
     * Property: counter
     * {Integer} A counter for the number of layers loading
     */ 
    counter: 0,

    /**
     * Property: maximized
     * {Boolean} A boolean indicating whether or not the control is maximized
    */
    maximized: false,

    /**
     * Property: visible
     * {Boolean} A boolean indicating whether or not the control is visible
    */
    visible: true,

    /**
     * Constructor: OpenLayers.Control.LoadingPanel
     * Display a panel across the map that says 'loading'. 
     *
     * Parameters:
     * options - {Object} additional options.
     */
    initialize: function(options) {
         OpenLayers.Control.prototype.initialize.apply(this, [options]);
    },

    /**
     * Function: setVisible
     * Set the visibility of this control
     *
     * Parameters:
     * visible - {Boolean} should the control be visible or not?
    */
    setVisible: function(visible) {
        this.visible = visible;
        if (visible) {
            OpenLayers.Element.show(this.div);
        } else {
            OpenLayers.Element.hide(this.div);
        }
    },

    /**
     * Function: getVisible
     * Get the visibility of this control
     *
     * Returns:
     * {Boolean} the current visibility of this control
    */
    getVisible: function() {
        return this.visible;
    },

    /**
     * APIMethod: hide
     * Hide the loading panel control
    */
    hide: function() {
        this.setVisible(false);
    },

    /**
     * APIMethod: show
     * Show the loading panel control
    */
    show: function() {
        this.setVisible(true);
    },

    /**
     * APIMethod: toggle
     * Toggle the visibility of the loading panel control
    */
    toggle: function() {
        this.setVisible(!this.getVisible());
    },

    /**
     * Method: addLayer
     * Attach event handlers when new layer gets added to the map
     *
     * Parameters:
     * evt - {Event}
    */
    addLayer: function(evt) {
        if (evt.layer) {
            evt.layer.events.register('loadstart', this, this.increaseCounter);
            evt.layer.events.register('loadend', this, this.decreaseCounter);
        }
    },

    /**
     * Method: setMap
     * Set the map property for the control and all handlers.
     *
     * Parameters: 
     * map - {<OpenLayers.Map>} The control's map.
     */
    setMap: function(map) {
        OpenLayers.Control.prototype.setMap.apply(this, arguments);
        this.map.events.register('preaddlayer', this, this.addLayer);
        for (var i = 0; i < this.map.layers.length; i++) {
            var layer = this.map.layers[i];
            layer.events.register('loadstart', this, this.increaseCounter);
            layer.events.register('loadend', this, this.decreaseCounter);
        }
    },

    /**
     * Method: increaseCounter
     * Increase the counter and show control
    */
    increaseCounter: function() {
        this.counter++;
        if (this.counter > 0) { 
            if (!this.maximized && this.visible) {
                this.maximizeControl(); 
            }
        }
    },
    
    /**
     * Method: decreaseCounter
     * Decrease the counter and hide the control if finished
    */
    decreaseCounter: function() {
        if (this.counter > 0) {
            this.counter--;
        }
        if (this.counter == 0) {
            if (this.maximized && this.visible) {
                this.minimizeControl();
            }
        }
    },

    /**
     * Method: draw
     * Create and return the element to be splashed over the map.
     */
    draw: function () {
        OpenLayers.Control.prototype.draw.apply(this, arguments);
        return this.div;
    },
     
    /**
     * Method: minimizeControl
     * Set the display properties of the control to make it disappear.
     *
     * Parameters:
     * evt - {Event}
     */
    minimizeControl: function(evt) {
        this.div.style.display = "none"; 
        this.maximized = false;
    
        if (evt != null) {
            OpenLayers.Event.stop(evt);
        }
    },
    
    /**
     * Method: maximizeControl
     * Make the control visible.
     *
     * Parameters:
     * evt - {Event}
     */
    maximizeControl: function(evt) {
        this.div.style.display = "block";
        this.maximized = true;
    
        if (evt != null) {
            OpenLayers.Event.stop(evt);
        }
    },

    /** 
     * Method: destroy
     * Destroy control.
     */
    destroy: function() {
        if (this.map) {
            this.map.events.unregister('preaddlayer', this, this.addLayer);
            if (this.map.layers) {
                for (var i = 0; i < this.map.layers.length; i++) {
                    var layer = this.map.layers[i];
                    layer.events.unregister('loadstart', this, 
                        this.increaseCounter);
                    layer.events.unregister('loadend', this, 
                        this.decreaseCounter);
                }
            }
        }
        OpenLayers.Control.prototype.destroy.apply(this, arguments);
    },     

    CLASS_NAME: "OpenLayers.Control.LoadingPanel"

});
