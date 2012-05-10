/**
 * @copyright  2011 geOps
 * @license    https://github.com/geops/ole/blob/master/license.txt
 * @link       https://github.com/geops/ole
 */

/**
 * Class: OpenLayers.Editor.Control.Dialog
 * ...
 *
 * Inherits from:
 *  - <OpenLayers.Control>
 */
OpenLayers.Editor.Control.Dialog =  OpenLayers.Class(OpenLayers.Control, {

    dialogDiv: null,

    initialize: function (options) {

        OpenLayers.Control.prototype.initialize.apply(this, [options]);

    },

    show: function (options) {

        var element, cancelButton, saveButton, okButton;

        if (OpenLayers.Util.indexOf(this.map.viewPortDiv.childNodes, this.dialogDiv) > -1) {
            this.map.viewPortDiv.removeChild(this.dialogDiv);
        }

        if (!options) {
            options = {};
        }

        this.dialogDiv = document.createElement('div');
        OpenLayers.Element.addClass(this.dialogDiv, 'dialog');
        OpenLayers.Element.addClass(this.div, 'fadeMap');

        if (options.title) {
            element = document.createElement('h3');
            element.innerHTML = options.title;
            this.dialogDiv.appendChild(element);
        }

        if (typeof options.content === 'string') {
            element = document.createElement('div');
            element.innerHTML = options.content;
            this.dialogDiv.appendChild(element);
        } else if (OpenLayers.Util.isElement(options.content)) {
            this.dialogDiv.appendChild(options.content);
        }

        if (options.save) {
            cancelButton = document.createElement('input');
            cancelButton.name = 'cancel';
            cancelButton.id = 'cancelButton';
            cancelButton.value = OpenLayers.i18n('oleDialogCancelButton');
            cancelButton.type = 'button';
            this.dialogDiv.appendChild(cancelButton);
            saveButton = document.createElement('input');
            saveButton.name = 'save';
            saveButton.id = 'saveButton';
            saveButton.value = OpenLayers.i18n('oleDialogSaveButton');
            saveButton.type = 'button';
            this.dialogDiv.appendChild(saveButton);
            OpenLayers.Event.observe(cancelButton, 'click', this.hide.bind(this));
            OpenLayers.Event.observe(saveButton, 'click', this.hide.bind(this));
            OpenLayers.Event.observe(saveButton, 'click', options.save.bind(this));
            if (options.cancel) {
                OpenLayers.Event.observe(cancelButton, 'click', options.cancel.bind(this));
            }
        } else {
            okButton = document.createElement('input');
            okButton.name = 'save';
            okButton.id = 'okButton';
            okButton.value = OpenLayers.i18n('oleDialogOkButton');
            okButton.type = 'button';
            this.dialogDiv.appendChild(okButton);

            if (options.close) {
                OpenLayers.Event.observe(okButton, 'click', options.close);
            }

            OpenLayers.Event.observe(okButton, 'click', OpenLayers.Function.bind(this.hide, this));
        }

        this.map.viewPortDiv.appendChild(this.dialogDiv);

        OpenLayers.Event.observe(this.div, 'click', this.ignoreEvent);
        OpenLayers.Event.observe(this.div, 'mousedown', this.ignoreEvent);
        OpenLayers.Event.observe(this.div, 'dblclick', this.ignoreEvent);
        OpenLayers.Event.observe(this.dialogDiv, 'mousedown', this.ignoreEvent);
        OpenLayers.Event.observe(this.dialogDiv, 'dblclick', this.ignoreEvent);
    },

    hide: function () {
        this.map.viewPortDiv.removeChild(this.dialogDiv);
        OpenLayers.Element.removeClass(this.div, 'fadeMap');
    },

    ignoreEvent: function (event) {
        OpenLayers.Event.stop(event, true);
    },

    CLASS_NAME: 'OpenLayers.Editor.Control.Dialog'
});
