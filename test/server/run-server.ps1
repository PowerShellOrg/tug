#!/usr/bin/env pwsh

param(
    [Parameter(Mandatory)]
    [ValidateSet(
        'netcore',
        'net461')]
    [string]$RunProfile,

    # [ValidateSet(
    #     'netcoreapp2.0',
    #     'net461')]
    # $Framework = 'netcoreapp2.0'

    [switch]$Restart
)

$runProfilePath = [System.IO.Path]::Combine($PSScriptRoot, "run-profiles/$($RunProfile)")
$jsonFile = [System.IO.Path]::Combine($runProfilePath, "appsettings.json")

$runProjName = "TugDSC.Server.WebAppHost"
$runProjRoot = [System.IO.Path]::Combine($PSScriptRoot, "..\..\src\$($runProjName)")


if ($Restart) {
    try {
        Write-Warning "Looking for existing process [$($runProjName)]"
        Get-Process -Name $runProjName -ErrorAction Stop
        Write-Warning "Stopping existing process [$($runProjName)]"
        Stop-Process -Name $runProjName
    }
    catch {}
}


$fw = 'netcoreapp2.0'
switch ($RunProfile) {
    'netcore' {
    }
    'net461' {
        $fw = 'net461'
    }
    'net461-ps5' {
        $fw = 'net461'
    }
}

try {
    Push-Location $runProfilePath
    & dotnet run -p $runProjRoot -f $fw --config=$jsonFile
}
finally {
    Pop-Location
}
