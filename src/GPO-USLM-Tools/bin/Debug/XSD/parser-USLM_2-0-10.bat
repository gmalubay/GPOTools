@echo off
cls

title uslm-2.0.10_parser
for %%A in (*.xml) do (
	echo %%~nA
	java -jar msv-core-2022.8-SNAPSHOT-jar-with-dependencies.jar USLM\uslm-2.0.10.xsd -verbose %%A >%%~dpnA.log  2>&1)
exit