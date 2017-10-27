@SETLOCAL
@SET THIS=%0
@SET THIS_DIR=%~dp0

@FOR %%* IN (.) DO @SET THIS_PROJ=%%~nx*

@ECHO THIS=%THIS%
@ECHO THIS_DIR=%THIS_DIR%
@ECHO THIS_PROJ=%THIS_PROJ%
@ECHO.

@SET TSTCFG_adjust_for_wmf_50=false
@SET TSTCFG_server_url=http://localhost:5000/

@ECHO *** REMEMBER - WE'RE SKIPPING THE BUILD ***
dotnet test %THIS_DIR%%THIS_PROJ%.csproj --no-build %*

@ENDLOCAL
