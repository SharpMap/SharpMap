/**
 * @copyright  2011 geOps
 * @license    https://github.com/geops/ole/blob/master/license.txt
 * @link       https://github.com/geops/ole
 */

/**
 * Class: OpenLayers.Editor.Control.MergeFeature
 * ...
 *
 * Inherits from:
 *  - <OpenLayers.Control.Button>
 */
OpenLayers.Editor.Control.SaveFeature = OpenLayers.Class(OpenLayers.Control.Button, {

    EVENT_TYPES: ["featuresaved"],

    layer: null,

    url: '',

    proxy: null,

    fields: [],

    textFields: {},

    title: OpenLayers.i18n('oleSaveFeature'),
    
    /**
     * Constructor: OpenLayers.Editor.Control.MergeFeature
     * Create a new control for merging features.
     *
     * Parameters:
     * layer - {<OpenLayers.Layer.Vector>}
     * options - {Object} An optional object whose properties will be used
     *     to extend the control.
     */
    initialize: function(layer, options) {

        this.layer = layer;

        OpenLayers.Control.Button.prototype.initialize.apply(this, [options]);

        this.trigger = this.openSaveDialog;

        this.title = OpenLayers.i18n('oleSaveFeature');
    },

    openSaveDialog: function() {
        if(this.layer.features.length < 0) {
            alert("Keine Objekte vorhanden.");
        } else {
            var fieldList = new Element("ul");
            //this.fields.each( function(field) {
            for(var i = 0; i < this.fields.length; i++) {
                var field = this.fields[i];
                //alert(typeof field);
                if(field && field.type != "hidden" && field.type != "geometry") {
                    var fieldListItem = new Element("li");
                    var fieldLabel = new Element('label', {'for':field.name}).
                        update(field.label+': ');
                    var fieldInput = new Element('input', {
                        'type': field.type,
                        'name': field.name,
                        'id':field.name
                    });
                    fieldListItem.appendChild(fieldLabel);
                    fieldListItem.appendChild(fieldInput);
                    fieldList.appendChild(fieldListItem);
                    fieldInput.observe('click', function(e){
                        e.currentTarget.focus();
                    });
                    this.textFields[field.name] = fieldInput;
                }
            }
            
            var content = new Element("div");

            content.appendChild(fieldList);
            content.appendChild(new Element("p"));

            this.map.editor.dialog.show(content, {
                title: 'Änderungen speichern',
                save: this.saveFeature.bind(this)
            });
        }
    },
    
    saveFeature: function() {
        
        var multiPolygon = this.map.editor.toMultiPolygon(this.layer.features);
        var geoJSON = new OpenLayers.Format.GeoJSON().write(multiPolygon);
        var params = this.map.editor.params;
        
        // write fields to params
        for(var i = 0; i < this.fields.length; i++) {
            var field = this.fields[i];

                if (field.type == 'hidden') {
                    params[field.name] = field.value;
                } else if (field.type == 'geometry') {
                    params[field.name] = geoJSON;
                } else if (field.type == 'text') {
                    params[field.name] = this.textFields[field.name].value;
                }
            
        }

        OpenLayers.Request.POST({
            url: this.url,
            params: params,
            callback: this.saveFeatureComplete,
            proxy: this.proxy,
            scope: this
        });
    },
    
    saveFeatureLoading: function() {
        this.map.editor.dialog.show('Änderungen werden gespeichert ...');
    },

    saveFeatureComplete: function(response) {
        var responseJSON = new OpenLayers.Format.JSON().read(response.responseText);
        if (!responseJSON) {
            this.events.triggerEvent("featuresaved", response);
//            this.map.editor.dialog.show(OpenLayers.i18n('oleNoJSON'), {type: 'error'});
        } else if (responseJSON.error === 'true') {
            this.map.editor.dialog.show(responseJSON.message, {type: 'error'});
        } else {
            if (responseJSON.params) {
                OpenLayers.Util.extend(this.params, responseJSON.params);
            }
//            this.map.editor.dialog.show('Änderungen gespeichert.');
            this.events.triggerEvent("featuresaved", responseJSON);
        }
    },

    CLASS_NAME: "OpenLayers.Editor.Control.SaveFeature"
});