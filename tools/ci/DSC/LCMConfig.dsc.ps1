<#
 # Copyright © The DevOps Collective, Inc. All rights reserved.
 # Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 #>

## References:
##  * https://msdn.microsoft.com/en-us/powershell/dsc/pullclientconfignames?f=255&MSPPError=-2147217396
##  * https://mcpmag.com/articles/2016/10/13/configure-powershell-lcm.aspx
##  * https://blogs.msdn.microsoft.com/powershell/2015/05/29/how-to-register-a-node-with-a-dsc-pull-server/
##  * https://msdn.microsoft.com/en-us/powershell/dsc/metaconfig?f=255&MSPPError=-2147217396
##  * https://blogs.msdn.microsoft.com/powershell/2013/11/26/push-and-pull-configuration-modes/
## Partial Configurations:
##  * https://msdn.microsoft.com/en-us/powershell/dsc/partialconfigs?f=255&MSPPError=-2147217396

[DSCLocalConfigurationManager()]
Configuration LCMConfig
{
    Node localhost
    {
        Settings
        {
            RefreshMode = 'Pull'
            RebootNodeIfNeeded = $false
            ConfigurationMode = 'ApplyOnly'
            DebugMode = 'All'
            AllowModuleOverwrite = $false
            RefreshFrequencyMins = 30
            ConfigurationModeFrequencyMins = 60
        }

        ConfigurationRepositoryWeb config_tug
        {
            ServerURL = 'http://localhost:5000/'
           #ServerURL = 'http://DSC-SERVER1.tugnet:8080/PSDSCPullServer.svc'
            AllowUnsecureConnection = $true
           #RegistrationKey = "4008e198-e375-46be-847c-53c3c249c899"
            RegistrationKey = "f65e1a0c-46b0-424c-a6a5-c3701aef32e5"
            ConfigurationNames = @("TestConfig1")
        }

        #ResourceRepositoryWeb resource_tug
        #{
        #    ServerURL = 'http://127.0.0.1:5000/'
        #    AllowUnsecureConnection = $true
        #    #RegistrationKey = "FOO"
        #}

        #ReportServerWeb report_tug
        #{
        #    ServerURL = 'https://127.0.0.1:5000/'
        #    AllowUnsecureConnection = $true
        #    RegistrationKey = "FOO"
        #}
    }
}

LCMConfig

## Keep this in mind to apply the LCM Configuration!!!
##    https://powershell.org/forums/topic/pull-server-connection-error/
## PS> Get-Service WinRM
## PS> Start-Service WinRM

Enable-DscDebug -BreakAll
$x = Get-DscLocalConfigurationManager

#$webResp = Invoke-WebRequest http://127.0.0.1:5000/PSDSCPullServer.svc
#$webResp = Invoke-WebRequest http://192.168.1.6:5000/PSDSCPullServer.svc

## This sends RegisterAgent
Set-DscLocalConfigurationManager -ComputerName localhost -Path .\LCMConfig -Verbose

## This sends GetAction, and if necessary, GetConfiguration/GetModule
#Update-DscConfiguration -Wait -Verbose
