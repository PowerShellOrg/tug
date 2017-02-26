@SETLOCAL
@SET THIS=%0
@SET THIS_DIR=%~dp0

@ECHO THIS=%THIS%
@ECHO THIS_DIR=%THIS_DIR%

@SET PUBLISH_DIR=%THIS_DIR%bin\posh-modules\Tug.Server-ps5
@IF EXIST "%PUBLISH_DIR%" RD %PUBLISH_DIR% /s /q
@ECHO Publishing to [%PUBLISH_DIR%]

@REM We need to publish each of the Tug.Server and the PS5 Handler
@REM projects to produce the necessary artifacts for deployment
dotnet publish %THIS_DIR%..\..\Tug.Server                         -o %PUBLISH_DIR%\bin     -f net452 -r win7-x64
dotnet publish %THIS_DIR%..\..\Tug.Server.Providers.Ps5DscHandler -o %PUBLISH_DIR%\bin\ext -f net452 -r win7-x64
dotnet publish %THIS_DIR%                                         -o %PUBLISH_DIR%\bin     -f net452 -r win7-x64

@REM We iterate through each file that's in EXT and if it exists
@REM exactly the same in the parent bin folder, we can remove it
@FOR /F %%f IN ('DIR %PUBLISH_DIR%\bin\ext\* /a-d/b') DO @(
    @IF EXIST %PUBLISH_DIR%\bin\%%f (
        @FC /B %PUBLISH_DIR%\bin\ext\%%f %PUBLISH_DIR%\bin\%%f > nul
        @IF "%ERRORLEVEL%"=="0" @DEL %PUBLISH_DIR%\bin\ext\%%f
    )
)

@REM Copy over the supporting config and
@REM doc files exactly as they are
xcopy %THIS_DIR%posh-res %PUBLISH_DIR%\ /E

@REM Copy over the Basic Tug PS cmdlets over from the test folder
copy %THIS_DIR%..\..\..\test\run-net452-ps5\BasicTugCmdlets.ps1 %PUBLISH_DIR%\samples

@ENDLOCAL
