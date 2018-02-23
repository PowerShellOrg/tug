#!/usr/bin/env pwsh

##!function Publish-ToLocalTug {
##!<#
##!.SYNOPSIS
##!This cmdlet is used to publish configurations (MOF files) and supporting modules to the
##!appropriate file locations used by a local TugDSC Server instance for serving them up.
##!
##!.PARAMETER Source
##!One ore more paths to DSC configurations (MOF files) or to directories containing
##!configurations to be published.  If not specified, defaults to the current directory.
##!
##!.PARAMETER ModuleNameList
##!One or more module names that will be located in the current PowerShell module path,
##!packaged up and published for retrieval from the TugDSC server.
##!
##!.PARAMETER Force
##!Unless this flag is specified, existing configurations and modules (of the same version)
##!that are already published won't be overwritten.
##!
##!.PARAMETER ConfigSettings
##!By default, the current TugDSC configuration settings are resolved against a well-known
##!path to a configuration settings file that defines the location of configurations and
##!modules that TugDSC can serve up.  You can use this parameter to override that path
##!if your setup does not contain the TugDSC config settings file in its default location.
##!#>
##!    [CmdletBinding()]
##!    param(
##!        [string[]]$Source=$PWD,
##!        [string[]]$ModuleNameList,
##!        [switch]$Force,
##!        [string]$ConfigSettings="$($env:ProgramData)\TugDSC\Server\appsettings.json"
##!    )
##!
##!    Write-Verbose "Loading TugDSC Server configuration settings from [$($ConfigSettings)]"
##!    if (-not (Test-Path -PathType Leaf $ConfigSettings)) {
##!        Write-Error "Missing TugDSC Server configuration settings file"
##!        return
##!    }
##!
##!    $configJson = ConvertFrom-Json $ConfigSettings -ErrorAction Stop
##!
##!    Write-Verbose "Read in configuration settings:"
##!    $checksum = $configJson.appSettings.checksum.default
##!    Write-Verbose "  * defaults to checksum [$($checksum)]"
##!    $mofFilesPath = $configJson.appSettings.handler.params.ConfigurationPath
##!    Write-Verbose "  * configuration MOF files stored at [$($mofFilesPath)]"
##!    $modulesPath = $configJson.appSettings.handler.params.ModulePath
##!    Write-Verbose "Configuration default to checksum [$($checksum)]"
##!
##!    $tempMofFilesZip = [System.IO.Path]::GetTempFileName()
##!    $zipSources = @()
##!    foreach ($s in $Source) {
##!        [string]$sourcePath = $s -replace "[/\\]+$","" ## Remove any trailing slashes
##!        if ($sourcePath.EndsWith(".mof", $true)) {
##!            $zipSources += $s
##!        }
##!        else {
##!            $zipSources += ($s + "/*.mof")
##!        }
##!    }
##!    Compress-Archive -DestinationPath $tempMofFilesZip -Path $zipSources
##!}


function Publish-ModuleToLocalTug {
<#
.SYNOPSIS
Deploy DSC modules to TugDSC Server.

.DESCRIPTION
Publish DSC module using Module Info object as an input. 
The cmdlet will figure out the location of the module repository using appsettings.json of the TugDSC Server.

.PARAMETER Name
Name of module.

.PARAMETER ModuleBase
This is the location of the base of the module.

.PARAMETER Version
This is the version of the module.

.PARAMETER ConfigSettings
Optionally, override the default location to read TugDSC Server configuration settings from.
This is needed to resolve the directory to which the modules will be published.  This is not
needed if the output directory is explecitly specified.

.PARAMETER OutputFolderPath
By default, this will be resolved by inspecting the TugDSC Server's configuration settings.

.PARAMETER Force
Forces an overwrite of the published module files if they already exist in the target publish
directory.

.EXAMPLE
Get-Module <ModuleName> | Publish-ModuleToLocalTug
#>
    [CmdletBinding()]
    [Alias("mod2tug")]    
    [OutputType([void])]
    param(
        [Parameter(Mandatory=$true, Position=0,
            ValueFromPipelineByPropertyName=$true)]
        [string]$Name,
                
        [Parameter(Mandatory=$true, Position=1,
            ValueFromPipelineByPropertyName=$true)]
        [string]$ModuleBase,
        
        [Parameter(Mandatory = $true, Position=2,
            ValueFromPipelineByPropertyName = $true)]
        [string]$Version,

        [string]$ConfigSettings = "$($env:ProgramData)\TugDSC\Server\appsettings.json",

        [string]$OutputFolderPath = $null,

        [switch]$Force
    )

    begin {
        if (-not $OutputFolderPath) {
            Write-Verbose "Loading TugDSC Server configuration settings from [$($ConfigSettings)]"
            if (-not (Test-Path -PathType Leaf $ConfigSettings)) {
                Write-Error "Missing TugDSC Server configuration settings file (appsettings.json)"
                return
            }
            
            $configJson = ConvertFrom-Json $ConfigSettings -ErrorAction Stop
            
            Write-Verbose "Read in configuration settings:"
            $modulesPath = $configJson.appSettings.handler.params.ModulePath
            Write-Verbose "  * modules stored at [$($modulesPath)]"
            $mofFilesPath = $configJson.appSettings.handler.params.ConfigurationPath
            Write-Verbose "  * configuration MOF files stored at [$($mofFilesPath)]"

            $OutputFolderPath = $modulesPath
        }
    }
    process {
        Write-Verbose "Name: $Name , ModuleBase : $ModuleBase ,Version: $Version"
        $targetPath = Join-Path $OutputFolderPath "$($Name)_$($Version).zip"

        if (Test-Path $targetPath) {
            if (-not $Force) {
                Write-Error "Existing published module found.  Specify -Force flag to overwrite."
                return
            }
            Compress-Archive -DestinationPath $targetPath -Path "$($ModuleBase)\*" -Update -Verbose
        }
        else {
            Compress-Archive -DestinationPath $targetPath -Path "$($ModuleBase)\*" -Verbose
        }
    }
    end {
        ## NOTE, we don't have to compute a checksum because that's
        ## handled by the BasicDscHandler that comes with TugDSC
    }
} 

function Publish-MOFToLocalTug {
<#
.SYNOPSIS
Deploy DSC Configuration document to the TugDSC Server.

.DESCRIPTION
Publish MOF file to the TugDSC Server. It takes File Info object as pipeline input.
It also auto detects the location of the configuration repository using the appsettings.json of the TugDSC.

.PARAMETER FullName
Absolute path to the DSC configuration (MOF) file.

.PARAMETER ConfigSettings
Optionally, override the default location to read TugDSC Server configuration settings from.
This is needed to resolve the directory to which the MOF files will be published.  This is not
needed if the output directory is explecitly specified.

.PARAMETER OutputFolderPath
By default, this will be resolved by inspecting the TugDSC Server's configuration settings.

.PARAMETER Force
Forces an overwrite of the published MOF files if they already exist in the target publish
directory.

.EXAMPLE
dir <path>\*.mof | Publish-MOFToLocalTug
#>
    [CmdletBinding()]
    [Alias("mof2tug")]
    [OutputType([void])]
    param(
        # Mof file Name
        [Parameter(Mandatory = $true,Position=0,
            ValueFromPipelineByPropertyName = $true)]
        [string]$FullName,
       
        [string]$ConfigSettings = "$($env:ProgramData)\TugDSC\Server\appsettings.json",

        [string]$OutputFolderPath = $null,

        [switch]$Force
    )

    begin {
        if (-not $OutputFolderPath) {
            Write-Verbose "Loading TugDSC Server configuration settings from [$($ConfigSettings)]"
            if (-not (Test-Path -PathType Leaf $ConfigSettings)) {
                Write-Error "Missing TugDSC Server configuration settings file (appsettings.json)"
                return
            }
            
            $configJson = ConvertFrom-Json $ConfigSettings -ErrorAction Stop
            
            Write-Verbose "Read in configuration settings:"
            $modulesPath = $configJson.appSettings.handler.params.ModulePath
            Write-Verbose "  * modules stored at [$($modulesPath)]"
            $mofFilesPath = $configJson.appSettings.handler.params.ConfigurationPath
            Write-Verbose "  * configuration MOF files stored at [$($mofFilesPath)]"

            $OutputFolderPath = $mofFilesPath
        }
    }
    process {
        $sourceInfo = [System.IO.FileInfo]::new($FullName)
        if ($sourceInfo.Extension -ine '.mof') {
            Write-Error "Invalid file $FullName. Only mof files can be copied to the pullserver configuration repository"
            return
        }

        $targetPath = Join-Path $OutputFolderPath $sourceInfo.Name
        if ((Test-Path $targetPath) -and -not $Force) {
            Write-Error "Existing published configuration found.  Specify -Force flag to overwrite."
            return
        }

        if (-not (Test-Path $FullName)) {
            Write-Error "File not found at $FullName"
        }

        Copy-Item $FullName $OutputFolderPath -Verbose -Force
    }
    end {
        ## NOTE, we don't have to compute a checksum because that's
        ## handled by the BasicDscHandler that comes with TugDSC
    }
}

Export-ModuleMember -Function Publish-*
