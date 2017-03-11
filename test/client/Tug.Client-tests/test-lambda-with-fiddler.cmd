@SETLOCAL
@SET THIS=%0
@SET THIS_DIR=%~dp0

@ECHO THIS=%THIS%
@ECHO THIS_DIR=%THIS_DIR%
@ECHO.

@REM Assuming SERVER2 is a Win2012R2 server with WMF 5.0 so we 
@REM don't touch the default setting of /adjust_for_wmf_50=true

@SET SERVER_URL=http://DSC-SERVER2.tugnet:8080/PSDSCPullServer.svc/
@SET PROXY_URL="http://localhost:8888"

@IF NOT "%TUG_LAMBDA_SERVER%"=="" @GOTO :SERVER_RESOLVED
@ECHO **************************************
@ECHO Lambda Server endpoint is UNDEFINED!!!
@ECHO Define the EnvVar TUG_LAMBDA_SERVER
@ECHO **************************************
@GOTO :eof

:SERVER_RESOLVED

@SET SERVER_URL=https://%TUG_LAMBDA_SERVER%/
@SET WMF5_ADJUST=/adjust_for_wmf_50=false

dotnet test %THIS_DIR% %* -- /server_url=%SERVER_URL% -- /proxy_url=%PROXY_URL%

@ENDLOCAL
