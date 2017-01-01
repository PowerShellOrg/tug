<#
 # Copyright © The DevOps Collective, Inc. All rights reserved.
 # Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 #>

## This DSC configuration is used by the unit tests to verify
## protocol compatibility between the Tug.Client library and
## a DSC Pull Server v2.  It is published to a DSC Pull Server
## setup by the configuration settings in DscPullServer.dsc.ps1 

Configuration ClientConfig {

    Node ClientConfig {
        File TempDir {
            Ensure = 'Present'
            Type = 'Directory'
            DestinationPath = 'c:\temp'
        }

        File TempFile {
            DependsOn = "[File]TempDir"
            Ensure = 'Present'
            Type = 'File'
            DestinationPath = 'c:\temp\foo.txt'
            Contents = 'This is a test!'
        }
    }
}

ClientConfig

Publish-MOFToPullServer -PullServerWebConfig C:\DscService\WebSite\web.config -FullName .\ClientConfig\ClientConfig.mof -Verbose
