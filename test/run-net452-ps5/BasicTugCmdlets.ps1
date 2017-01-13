<#
 # Copyright � The DevOps Collective, Inc. All rights reserved.
 # Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 #> 

###########################################################################
## This is a "basic" set of Tug Cmdlets that implement a simple but
## functional DSC Pull Service.  The cmdlets attempt to mimic the same
## basic functionality of the Tug.Server.Providers.BasicDscHandler
## binary (C#) handler, but it does so in PowerShell.
###########################################################################


## At our disposal we have access to several contextual variables
##  [Tug.Server.Providers.PsLogger]$handlerLogger -
##    provides a logging object specific to the handler cmdlets
##    (this is a PS-friendly instance of the ILogger interface)
##
##  [Microsoft.Extensions.Configuration.IConfiguration]$handlerAppConfiguration -
##    this provides read-only access to the resolved application-wide configuration
##
##  [Tug.Server.Provider.Ps5DscHandlercontext]$handlerContext -
##    TODO:  NOT IMPLEMENTED YET

$handlerLogger.LogInformation("Loading BASIC Tug Cmdlets...")
$handlerLogger.LogInformation("* Got Logger:  $handlerLogger")
$handlerLogger.LogInformation("* Got Config:  $handlerAppConfiguration")

## We pull out the paths we use to store things from the global app settings
$dscRegSavePath = [System.IO.Path]::GetFullPath(
        $handlerAppConfiguration["handler:params:RegistrationSavePath"])
$dscConfigPath  = [System.IO.Path]::GetFullPath(
        $handlerAppConfiguration["handler:params:ConfigurationPath"])
$dscModulePath  = [System.IO.Path]::GetFullPath(
        $handlerAppConfiguration["handler:params:ModulePath"])

## Log out the resolved paths
$handlerLogger.LogInformation("Resolved App Settings:")
$handlerLogger.LogInformation("  * dscRegSavePath = [$dscRegSavePath]")
$handlerLogger.LogInformation("  * dscConfigPath  = [$dscConfigPath]")
$handlerLogger.LogInformation("  * dscModulePath  = [$dscModulePath]")

## Make sure the paths exist
if (!(test-path -path $dscRegSavePath)) {
    new-item -ItemType Directory -Force -Path $dscRegSavePath
    }
if (!(test-path -path $dscConfigPath)) {
    new-item -ItemType Directory -Force -Path $dscConfigPath
    }
if (!(Test-Path -Path $dscModulePath)) {
    new-item -ItemType Directory -Force -Path $dscModulePath
    }


function Register-TugNode {
    param(
        [guid]$AgentId,
        [Tug.Model.RegisterDscAgentRequestBody]$Details
    )
    
    ## Return:
    ##    SUCCESS:  n/a
    ##    FAILURE:  throw an exception

    $handlerLogger.LogTrace("REGISTER: $($PSBoundParameters | ConvertTo-Json -Depth 3)")

    $regPath = [System.IO.Path]::Combine($dscRegSavePath, "$($AgentId).json")
    $regInfo = $Details | ConvertTo-Json -Depth 10
    
    #New Registration
    if (!(Test-Path $regPath)) {
        $handlerLogger.LogDebug("Saving node reg details [$regPath]: $regInfo")
        Set-Content -Path $regPath -Value $regInfo
        }
    
    #Update existing registration
    else {
        set-content -path "$dscRegSavePath\Temp.json" -value $reginfo
        if ((compare-object -referenceObject (get-content $regPath) -differenceObject (get-content "$dscRegSavePath\Temp.json")) -ne $Null) {
            $handlerLogger.LogDebug("Updating node reg details [$regPath]: $regInfo")
            Set-Content -Path $regPath -Value $regInfo
            }
        else {
   
        #Registration information is current
            $handlerLogger.logDebug("Node registration details are current.  No changes necessary.")
        }
        remove-item -path "$dscRegSavePath\Temp.json"
    }
}

function Get-TugNodeAction {
    param(
        [guid]$AgentId,
        [Tug.Model.GetDscActionRequestBody]$Detail
    )
    $handlerLogger.LogTrace("GET-ACTION: $($PSBoundParameters | ConvertTo-Json -Depth 3)")

    ## Return:
    ##    SUCCESS:  return an instance of [Tug.Server.ActionStatus]
    ##    FAILURE:  throw an exception

    $regPath = [System.IO.Path]::Combine($dscRegSavePath, "$($AgentId).json")
    $regInfo = [Tug.Model.RegisterDscAgentRequestBody](Get-Content -Path $regPath | ConvertFrom-Json)
    $nodeStatus = "OK"

    ## TODO:  we're ignoring/assuming a bunch here, like if there are multiple configs
    ## in the node registration, or if the config names don't match in $Details

    $configName = $regInfo.ConfigurationNames
    $handlerLogger.LogTrace("Resolved requested configuration name as [$configName]")
    if (-not $configName) {
        $handlerLogger.LogWarning("No configuration name specified for agent [$AgentId]")
    }
    else {
        $configPath = [System.IO.Path]::Combine($dscConfigPath, "SHARED/$($configName).mof")
        if (-not (Test-Path -PathType Leaf $configPath)) {
            $handlerLogger.LogWarning("No configuration found for name [$ConfigName]")
            return $null
        }
        $nodeStatus = "GetConfiguration"
        $configData = [System.IO.File]::ReadAllBytes($configPath)

        $checksum = Get-Sha256Checksum $configData
        if ($checksum -eq $Detail.ClientStatus.Checksum) {
            $nodeStatus = "OK"
        }
    }

    $status = [Tug.Server.ActionStatus]@{
        NodeStatus = $nodeStatus
        ConfigurationStatuses = [Tug.Model.ActionDetailsItem[]]@(
            [Tug.Model.ActionDetailsItem]@{
                ConfigurationName = $configName
                Status = $nodeStatus
            }
        )
    }
    
    $handlerLogger.LogTrace("Returning status: $($status.GetType().FullName)")
    return $status
}

function Get-TugNodeConfiguration {
    param(
        [guid]$AgentId,
        [string]$ConfigName
    )
    $handlerLogger.LogTrace("GET-CONFIGURATION: $($PSBoundParameters | ConvertTo-Json -Depth 3)")

    ## Return:
    ##    SUCCESS:  return an instance of [Tug.Server.FileContent]
    ##    FAILURE:  throw an exception

    $configPath = [System.IO.Path]::Combine($dscConfigPath, "SHARED/$($configName).mof")
    if (-not (Test-Path -PathType Leaf $configPath)) {
        $handlerLogger.LogWarning("No configuration found for name [$ConfigName]")
        return $null
    }
    $configData = [System.IO.File]::ReadAllBytes($configPath)
    $checksum = Get-Sha256Checksum $configData

    return [Tug.Server.FileContent]@{
        ChecksumAlgorithm = "SHA-256"
        Checksum = $checksum
        Content = (New-Object System.IO.MemoryStream(,$configData))
    }
}

function Get-TugModule {
    param(
        [guid]$AgentId,
        [string]$ModuleName,
        [string]$ModuleVersion
    )
    $handlerLogger.LogTrace("GET-MODULE: $($PSBoundParameters | ConvertTo-Json -Depth 3)")

    $modulePath = [System.IO.Path]::Combine($dscModulePath, "$($ModuleName)/$($ModuleVersion).zip")
    if (-not (Test-Path -PathType Leaf $modulePath)) {
        $handlerLogger.LogWarning("No module found for name [$ModuleName] version [$ModuleVersion]")
        return $null
    }
    $moduleData = [System.IO.File]::ReadAllBytes($modulePath)
    $checksum = Get-Sha256Checksum $moduleData

    return [Tug.Server.FileContent]@{
        ChecksumAlgorithm = "SHA-256"
        Checksum = $checksum
        Content = (New-Object System.IO.MemoryStream(,$moduleData))
    }
}

function New-TugNodeReport {
    $handlerLogger.LogTrace("NEW-REPORT: $($args | ConvertTo-Json -Depth 3)")

    throw "NOT IMPLEMENTED"
}

function Get-TugNodeReports {
    $handlerLogger.LogTrace("GET-REPORTS: $($args | ConvertTo-Json -Depth 3)")

    throw "NOT IMPLEMENTED"
}

function Get-Sha256Checksum {
    param(
        [byte[]]$Bytes
    )

    $sha256 = [System.Security.Cryptography.SHA256]::Create()
    $checksumBytes = $sha256.ComputeHash($Bytes)
    $checksum = [System.BitConverter]::ToString($checksumBytes).Replace("-", "")
    $sha256.Dispose()
    return $checksum
}

Write-Output "All BASIC Tug Cmdlets are defined"
