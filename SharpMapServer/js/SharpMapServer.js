// Declaration of Namespaces and 'Enums'
if (typeof SharpMap == 'undefined') {
    SharpMap = {};
}

SharpMap.Server = function () {
    this.UpdateStatus = function () {
        $('#serverStatus').html("loading status....");
        $.ajax({
            url: 'admin/services?operation=status',
            dataType: 'json',
            success: function (data) {
                $("#serverStatus").html(data.Version);
            }
        });
    }

    this.UpdateWMSLayers = function () {
        $("#wmsLayerList").html("Loading layerlist...");
        $.ajax({
            url: 'admin/services?operation=getwmslayers',
            dataType: 'json',
            success: function (data) {
                $("#wmsLayerList").html("");
                var html = "";
                for (i = 0; i < data.layers.length; i++) {
                    html += i + ". <b>" + data.layers[i].Name + "</b> | <a href=\"Demo.aspx?layerName=" + encodeURIComponent(data.layers[i].Name) + "\" target=\"_blank\">Demo</a><br/>";
                }
                $("#wmsLayerList").html(html);
            }
        });
    }

    this.UpdateBasicSettings = function () {
        $.ajax({
            url: 'admin/services?operation=generalsettings',
            dataType: 'json',
            success: function (data) {
                $("#settsTitle").val(data.Title);
                $("#settsAbstract").val(data.Abstract);
                $("#settsAccessConstraints").val(data.AccessConstraints);
                $("#settsContactInformation").val(data.ContactInformation);
                $("#settsFees").val(data.Fees);
                $("#settsKeywords").val(data.Keywords);
                $("#settsOnlineResource").val(data.OnlineResource);
            }
        });
    }
}