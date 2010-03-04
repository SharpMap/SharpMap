This backup must be restored to a suitable PostgreSQL Database Server with PostGis extension installed:
The backup was made with the following versions:
- PostgreSQL 8.3.4
- Postgis 1.3.3

Steps to restore this database you need to run the following commands from console:
	psql -U postgres -c "CREATE DATABASE postgis_sample OWNER postgres TEMPLATE template_postgis ENCODING 'UTF8';"
	psql -d postgis_sample -U postgres -f <path to postgisdemodb.backup>

It contains the data from the Countries/Rivers/Cities shapefiles
