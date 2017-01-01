<#
 # Copyright © The DevOps Collective, Inc. All rights reserved.
 # Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 #>

## This DSC configuration is used by the unit tests to verify
## protocol compatibility between the Tug.Client library and
## a DSC Pull Server v2.  It is published to a DSC Pull Server
## setup by the configuration settings in DscPullServer.dsc.ps1 

. "$PSScriptRoot\DscCommon.ps1"

Configuration TestConfig1 {

    Import-DscResource –ModuleName 'PSDesiredStateConfiguration'

    Node TestConfig1 {
        File TempDir {
            Ensure = 'Present'
            Type = 'Directory'
            DestinationPath = 'c:\temp'
        }

        File TempFile {
            DependsOn = "[File]TempDir"
            Ensure = 'Present'
            Type = 'File'
            DestinationPath = 'c:\temp\tug-testconfig1-file.txt'
            Contents = 'This is a test!'
        }
    }
}

TestConfig1 @args

Get-ChildItem TestConfig1\*.MOF | Publish-MOFToPullServer -Verbose `
        -PullServerWebConfig "$webSitePath\web.config"
