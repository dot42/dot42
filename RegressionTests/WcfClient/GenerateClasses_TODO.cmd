@echo off
cls
:: Set environment variables
call "%VS100COMNTOOLS%\vsvars32.bat"
:: Run the XSD tool
xsd.exe /c /n:Dot42.TodoApi.Version_1_0 TodoApi.xsd

PAUSE
