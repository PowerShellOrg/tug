<#
 # PowerShell.org Tug DSC Pull Server
 # Copyright (c) The DevOps Collective, Inc.  All rights reserved.
 # Licensed under the MIT license.  See the LICENSE file in the project root for more information.
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
$dscReportsPath  = [System.IO.Path]::GetFullPath(
        $handlerAppConfiguration["handler:params:ReportsPath"])

## Log out the resolved paths
$handlerLogger.LogInformation("Resolved App Settings:")
$handlerLogger.LogInformation("  * dscRegSavePath = [$dscRegSavePath]")
$handlerLogger.LogInformation("  * dscConfigPath  = [$dscConfigPath]")
$handlerLogger.LogInformation("  * dscModulePath  = [$dscModulePath]")
$handlerLogger.LogInformation("  * dscReportsPath = [$dscReportsPath]")

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
if (!(Test-Path -Path $dscReportsPath)) {
    New-Item -ItemType Directory -Force -Path $dscReportsPath
}

function Register-TugNode {
<#
.SYNOPSIS
Save node registration information.
.DESCRIPTION
Register-TugNode will register a node with the Tug Server, saving the node-specific information in a JSON file for later use.  It will update registration information if a change in registration information is detected.
Because a node can register itself multiple times, once for each type of supported DSC service
endpoint and because the details of each registration may differ, we need to capture the details
of each registration type individually, distinguished by the value of the JSON payload at the path
`RegistrationInformation.RegistrationMessageType`, which can have one of 3 values:
* ConfigurationRepository
* ResourceRepository
* ReportServer
.PARAMETER AgentID
The node's calculated AgentID from the LCM.
.PARAMETER Details
The registration details received from the LCM / Tug Server.
.OUTPUTS
This cmdlet is not expected to return any output, and in fact doing so will produce
an exception that will surface to the calling client as an unexpected error.
#>

[cmdletbinding()]
    param(
        [Parameter(Mandatory=$True)]
        #[ValidateCount(1)]
        [guid]$AgentId,
        
        [Parameter(Mandatory=$True)]
        #[ValidateCount(1)]
        [Tug.Model.RegisterDscAgentRequestBody]$Details
    )
    
    BEGIN {
        $handlerLogger.LogTrace("REGISTER: $($PSBoundParameters | ConvertTo-Json -Depth 3)")
        $regInfo = $Details | ConvertTo-Json -Depth 10
        $regType = $Details.RegistrationInformation.RegistrationMessageType
        ## Default to ConfigRepo reg type file
        $regFile = [System.IO.Path]::Combine($dscRegSavePath, "$($AgentId).json")
        if ($regType -ne [Tug.Model.CommonRegistrationMessageTypes]::ConfigurationRepository) {
            $regFile = [System.IO.Path]::Combine($dscRegSavePath, "$($AgentId)_$($regType).json")
        }
    }

    PROCESS {

        #New Registration
        if (!(Test-Path $regFile)) {
            $handlerLogger.LogDebug("Saving node reg details [$regFile]: $regInfo")
            Set-Content -Path $regFile -Value $regInfo
            }
    
        #Update existing registration
        else {
            $TempFile = "$dscRegSavePath\Temp$($AgentID).json"
            set-content -path $TempFile -value $reginfo
            if ((compare-object -referenceObject (get-content $regFile) -differenceObject (get-content $TempFile)) -ne $Null) {
                $handlerLogger.LogDebug("Updating node reg details [$regFile]: $regInfo")
                remove-item -path $regFile -force
                rename-item -path $tempFile -newName $regFile
                }
            else {
   
            #Registration information is current
            $handlerLogger.logDebug("Node registration details are current.  No changes necessary.")
            remove-item -path $tempFile -force
            }
        }
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

    $modulePath = [System.IO.Path]::Combine($dscModulePath, "$($ModuleName)_$($ModuleVersion).zip")
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
<#
.SYNOPSIS
Save node status report.
.DESCRIPTION
New-TugNodeReport will receive any status reports that are submitted to the pull server.
The content and structure of reports is not well-defined, only that it is a blob of JSON
data.  Through observation, the basic common elements of the *outer* report structure
have been compiled into the model object Tug.Model.SendReportBody.  Within this model
class several fields support nested or *inner* JSON structures, which vary and do not
necessarily follow a strict form.
.PARAMETER AgentId
The node's calculated Agent ID from the LCM.
.PARAMETER Report
The report model object instance.
.OUTPUTS
This cmdlet is not expected to return any output, and in fact doing so will produce
an exception that will surface to the calling client as an unexpected error.
#>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$True)]
        [guid]$AgentId,
        [Parameter(Mandatory=$True)]
        [Tug.Model.SendReportBody]$Report
    )
    $handlerLogger.LogTrace("NEW-REPORT: $($PSBoundParameters | ConvertTo-Json -Depth 3)")

    ## TODO:  this is where we may want to validate Agent ID is a real, registere ID

    $reportsDir = [System.IO.Path]::Combine($dscReportsPath, "$($AgentId)")
    $reportPath = [System.IO.Path]::Combine($dscReportsPath, "$($AgentId)/$($Report.JobId).json")

    ## Unfortunately, the PS built-in JSON converters don't respect the JSON.NET
    ## attributes that control how objects are serialized so if we want to preserve
    ## the report-saving behavior of Classic DSC Pull Server exactly, we can't use this
    ## NOTE:  This is *NOT* strictly necessary, but it breaks some of our
    ##        Unit Tests that validate Pull Server Protocol Compatibility
    #$reportJson = $Report | ConvertTo-Json -Depth 10
    $reportJson = [Newtonsoft.Json.JsonConvert]::SerializeObject($Report)

    if (-not (Test-Path -PathType Container $reportsDir)) {
        $handlerLogger.LogTrace("Creating FIRST-TIME reports container [$reportsDir]")
        mkdir $reportsDir -Force | Out-Null
    }
    $handlerLogger.LogTrace("Saving report as [$reportPath]")
    [System.IO.File]::WriteAllText($reportPath, $reportJson)
}

function Get-TugNodeReports {
<#
.SYNOPSIS
Retrieve one or all status reports for a given Agent ID.
.DESCRIPTION
Get-TugNodeReports is invoked when a client requests either a single specific report
for a given Agent ID, identified by a unique Job ID, or all reports for a given Agent ID
identified by the lack of any Job ID.
.PARAMETER AgentId
The node's calculated Agent ID from the LCM.
.PARAMETER JobId
An optional ID indicating a specific report.
.OUTPUTS
System.Collections.Generic.IEnumerable<Tug.Model.SendReportBody>.
Returns one or more report model objects as per the requested parameters.
#>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$True)]
        [guid]$AgentId,
        [Parameter(Mandatory=$False)]
        [nullable[guid]]$JobId
    )
    $handlerLogger.LogTrace("GET-REPORTS: $($PSBoundParameters | ConvertTo-Json -Depth 3)")

    $reportsDir = [System.IO.Path]::Combine($dscReportsPath, "$($AgentId)")
    $reportPath = [System.IO.Path]::Combine($dscReportsPath, "$($AgentId)/$($Report.JobId).json")

    if ($JobId) {
        ## Job ID was specified, return only that one if it exists

        if (-not (Test-Path -PathType Leaf $reportPath)) {
            ## Returning null signifies missing file (404)
            $handlerLogger.LogWarning("Could not find the requested Job ID [$$reportPath]")
            return $null
        }

        $reportJson = [System.IO.File]::ReadAllText($reportPath)
        $handlerLogger.LogTrace("Sending requested Job ID read from [$reportPath]")
        Write-Output [Tug.Model.SendReportBody](Get-Content -Path $regPath | ConvertFrom-Json)
    }
    else {
        ## No Job ID was specified, return all reports for the given
        ## Agent ID to mimic behavior of the Classic Pull Server
        if (-not (Test-Path -PathType Container $reportsDir)) {
            $handlerLogger.LogWarning("Could not find the requested reports container [$reportsDir]")
            return $null
        }

        $handlerLogger.LogTrace("Sending all reports for container [$reportsDir]")       
        foreach ($r in (dir $reportsDir | select -ExpandProperty FullName)) {
            $reportJson = [System.IO.File]::ReadAllText($r)
            $report = [Tug.Model.SendReportBody]($reportJson | ConvertFrom-Json)
            $handlerLogger.LogTrace("  * sending report [$report]")
            Write-Output $report
        }
    }
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
