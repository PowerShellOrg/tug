@SETLOCAL
@SET THIS=%0
@SET THIS_DIR=%~dp0

@ECHO THIS=%THIS%
@ECHO THIS_DIR=%THIS_DIR%

@SET PROJ_ROOT=%THIS_DIR%..\..\src\Tug.Server
@SET PROJ_FILE=%PROJ_ROOT%\Tug.Server.csproj
@SET DOTNET_MONIK=-f net452

@MKDIR %THIS_DIR%\tug-as-winsvc
@XCOPY %PROJ_ROOT%\bin\Debug\net452 %THIS_DIR%\tug-as-winsvc /e/y


sc query TugService-TEST 2> nul
@IF "%ERRORLEVEL%"=="1060" GOTO sc_create
sc stop TugService-TEST
sc delete TugService-TEST

:sc_create 
sc create TugService-TEST binPath= "\"%THIS_DIR%\tug-as-winsvc\Tug.Server.exe\" --service true --contentRoot \"%THIS_DIR%\tug-as-winsvc\""
sc start TugService-TEST

@ENDLOCAL
