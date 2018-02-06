[CmdletBinding(DefaultParameterSetName="Default")]
param(
    [Parameter(ParameterSetName="Predefined")]
    [ValidateSet(
            "WithFiddler",
            "LocalhostOn5000",
            "Server1On5000",
            "Server1On5000WithFiddler",
            "Server2",
            "Server2WithFiddler",
            "Lambda",
            "LambdaWithFiddler")]
    [string]$TestRunConfig,

    [Parameter(ParameterSetName="Default")]
    [switch]$AdjustForWmf50,
    [Parameter(ParameterSetName="Default")]
    [string]$ServerUrl,
    [Parameter(ParameterSetName="Default")]
    [string]$ProxyUrl,
    [Parameter(ParameterSetName="Default")]
    [switch]$SkipBuild

    ## Can be added to either param set
    # ,[string[]]$DotnetParams
    # ,[string[]]$TestParams
)

$ThisProjName = [System.IO.Path]::GetFileName($PSScriptRoot)
$ThisProjFile = [System.IO.Path]::Combine($PSScriptRoot, "$($ThisProjName).csproj")

## Resolve pre-defined, named test configurations into their explicit parameters
switch -Wildcard ($TestRunConfig) {
    ## Anything that ends in 'WithFiddler' gets a proxy URL
    "*WithFiddler" {
        $ProxyUrl = "http://localhost:8888"
    }
    
    "LocalhostOn5000*" {
        $ServerUrl = "http://localhost:5000/"
        $SkipBuild = $true
    }
    "Server1On5000*" {
        $ServerUrl = "http://DSC-SERVER1.tugnet:5000/"
    }
    "Server2*" {
        $ServerUrl = "http://DSC-SERVER2.tugnet:8080/PSDSCPullServer.svc/"
    }

    ## Anything that starts with Lambda, needs to specify the Lambda endpoint
    "Lambda*" {
        if (-not $ServerUrl) {
            Write-Error "Lambda Server endpoint must be specified with -ServerUrl parameter"
            return
        }
    }
}

## Assemble explicit parameters
$runParams = @()
if ($SkipBuild) {
    $runParams += "--no-build"
}
$runParams += "--"
if ($ServerUrl) {
    $env:TSTCFG_server_url=$ServerUrl
}
if ($ProxyUrl) {
    $env:TSTCFG_proxy_url=$ProxyUrl
}
if ($AdjustForWmf50) {
    $env:TSTCFG_adjust_for_wmf_50="true"
}
else {
    $env:TSTCFG_adjust_for_wmf_50="false"
}

& dotnet test $ThisProjFile @runParams