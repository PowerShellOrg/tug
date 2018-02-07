#!/usr/bin/env pwsh

param(
    [Parameter(Mandatory)]
    [ValidateSet(
        'netcore')]
    [string]$RunProfile,

    [ValidateSet(
        'netcoreapp2.0',
        'net461')]
    $Framework = 'netcoreapp2.0'
)

$runProfilePath = [System.IO.Path]::Combine($PSScriptRoot, "run-profiles/$($RunProfile)")
$jsonFile = [System.IO.Path]::Combine($runProfilePath, "appsettings.json")

$runProjName = "TugDSC.Client.CLIApp"
$runProjRoot = [System.IO.Path]::Combine($PSScriptRoot, "..\..\src\$($runProjName)")

try {
    Push-Location $runProfilePath
    & dotnet run -p $runProjRoot -f $Framework --config $jsonFile
}
finally {
    Pop-Location
}
