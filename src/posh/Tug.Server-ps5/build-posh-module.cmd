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
dotnet publish %THIS_DIR%..\..\Tug.Server                         -o %PUBLISH_DIR%\bin -f net452 -r win7-x64
dotnet publish %THIS_DIR%..\..\Tug.Server.Providers.Ps5DscHandler -o %PUBLISH_DIR%\bin -f net452 -r win7-x64
dotnet publish %THIS_DIR%                                         -o %PUBLISH_DIR%\bin -f net452 -r win7-x64

xcopy %THIS_DIR%posh-res %PUBLISH_DIR%\ /E

@ENDLOCAL
