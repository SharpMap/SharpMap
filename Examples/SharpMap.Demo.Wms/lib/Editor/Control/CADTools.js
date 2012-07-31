/**
 * @copyright  2011 geOps
 * @license    https://github.com/geops/ole/blob/master/license.txt
 * @link       https://github.com/geops/ole
 */

/**
 * Class: OpenLayers.Editor.Control.CADTools
 * ...
 *
 * Inherits from:
 *  - <OpenLayers.Control.Button>
 */
OpenLayers.Editor.Control.CADTools = OpenLayers.Class(OpenLayers.Control.Button, {

    title: OpenLayers.i18n('oleCADTools'),

    layer: null,

    parallelDrawingButton: null,

    guidedDrawingButton: null,

    /**
     * @var {Number} Snapping tolerance in pixels
     */
    tolerance: 10,

    /**
     * @var {OpenLayers.Editor.Control.FixedAngleDrawing}
     */
    fixedAngleDrawingControl: null,
    /**
     * @var {OpenLayers.Editor.Control.ParallelDrawing}
     */
    parallelDrawingControl: null,

    /**
     * @var {OpenLayers.Control.Snapping} Snapping for all CAD tools
     */
    snappingControl: null,

    initialize: function(layer, options) {

        this.layer = layer;

        this.fixedAngleDrawingControl = new OpenLayers.Editor.Control.FixedAngleDrawing(layer);
        this.parallelDrawingControl = new OpenLayers.Editor.Control.ParallelDrawing(layer);

        OpenLayers.Control.Button.prototype.initialize.apply(this, [options]);
        
        this.trigger = OpenLayers.Function.bind(this.openCADToolsDialog, this);

        this.events.register("deactivate", this, this.onDeactivate);

        this.title = OpenLayers.i18n('oleCADTools');

    },

    activate: function() {
        var activated = OpenLayers.Control.Button.prototype.activate.call(this);
        if(activated) {
            this.snappingControl.activate();
        }
        return activated;
    },

    deactivate: function() {
        var deactivated = OpenLayers.Control.Button.prototype.deactivate.call(this);
        if(deactivated) {
            this.snappingControl.deactivate();
        }
        return deactivated;
    },

    setMap: function(map) {

        OpenLayers.Control.Button.prototype.setMap.call(this, map);

        this.map.addControl(this.fixedAngleDrawingControl);
        this.map.addControl(this.parallelDrawingControl);

        this.snappingControl = new OpenLayers.Control.Snapping({
            layer: this.layer,
            targets: [{
                layer: this.map.getLayersByClass('OpenLayers.Editor.Layer.Snapping')[0],
                tolerance: this.tolerance
            }]
        });
    },

    /**
     * Handles showing and hiding of the CAD tools dialog
     */
    openCADToolsDialog: function() {

        if (this.active) {
            this.deactivate();
            this.map.editor.dialog.hide();
        } else {

            this.activate();

            var content, element;

            content = document.createElement('div');

            var toolbar = document.createElement('div');
            toolbar.className = 'olEditorControlEditorPanel olEditorCADToolsToolbar';
            toolbar.style.top = '10px';
            toolbar.style.right = '10px';

            this.parallelDrawingButton = document.createElement('div');
            this.parallelDrawingButton.title = OpenLayers.i18n('oleCADToolsDialogParallelDrawing');
            if (this.parallelDrawingControl.active) {
                this.parallelDrawingButton.className = 'olEditorParallelDrawingActive';
            } else {
                this.parallelDrawingButton.className = 'olEditorParallelDrawingInactive';
            }
            OpenLayers.Event.observe(this.parallelDrawingButton, 'click', OpenLayers.Function.bind(function() {
                if (this.parallelDrawingControl.active) {
                    this.parallelDrawingControl.deactivate();
                    this.parallelDrawingButton.className = 'olEditorParallelDrawingInactive';
                } else {
                    this.parallelDrawingControl.activate();
                    this.parallelDrawingButton.className = 'olEditorParallelDrawingActive';
                }
            }, this, this.parallelDrawingButton));
            toolbar.appendChild(this.parallelDrawingButton);

            this.guidedDrawingButton = document.createElement('div');
            this.guidedDrawingButton.title = OpenLayers.i18n('oleCADToolsDialogGuidedDrawing');
            if (this.fixedAngleDrawingControl.active) {
                this.guidedDrawingButton.className = 'olEditorGuidedDrawingActive';
            } else {
                this.guidedDrawingButton.className = 'olEditorGuidedDrawingInactive';
            }
            OpenLayers.Event.observe(this.guidedDrawingButton, 'click', OpenLayers.Function.bind(function() {
                if (this.fixedAngleDrawingControl.active) {
                    this.fixedAngleDrawingControl.deactivate();
                    this.guidedDrawingButton.className = 'olEditorGuidedDrawingInactive';
                } else {
                    this.fixedAngleDrawingControl.activate();
                    this.guidedDrawingButton.className = 'olEditorGuidedDrawingActive';
                }
            }, this, this.guidedDrawingButton));
            toolbar.appendChild(this.guidedDrawingButton);

            content.appendChild(toolbar);

            var settings = document.createElement('div');
            var showGuideLine = document.createElement('p');
            var showLayer = document.createElement('input');
            showLayer.type = 'checkbox';
            showLayer.id = 'oleCADToolsDialogShowLayer';
            showLayer.name = 'guidedDrawing';
            showLayer.value = 'true';
            showLayer.checked = true;
            // The following IE is required due to a bug in IE (below version 8) to tick the checkbox prior inserting it into the DOM
            showLayer.defaultChecked = true;
            this.setShowGuides(showLayer.checked);
            showGuideLine.appendChild(showLayer);

            OpenLayers.Event.observe(showGuideLine, 'click', OpenLayers.Function.bind(function(event) {
                // Prevent propagation of event to drawing controls
                OpenLayers.Event.stop(event, true);

                this.setShowGuides(showLayer.checked);
            }, this));

            element = document.createElement('label');
            element.htmlFor = 'oleCADToolsDialogShowLayer';
            element.appendChild(document.createTextNode(OpenLayers.i18n('oleCADToolsDialogShowLayer')));
            showGuideLine.appendChild(element);
            settings.appendChild(showGuideLine);

            var toleranceSetting = document.createElement('p');
            element = document.createElement('input');
            element.type = 'text';
            element.id = 'oleCADToolsDialogTolerance';
            element.size = 4;
            element.value = this.tolerance;
            OpenLayers.Event.observe(element, 'change', OpenLayers.Function.bind(function(event){
                this.setTolerance(event.target.value);
            }, this));
            toleranceSetting.appendChild(element);

            element = document.createElement('label');
            element.htmlFor = 'oleCADToolsDialogTolerance';
            element.appendChild(document.createTextNode(OpenLayers.i18n('oleCADToolsDialogTolerance')));
            toleranceSetting.appendChild(element);
            settings.appendChild(toleranceSetting);

            content.appendChild(settings);
            this.map.editor.dialog.show({
                content: content,
                toolbox: true
            });
        }
    },

    /**
     * @param {Number} tolerance Snapping tolerance in pixels
     */
    setTolerance: function(tolerance){
        this.tolerance = tolerance;

        this.snappingControl.setTargets([{
            layer: this.map.getLayersByClass('OpenLayers.Editor.Layer.Snapping')[0],
            tolerance: this.tolerance
        }]);
    },

    /**
     * Shows or hides guides
     * @param {Boolean} guidesVisible Set to TRUE to show guides
     */
    setShowGuides: function(guidesVisible){
        var snappingLayer = this.map.getLayersByClass('OpenLayers.Editor.Layer.Snapping')[0];
        snappingLayer.setVisibility(guidesVisible);
    },

    CLASS_NAME: "OpenLayers.Editor.Control.CADTools"
});