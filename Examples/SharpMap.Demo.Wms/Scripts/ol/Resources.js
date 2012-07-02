OpenLayers.Resources = {
    Styles: {
        Layers: {
            selection: new OpenLayers.StyleMap({
                'default': new OpenLayers.Style(null, {
                    rules: [
                        new OpenLayers.Rule({
                            symbolizer: {
                                Point: {
                                    pointRadius: 5,
                                    graphicName: 'circle',
                                    fillColor: 'white',
                                    fillOpacity: 0.25,
                                    strokeWidth: 2,
                                    strokeOpacity: 1,
                                    strokeColor: '#ffff00'
                                },
                                Line: {
                                    strokeWidth: 2,
                                    strokeOpacity: 1,
                                    strokeColor: '#ffff00'
                                },
                                Polygon: {
                                    strokeWidth: 2,
                                    strokeOpacity: 1,
                                    fillColor: '#9999aa',
                                    strokeColor: '#ffff00'
                                }
                            }
                        })
                    ]
                }),
                temporary: new OpenLayers.Style(null, {
                    rules: [
                        new OpenLayers.Rule({
                            symbolizer: {
                                Point: {
                                    pointRadius: 5,
                                    graphicName: 'circle',
                                    fillColor: 'white',
                                    fillOpacity: 0.25,
                                    strokeWidth: 3,
                                    strokeOpacity: 1,
                                    strokeColor: '#00ffff'
                                },
                                Line: {
                                    strokeWidth: 3,
                                    strokeOpacity: 1,
                                    strokeColor: '#00ffff'
                                },
                                Polygon: {
                                    strokeWidth: 3,
                                    strokeOpacity: 1,
                                    fillColor: '#9999aa',
                                    strokeColor: '#00ffff'
                                }
                            }
                        })
                    ]
                })
            }),

            editable: new OpenLayers.StyleMap({
                'default': new OpenLayers.Style(null, {
                    rules: [
                        new OpenLayers.Rule({
                            symbolizer: {
                                Point: {
                                    pointRadius: 5,
                                    graphicName: 'square',
                                    fillColor: 'white',
                                    fillOpacity: 0.5,
                                    strokeWidth: 1,
                                    strokeOpacity: 2,
                                    strokeColor: '#ff0000'
                                },
                                Line: {
                                    strokeWidth: 3,
                                    strokeOpacity: 0.75,
                                    strokeColor: '#339900'
                                },
                                Polygon: {
                                    fillColor: '#909090',
                                    fillOpacity: 0.5,
                                    strokeWidth: 1,
                                    strokeOpacity: 1,
                                    strokeColor: '#000000'
                                }
                            }
                        })
                    ]
                }),
                select: new OpenLayers.Style(null, {
                    rules: [
                        new OpenLayers.Rule({
                            symbolizer: {
                                Point: {
                                    pointRadius: 5,
                                    graphicName: 'square',
                                    fillColor: 'white',
                                    fillOpacity: 0.25,
                                    strokeWidth: 2,
                                    strokeOpacity: 1,
                                    strokeColor: '#0000ff'
                                },
                                Line: {
                                    strokeWidth: 3,
                                    strokeOpacity: 1,
                                    strokeColor: '#0000ff'
                                },
                                Polygon: {
                                    strokeWidth: 2,
                                    strokeOpacity: 1,
                                    fillColor: '#0000ff',
                                    strokeColor: '#0000ff'
                                }
                            }
                        })
                    ]
                }),
                temporary: new OpenLayers.Style(null, {
                    rules: [
                        new OpenLayers.Rule({
                            symbolizer: {
                                Point: {
                                    graphicName: 'square',
                                    pointRadius: 5,
                                    fillColor: 'white',
                                    fillOpacity: 0.25,
                                    strokeWidth: 2,
                                    strokeColor: '#0000ff'
                                },
                                Line: {
                                    strokeWidth: 3,
                                    strokeOpacity: 1,
                                    strokeColor: '#0000ff'
                                },
                                Polygon: {
                                    strokeWidth: 2,
                                    strokeOpacity: 1,
                                    strokeColor: '#0000ff',
                                    fillColor: '#0000ff'
                                }
                            }
                        })
                    ]
                })
            })
        }
    }
}