OpenGIS(r) WFS schema - ReadMe.txt
==================================

OpenGIS(r) Web Feature Service (WFS) Implementation Standard
-------------------------------------------------------------------

The OpenGIS Web Feature Service Interface Standard (WFS) defines an
interface for specifying requests for retrieving geographic features
across the Web using platform-independent calls. The WFS standard
defines interfaces and operations for data access and manipulation
on a set of geographic features.

The WFS 2.0 standard is defined in OGC document 09-025r1 and 
ISO/DIS 19142.

More information may be found at
 http://www.opengeospatial.org/standards/wfs

The most current schema are available at http://schemas.opengis.net/ .

The root (all-components) XML Schema Document, which includes
directly and indirectly all the XML Schema Documents, defined by
WFS 2.0 is wfs.xsd .

* Latest version is: http://schemas.opengis.net/wfs/2.0/wfs.xsd *

-----------------------------------------------------------------------

2010-11-02 Panagiotis (Peter) A. Vretanos
   * v2.0: Added 2.0.0 from 09-025r1

2010-02-03  Kevin Stegemoller

	* v1.1.0: updated xsd:schema:@version attribute to 1.1.2 (06-135r7 s#13.4)
	* v1.0.0: updated xsd:schema:@version to 1.0.0 2010-02-02 (06-135r7 s#13.4)
	* v1.1.0, 1.0.0:
    + updated xsd:schema:@version attribute (06-135r7 s#13.4)
    + update relative schema imports to absolute URLs (06-135r7 s#15)
    + update/verify copyright (06-135r7 s#3.2)
    + add archives (.zip) files of previous versions
    + create/update ReadMe.txt (06-135r7 s#17)

2009-05-08  Clemens Portele

  * v1.1.0: The cardinality of the InsertResults element is 1 which means that
    the element must always be present in a transaction response ...  even if
    that transaction contains no insert actions.  The cardinality should be
    zero.  Every instance that validates against the buggy schema document will
    also validate against the fixed schema document. See wfs-1_1_0-1.zip.

2005-11-22  Arliss Whiteside

  * v1.1.0, v1.0.0: The sets of XML Schema Documents for WFS versions have been
    edited to reflect the corrigenda to documents OGC 02-058 (WFS 1.0.0) and
    OGC 04-09 (WFS 1.1.0) that are based on the change requests: 
     OGC 05-068r1 "Store xlinks.xsd file at a fixed location"
     OGC 05-081r2 "Change to use relative paths"

 Note: check each OGC numbered document for detailed changes.

-----------------------------------------------------------------------

Policies, Procedures, Terms, and Conditions of OGC(r) are available
  http://www.opengeospatial.org/ogc/legal/ .

Copyright (c) 2010 Open Geospatial Consortium, Inc. All Rights Reserved.

-----------------------------------------------------------------------

