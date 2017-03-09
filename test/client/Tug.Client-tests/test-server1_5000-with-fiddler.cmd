@SETLOCAL
@SET THIS=%0
@SET THIS_DIR=%~dp0

@ECHO THIS=%THIS%
@ECHO THIS_DIR=%THIS_DIR%

dotnet test %THIS_DIR% %* -- /server_url=http://DSC-SERVER1.tugnet:5000/ -- /proxy_url="http://localhost:8888" -- /adjust_for_wmf_50=false

@ENDLOCAL
