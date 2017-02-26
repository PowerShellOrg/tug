@SETLOCAL
@SET THIS=%0
@SET THIS_DIR=%~dp0

@ECHO THIS=%THIS%
@ECHO THIS_DIR=%THIS_DIR%

@REM Assuming SERVER2 is a Win2012R2 server with WMF 5.0 so we 
@REM don't touch the default setting of /adjust_for_wmf_50=true

dotnet test %THIS_DIR% %* -- /server_url=http://DSC-SERVER2.tugnet:8080/PSDSCPullServer.svc/ -- /proxy_url="http://localhost:8888"

@ENDLOCAL
