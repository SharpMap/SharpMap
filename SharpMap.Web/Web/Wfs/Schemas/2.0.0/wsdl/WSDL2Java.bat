@echo off

set CP=.
set CP=%CP%;%AXIS_LIB%\axis.jar
set CP=%CP%;%AXIS_LIB%\commons-logging.jar
set CP=%CP%;%AXIS_LIB%\commons-discovery.jar
set CP=%CP%;%AXIS_LIB%\log4j-1.2.8.jar.jar
set CP=%CP%;%AXIS_LIB%\jaxrpc.jar
set CP=%CP%;%AXIS_LIB%\wsdl4j.jar
set CP=%CP%;%AXIS_LIB%\saaj.jar

@echo on

java -classpath %CP% org.apache.axis.wsdl.WSDL2Java --server-side example-SOAP-endpoints.wsdl
