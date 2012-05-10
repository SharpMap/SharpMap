(function() {
    
    var scripts = document.getElementsByTagName("script");
    var src = scripts[scripts.length - 1].src;
    var path = src.substring(0, src.lastIndexOf("/") + 1);

    var files = [
        'Editor.js',
        'Editor/Control/CleanFeature.js',
        'Editor/Control/DragFeature.js',
        'Editor/Control/DeleteFeature.js',
        'Editor/Control/Dialog.js',
        'Editor/Control/DrawHole.js', 
        'Editor/Control/DrawPolygon.js',
        'Editor/Control/DrawPath.js',
        'Editor/Control/DrawPoint.js',
        'Editor/Control/EditorPanel.js',
        'Editor/Control/ImportFeature.js',
        'Editor/Control/LayerSettings.js',
        'Editor/Control/MergeFeature.js',
        'Editor/Control/SaveFeature.js',
        'Editor/Control/SnappingSettings.js',
        'Editor/Control/SplitFeature.js',
        'Editor/Control/UndoRedo.js'
    ];
    
    // Load translations if HTML page defines a language
    var language = document.documentElement.getAttribute('lang');
    if(language){
        files.unshift('Editor/Lang/'+language+'.js');
        if(OpenLayers.Lang[language]===undefined){
            OpenLayers.Lang[language] = {};
        }
    }
    
    var tags = new Array(files.length);

    var el = document.getElementsByTagName("head").length ? 
	document.getElementsByTagName("head")[0] : 
	document.body;

    for(var i=0, len=files.length; i<len; i++) {
	tags[i] = "<script src='" + path + files[i] + "'></script>"; 
    }
    document.write(tags.join(""));
	
})();
