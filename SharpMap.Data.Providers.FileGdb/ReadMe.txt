To compile and use this provider you need to perform the following steps:

1. Get your private copy of Esri's File Geodatabase API SDK v1.2
   http://resources.arcgis.com/content/geodatabases/10.0/file-gdb-api

2. Unpack the contents of the zip-File to a location of your choice.

3. (Update) Reference to the Esri.FileGDBAPI assembly.
   It is located in the .\bin or .\bin64 folder.

4. Adjust app.config file
%<-----
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <add key="FileGdbNativeDirectory" value="__Location_where_you_unpacked_the_zip-file__\[bin|bin64]"/>
  </appSettings>
</configuration>
----->%

5. Set the target platform according to your needs.

6. For productive use, exclude 
   - reference to NUnit 
   - FileGdbProviderTest.cs

Enjoy
FObermaier
