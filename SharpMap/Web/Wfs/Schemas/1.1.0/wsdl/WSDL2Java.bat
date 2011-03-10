@echo off

set AXIS_LIB=C:\axis-1_3\lib

set CP=.
set CP=%CP%;%AXIS_LIB%\axis.jar
set CP=%CP%;%AXIS_LIB%\commons-logging-1.0.4.jar
set CP=%CP%;%AXIS_LIB%\commons-discovery-0.2.jar
set CP=%CP%;%AXIS_LIB%\log4j-1.2.8.jar.jar
set CP=%CP%;%AXIS_LIB%\jaxrpc.jar
set CP=%CP%;%AXIS_LIB%\wsdl4j-1.5.1.jar
set CP=%CP%;%AXIS_LIB%\saaj.jar

@echo on

java -classpath %CP% org.apache.axis.wsdl.WSDL2Java --server-side --timeout -1 example-SOAP-endpoints.wsdl
