The intention here is that individual implementations will declare something
like ?-endpoints.wsdl, and will provide restricted versions of ?-bindings.wsdl,
depending on the interfaces that the implementation supports.

The file dependencies.jpeg shows the import/include dependency graph.
Target namespaces are declared as follows:

Namespace				Defining File
http://www.opengis.net/gml		topology.xsd
http://www.opengis.net/ogc		expr.xsd
http://www.opengis.net/gml		geometryBasic0d1d.xsd
http://www.opengis.net/gml		geometryBasic2d.xsd
http://www.opengis.net/ows		ows19115subset.xsd
http://www.w3.org/1999/xlink		xlinks.xsd
http://www.opengis.net/gml		temporal.xsd
http://www.w3.org/2001/SMIL20/Language	smil20-language.xsd
http://www.opengis.net/gml		coordinateSystems.xsd
http://www.opengis.net/ows		owsServiceProvider.xsd
http://www.opengis.net/ows		owsExceptionReport.xsd
http://www.opengis.net/wfs/soap		wfs-soap-bindings.wsdl
http://www.opengis.net/gml		defaultStyle.xsd
http://www.w3.org/XML/1998/namespace	xml-mod.xsd
http://www.opengis.net/gml		valueObjects.xsd
http://www.opengis.net/wfs/requests/kvp	wfs-kvp-interfaces.wsdl
http://www.opengis.net/gml		datums.xsd
http://www.opengis.net/gml		observation.xsd
http://www.opengis.net/wfs/http/kvp	wfs-kvp-bindings.wsdl
http://www.w3.org/2001/SMIL20/		smil20.xsd
http://www.opengis.net/gml		temporalTopology.xsd
http://www.w3.org/XML/1998/namespace	xml.xsd
http://www.opengis.net/gml		geometryPrimitives.xsd
http://www.opengis.net/ows		owsServiceIdentification.xsd
http://www.opengis.net/wfs/http		wfs-http-bindings.wsdl
http://www.opengis.net/gml		feature.xsd
http://www.opengis.net/wfs-kvp		WFS-kvp.xsd
http://www.opengis.net/gml		basicTypes.xsd
http://www.opengis.net/gml		gml3.xsd
http://www.opengis.net/wfs/requests	wfs-xml-interfaces.wsdl
http://www.opengis.net/gml		dataQuality.xsd
http://www.opengis.net/gml		direction.xsd
http://www.opengis.net/gml		coverage.xsd
http://www.opengis.net/ows		owsOperationsMetadata.xsd
http://www.opengis.net/gml		measures.xsd
http://www.opengis.net/wfs		wfs.xsd
http://www.w3.org/2001/XMLSchema	XMLSchema.xsd
http://www.opengis.net/wfs/responses	wfs-responses.wsdl
http://www.opengis.net/gml		dictionary.xsd
http://www.myservice.com/wfs		example-endpoints.wsdl
http://www.opengis.net/ows		owsGetCapabilities.xsd
http://www.opengis.net/ogc		filter.xsd
http://www.opengis.net/gml		referenceSystems.xsd
http://www.opengis.net/gml		temporalReferenceSystems.xsd
http://www.opengis.net/gml		coordinateReferenceSystems.xsd
http://www.opengis.net/gml		geometryComplexes.xsd
http://www.opengis.net/gml		units.xsd
http://www.opengis.net/gml		dynamicFeature.xsd
http://www.opengis.net/gml		gmlBase.xsd
http://www.opengis.net/gml		geometryAggregates.xsd
http://www.opengis.net/gml		grids.xsd
http://www.opengis.net/gml		coordinateOperations.xsd

The file WSDL2Java.bat generates an Axis 1.1 service.
(It should work with Axis 1.2 also.)

