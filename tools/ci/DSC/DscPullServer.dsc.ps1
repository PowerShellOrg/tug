<#
 # Copyright © The DevOps Collective, Inc. All rights reserved.
 # Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 #>

## This DSC configuration is used to setup a classic DSC Pull Server (v2) on
## the CI build host which is then used to run some unit tests which validate
## MS-DSCPM protocol compatibility and behavior of the Tug.Client library


## Based on:
##  Older (PSv4) refs:
##    https://www.penflip.com/powershellorg/the-dsc-book
##    https://alexsoury.wordpress.com/2014/04/28/learning-dsc-part-3-setting-up-a-pull-server/
##  Newer (PSv5+) refs:
##    https://github.com/PowerShell/xPSDesiredStateConfiguration
##    https://github.com/PowerShell/xPSDesiredStateConfiguration/blob/dev/Examples/Sample_xDscWebServiceRegistration.ps1
##    https://github.com/PowerShell/xPSDesiredStateConfiguration/blob/dev/Examples/Sample_xDscWebServiceRegistrationWithSecurityBestPractices.ps1
##
##  IIS DSC Admin:
##    https://github.com/PowerShell/xWebAdministration

## Manual
#Install-WindowsFeature PowerShell,PowerShell-ISE,DSC-Service

#Install-PackageProvider -Name NuGet -MinimumVersion 2.8.5.201 -Force

#Install-Module xPSDesiredStateConfiguration -Force
#Install-Module xWebAdministration -Force

## Via DSC
Configuration DscPullServer {
    param(
        [string[]]$NodeName = 'localhost',
        [string]$CertThumbprint,
        [string]$RegistrationKey
    )

    Import-DscResource -ModuleName PSDesiredStateConfiguration
    Import-DscResource -ModuleName xPSDesiredStateConfiguration -ModuleVersion 5.1.0.0
    Import-DscResource -ModuleName xWebAdministration -ModuleVersion 1.16.0.0

    . "$PSScriptRoot\DscCommon.ps1"

    ## Resolve the Certificate and its Thumbprint
    if (-not $CertThumbprint) {
        $CertThumbprint = ((dir Cert:\LocalMachine\My |
                Where-Object { $_.Subject -eq 'CN=PSDSCPullServer' } |
                Select-Object -First 1).Thumbprint)
    }

    if ($CertThumbprint) {
        Write-Warning "Using CertThumbprint [$CertThumbprint]"
    }
    else {
        Write-Warning "***********************************************"
        Write-Warning "YOU WILL NEED TO RUN THIS SCRIPT A SECOND TIME:"
        Write-Warning "  The first time we create the cert"
        Write-Warning "  The second time we retrieve it and assign it"
        Write-Warning "***********************************************"
    }

    ## Resolve the Registration Key
    if (-not $RegistrationKey) {
        if (Test-Path -Type Leaf $regKeyPath) {
            Write-Warning "Trying to load RegKey from RegKey path"
            $RegistrationKey = [System.IO.File]::ReadAllText($regKeyPath)
        }
    }
    if (-not $RegistrationKey) {
        Write-Warning "Generating NEW Registration Key"
        $RegistrationKey = [Guid]::NewGuid()
    }
    else {
        Write-Warning "Using EXISTING/PROVIDED Registration Key"
    }

    ## Define the node's configuration
    Node $NodeName {
        WindowsFeature DSCServiceFeature {
            Ensure = 'Present'
            Name = "DSC-Service"
        }
        WindowsFeature IisManagementFeature {
            Ensure = 'Present'
            Name = "Web-Mgmt-Console"
        }
        WindowsFeature IisScriptingFeature {
            Ensure = 'Present'
            Name = "Web-Scripting-Tools"
        }

        File RegKeyFile {
            Ensure = 'Present'
            Type = 'File'
            DestinationPath = $regKeyPath
            Contents = $RegistrationKey
        }

        Script PSDSCPullServerCert {
            TestScript = {
                (dir Cert:\LocalMachine\My |
                        Where-Object { $_.Subject -eq 'CN=PSDSCPullServer' } |
                        Select-Object -First 1).length -eq 1
            }.ToString()

            SetScript = {
                New-SelfSignedCertificate `
                        -DnsName PSDSCPullServer `
                        -CertStoreLocation Cert:\LocalMachine\My
            }.ToString()


            GetScript = {
                @{ Result = ((dir Cert:\LocalMachine\My |
                        Where-Object { $_.Subject -eq 'CN=PSDSCPullServer' } |
                        Select-Object -First 1).Thumbprint) }
            }.ToString()
        }

        if ($CertThumbprint) {
            xDSCWebService PSDSCPullServer {
                DependsOn = @(
                    "[WindowsFeature]DSCServiceFeature",
                    "[File]RegKeyFile"
                )
                Ensure = 'Present' # 'Absent' # 
                EndpointName = "PSDSCPullServer"
                CertificateThumbPrint = $CertThumbprint
                AcceptSelfSignedCertificates = $true
                Port = 8443
                State = 'Started'
                
                UseSecurityBestPractices = $false

                PhysicalPath = $webSitePath
                RegistrationKeyPath = $regKeyDirPath
                ModulePath = $modulePath
                ConfigurationPath = $configPath
                DatabasePath = $dbPath
            }

            xWebsite PSDSCPullServerAltPorts {
                DependsOn = @(
                    "[xDSCWebService]PSDSCPullServer"
                )
                Name = "PSDSCPullServer"
                State = 'Started'
                BindingInfo = @(
                    MSFT_xWebBindingInformation {
                        Protocol = 'http'
                        Port     = 8080
                    }
                    MSFT_xWebBindingInformation {
                        Protocol = 'https'
                        Port = 8443
                        CertificateThumbprint = $CertThumbprint
                    }
                )
            }

            # To resolve issue with:
            #    "The Module DLL 'C:\windows\SysWOW64\inetsrv\IISSelfSignedCertModule.dll' could not be loaded due to a configuration problem."
            # See https://github.com/PowerShell/xPSDesiredStateConfiguration/issues/104
            # https://powershell.org/forums/topic/defaultapppool-crashes-after-pullserver-installation/
            xWebAppPool DefaultAppPool {
                Name                  = 'DefaultAppPool'
                State                 = 'Started'
                enable32BitAppOnWin64 = $True
            }
        }
    }
}

DscPullServer @args

Start-DscConfiguration -Wait -Force -Verbose -Path .\DscPullServer
