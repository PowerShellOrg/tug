@SETLOCAL
@SET THIS=%0
@SET THIS_DIR=%~dp0

@ECHO THIS=%THIS%
@ECHO THIS_DIR=%THIS_DIR%

dotnet test %THIS_DIR% %* -- /proxy_url="http://localhost:8888"

@ENDLOCAL
