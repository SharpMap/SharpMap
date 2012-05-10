/**
 * @copyright  2011 geOps
 * @license    https://github.com/geops/ole/blob/master/license.txt
 * @link       https://github.com/geops/ole
 */

/**
 * Class: OpenLayers.Editor.Control.LayerSettings
 * ...
 *
 * Inherits from:
 *  - <OpenLayers.Control>
 */
OpenLayers.Editor.Control.LayerSettings =  OpenLayers.Class(OpenLayers.Control, {

    currentLayer: null,

    layerSwitcher: null,

    initialize: function(editor, options) {

        OpenLayers.Control.prototype.initialize.apply(this, [options]);

        this.layerSwitcher = editor.map.getControlsByClass('OpenLayers.Control.LayerSwitcher')[0];

        if(this.layerSwitcher instanceof OpenLayers.Control.LayerSwitcher) {
            OpenLayers.Event.observe(this.layerSwitcher.maximizeDiv, 'click',
                OpenLayers.Function.bind(this.redraw, this));
        }

    },

    redraw: function() {

        var layerInput, layerLabel;
        
        this.layerSwitcher.dataLayersDiv.innerHTML = "";

        for (var i = 0, l = this.layerSwitcher.dataLayers.length; i < l; i++) {

            var dataLayer = this.layerSwitcher.dataLayers[i];

            layerInput = document.createElement('input');
            layerInput.type = 'checkbox';
            layerInput.id = 'list'+dataLayer.layer.name;
            layerInput.name = dataLayer.layer.name;
            if (dataLayer.layer.visibility) {
                layerInput.checked = 'checked';
                layerInput.defaultChecked = 'selected'; // IE7 hack
            }
            this.layerSwitcher.dataLayersDiv.appendChild(layerInput);
            layerLabel = document.createElement('span');
            layerLabel.innerHTML = dataLayer.layer.name;
            OpenLayers.Element.addClass(layerLabel, 'labelSpan');
            this.layerSwitcher.dataLayersDiv.appendChild(layerLabel);
            this.layerSwitcher.dataLayersDiv.appendChild(document.createElement('br'));

            OpenLayers.Event.observe(layerInput, 'click',
                OpenLayers.Function.bind(this.toggleLayerVisibility, this, dataLayer.layer.name));
            OpenLayers.Event.observe(layerLabel, 'click',
                OpenLayers.Function.bind(this.showLayerSettings, this, dataLayer.layer.name));
        }
    },

    showLayerSettings: function(layerName) {

        var content, opacityHeader, opacityTrack, opacityHandle, opacityInput,
            legendHeader, legendGraphic,
            importHeader, importInput, importLabel;

        this.currentLayer = this.map.getLayersByName(layerName)[0];

        var content = document.createElement('div');

        var opacityHeader = document.createElement('h4');
        opacityHeader.innerHTML = OpenLayers.i18n('oleLayerSettingsOpacityHeader');
        content.appendChild(opacityHeader);

        var opacity = (this.currentLayer.opacity) ? this.currentLayer.opacity : 1;

        opacityInput = document.createElement('input');
        opacityInput.type = 'text';
        opacityInput.size = '2';
        opacityInput.value = (opacity*100).toFixed(0);
        OpenLayers.Event.observe(opacityInput, 'change',
            OpenLayers.Function.bind(this.changeLayerOpacity, this, opacityInput));
        content.appendChild(opacityInput);

        // display import checkbox for vector layer
        if (this.currentLayer instanceof OpenLayers.Layer.Vector) {

            importHeader = document.createElement('h4');
            importHeader.innerHTML = OpenLayers.i18n('oleLayerSettingsImportHeader');
            importHeader.style.marginTop = '10px';
            content.appendChild(importHeader);

            importInput = document.createElement('input');
            importInput.type = 'checkbox';
            importInput.name = 'import'+this.currentLayer.name;
            content.appendChild(importInput);

            importLabel = document.createElement('label');
            importLabel.htmlFor = 'import'+this.currentLayer.name;
            importLabel.innerHTML = OpenLayers.i18n('oleLayerSettingsImportLabel');
            content.appendChild(importLabel);
            content.appendChild(document.createElement('p'));

            for(var i = 0, li = this.map.editor.sourceLayers.length; i < li; i++) {
                if (this.currentLayer.id == this.map.editor.sourceLayers[i].id) {
                    importInput.writeAttribute('checked','checked');
                    importInput.defaultChecked = 'selected'; // IE7 hack
                    break;
                }
            }
            OpenLayers.Event.observe(importInput, 'click',
                OpenLayers.Function.bind(this.toggleExportFeature, this));
        }

        var legendGraphics = this.getLegendGraphics(this.currentLayer);

        if (legendGraphics.length > 0) {

            legendHeader = document.createElement('h4');
            legendHeader.innerHTML = OpenLayers.i18n('oleLayerSettingsLegendHeader');
            legendHeader.style.marginTop = '10px';
            content.appendChild(legendHeader);

            for(var i = 0; i < legendGraphics.length; i++) {
                legendGraphic = document.createElement('img');
                legendGraphic.src = legendGraphics[i];
                content.appendChild(legendGraphic);
            }
        }

        this.map.editor.dialog.show({
            content: content,
            title: layerName
        });
    },

    toggleExportFeature: function() {
        var add = true;
        for(var i = 0, li = this.map.editor.sourceLayers.length; i < li; i++) {
            if (this.currentLayer.id == this.map.editor.sourceLayers[i].id) {
                this.map.editor.sourceLayers.splice(i, 1);
                add = false;
                break;
            }
        }
        if (add) {
            this.map.editor.sourceLayers.push(this.currentLayer);
        }
    },

    toggleLayerVisibility: function(layerName) {
        var layer = this.map.getLayersByName(layerName)[0];
        if(layer.visibility) {
            layer.setVisibility(false);
        } else {
            layer.setVisibility(true);
        }
        this.redraw();
    },

    changeLayerOpacity: function (opacityInput) {
        this.currentLayer.setOpacity(opacityInput.value/100);
    },

    getLegendGraphics: function(layer) {

        var legendGraphics = [];

        if(layer.legendGraphics) {

            legendGraphics = layer.legendGraphics;

        } else if (layer instanceof OpenLayers.Layer.WMS) {

            var urlLayers = layer.params.LAYERS.split(',');

            for(var j = 0; j < urlLayers.length; j++) {
                var singlelayer = urlLayers[j];
                var url = layer.url;
                url += ( url.indexOf('?') === -1 ) ? '?' : '';
                url += '&SERVICE=WMS';
                url += '&VERSION=1.1.1';
                url += '&REQUEST=GetLegendGraphic';
                url += '&FORMAT=image/png';
                url += '&LAYER=' + singlelayer;
                legendGraphics.push(url);
            }
        }
        return legendGraphics;
    }, 

    CLASS_NAME: 'OpenLayers.Editor.Control.LayerSettings'
});
