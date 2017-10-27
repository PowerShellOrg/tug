@SETLOCAL
@SET THIS=%0
@SET THIS_DIR=%~dp0

@FOR %%* IN (.) DO @SET THIS_PROJ=%%~nx*

@ECHO THIS=%THIS%
@ECHO THIS_DIR=%THIS_DIR%
@ECHO THIS_PROJ=%THIS_PROJ%
@ECHO.

@REM Assuming SERVER2 is a Win2012R2 server with WMF 5.0 so we 
@REM don't touch the default setting of /adjust_for_wmf_50=true

@SET TSTCFG_server_url=http://DSC-SERVER2.tugnet:8080/PSDSCPullServer.svc/
@SET TSTCFG_proxy_url=http://localhost:8888

@ECHO *** REMEMBER - WE'RE SKIPPING THE BUILD ***
dotnet test %THIS_DIR%%THIS_PROJ%.csproj --no-build %*

@ENDLOCAL
