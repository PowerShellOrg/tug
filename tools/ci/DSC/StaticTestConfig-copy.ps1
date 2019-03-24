<#
 # PowerShell.org Tug DSC Pull Server
 # Copyright (c) The DevOps Collective, Inc.  All rights reserved.
 # Licensed under the MIT license.  See the LICENSE file in the project root for more information.
 #>

## The StaticTestConfig DSC configuration is meant to be used
## exactly as-is, that is the MOF and checksum files are NOT
## meant to be published/computed, but copied to the target
## DSC Pull Servers config repo so that we can perform Unit
## Tests that are validating expected content

. "$PSScriptRoot\DscCommon.ps1"

copy "$PSScriptRoot\StaticTestConfig.mof"          $configPath
copy "$PSScriptRoot\StaticTestConfig.mof.checksum" $configPath
