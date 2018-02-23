
## We don't want to go any further after we encounter an error
$ErrorActionPreference = [System.Management.Automation.ActionPreference]::Stop

# # $TOOLS_PROJ_NAME = "TugDSC.Bundles.Tools"
# # $TOOLS_PROJ_ROOT = Join-Path -Resolve $PSScriptRoot ../$($TOOLS_PROJ_NAME)
# # $TOOLS_PROJ_FILE = Join-Path -Resolve $TOOLS_PROJ_ROOT "$($TOOLS_PROJ_NAME).csproj"

$HOST_PROJ_NAME = "TugDSC.Server.WebAppHost"
$HOST_PROJ_ROOT = Join-Path -Resolve $PSScriptRoot ../../$($HOST_PROJ_NAME)
$HOST_PROJ_FILE = Join-Path -Resolve $HOST_PROJ_ROOT "$($HOST_PROJ_NAME).csproj"

if (-not (Test-Path -PathType Leaf $HOST_PROJ_FILE)) {
    Write-Error "Unable to resolve Host Project build file"
    return
}

$DOTNET = "dotnet"
if (-not (Get-Command $DOTNET -ErrorAction SilentlyContinue)) {
    Write-Error "Unable to resolve DOTNET CLI tooling"
    return
}

# # ## Make sure tools are built and ready
# # & $DOTNET publish $TOOLS_PROJ_FILE

## Shortcut to get to Path-related funcs
$iopath = [System.IO.Path]

$moduleName = "TugDSC.Server.Basic"
$sourceRoot = $iopath::GetFullPath($iopath::Combine($PSScriptRoot, "src"))
$publishRoot = $iopath::GetFullPath($iopath::Combine($PSScriptRoot, "bin/publish"))

$moduleSrcDir = $iopath::Combine($sourceRoot, $moduleName)
$assetsSrcDir = $iopath::Combine($sourceRoot, "assets")

$modulePubDir = $iopath::Combine($publishRoot, $moduleName)
$webappPubDir = $iopath::Combine($modulePubDir, "webapp")
$assetsPubDir = $iopath::Combine($modulePubDir, "assets")

# $runtimeTargets = [ordered]@{
#     WIN = @{
#         framework = "net461"
#         runtimeid = "win-x64"
#     }
#     LNX = @{
#         framework = "netcoreapp2.0"
#         runtimeid = "linux-x64"
#     }
# }

# if (Test-Path $publishRoot) {
#     Write-Warning "Found existing target publish root; REMOVING [$($publishRoot)]"
#     Remove-Item -Recurse $publishRoot
# }

# foreach ($tk in $runtimeTargets.Keys) {
#     $tv = $runtimeTargets[$tk]
#     $pubout = $iopath::GetFullPath($iopath::Combine($webappPubDir, "$($tv.runtimeid)-$($tv.framework)"))
#     $publishParams = @(
#         "--output",        $pubout
#         "--framework",     $tv.framework
#         "--runtime",       $tv.runtimeid
#         "--configuration", "release"
#     )

#     Write-Host -ForegroundColor GREEN "Publishing [$($tk)] to [$($pubout)]"
#     & $DOTNET publish $HOST_PROJ_FILE @publishParams
# }

Write-Host -ForegroundColor GREEN "Copying PS Module source to [$($modulePubDir)]"
if (-not (Test-Path -PathType Container $modulePubDir)) {
    mkdir $modulePubDir -Force | Out-Null
}
Copy-Item -Path $moduleSrcDir/* -Destination $modulePubDir -Recurse

Write-Host -ForegroundColor GREEN "Copying PS Module assets to [$($assetsPubDir)]"
if (-not (Test-Path -PathType Container $assetsPubDir)) {
    mkdir $assetsPubDir -Force | Out-Null
}
Copy-Item -Path $assetsSrcDir/* -Destination $assetsPubDir -Recurse

$versFile = $iopath::Combine($publishRoot, "vers.txt")
& $DOTNET msbuild "$($PSScriptRoot)/Bundles.targets" /nologo /target:SaveVersion /property:VersOut=$versFile

$versText = [System.IO.File]::ReadAllText($versFile).Trim()
Update-ModuleManifest -Path "$($modulePubDir)/$moduleName.psd1" -ModuleVersion $versText
