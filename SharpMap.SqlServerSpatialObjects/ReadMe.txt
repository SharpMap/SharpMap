In order to use this project you need to add a reference to the
"Microsoft SQL Server System CLR Types". 

If you have SqlServer2008 (Express) installed on your machine you'll probably find in somewhere 
in the installation path of it. On my machine the path is:
C:\Program Files\Microsoft SQL Server\100\SDK\Assemblies\Microsoft.SqlServer.Types.dll

If you do not have SqlServer 2008 on your machine, all you need is download some files
from the "Microsoft SQL Server 2008 Feature Pack, October 2008":
http://www.microsoft.com/downloads/en/details.aspx?FamilyId=228DE03F-3B5A-428A-923F-58A033D316E1&displaylang=en

Look for "Microsoft SQL Server System CLR Types"

Enjoy
FObermaier

=================================================================================================================================

Update Nov 2018: Added support for SqlGeography
    SqlServer2008Ex.cs
    SqlGeographyConverter.cs (new class)
    SpatialOperationsEx.cs (basic support for Geography)
    SpatialRelationsEx.cs  (basic support for Geography)

Several more recents approaches were also considered, but would have caused breaking changes:
1) Drop reference to Microsoft.SqlServer.Types and replace with NuGet Microsoft.SqlServerTypes v14.0.1016.290
   This package support x86 and x64, and also includes SqlServerSpatialxxx.dll (no need to install any other Sql Server binaries).
   Also requires binding redirects, and native SQL assemblies must be explicitly loaded at runtime by client (breaking change)
2) Use NuGet NetTopologySuite.Io.SqlServerBytes (released Oct 2018) instead of SqlGeometryConverter.cs
   This package provides comprehenisve conversion between NTS geometry and SQL Server geometery/geography types, but targets NetStandard 2.0.
   Also, SqlGeometryConverter.cs implements custom exception handling.

Regards

Tim
