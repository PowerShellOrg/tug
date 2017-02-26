
# Tug Server with PowerShell v5 Handler

*(To view this file as formatted HTML, go [here](https://github.com/PowerShellOrg/tug/blob/master/src/bundles/Tug.Server-ps5/posh-res/samples/README.md))*

## Overview

This is a pre-packaged installation of the Tug DSC Pull Server configured
to use the PowerShell v5 Handler for servicing DSC Pull Mode requests.

The server can be run interactively (i.e. blocking mode) from the command
line by invoking the `tug-server.cmd`.  In this mode, it will listen by
default on port `5000` on all local network interfaces.  This uses the
ASP.NET Core Kestral server which only meant for demonstration purposes
and should not be exposed to a public or untrusted network interface.

Alternatively you can run the server behind IIS.  Details for configuring
this setup can be found on the ASP.NET Core documentation site under the
section for [Publishing to IIS](https://docs.microsoft.com/en-us/aspnet/core/publishing/iis).

## Server Application Configuration

The server is configured using the `appsettings.json` file.  There are
several configuration items that you should inspect and confirm or adjust
as needed.

### Authorization

The section `appSettings:authz` is where request authorization is
configured.  By default, the Tug server is configured to use
*Registration Key Authorization* (RegKey Authz).  There are two settings
that must be provided for this authorization method:

* `RegistrationKeyPath` - specifies a path where to find the Regisration Keys
  file.  This can be a path to a file or to a folder.  If the latter, the file
  named `RegistrationKeys.txt` is assumed.  This file lists one or more
  registration keys that are used to authorize requests from approved nodes.
  The resolved registration keys file must be readable by the Tug server.
  See the sample `RegistrationKeys.txt` file for more details.

* `RegistrationSavePath` - specifies a path where Tug will save necessary
  registration state data for nodes that are authorized.
  This location must be writable by the Tug server.

### PS5 Pull Handler

#### Background

The Tug server is composed of number of extensible components that are
integrated to formulate a complete working DSC Pull Server.  In this
pre-packaged configuration, the *Pull Mode Service Handling* is
implemented using the PowerShell v5 Pull Handler (`Ps5DscHandler`).

This handler is driven by a PowerShell Runspace which is used to delegate
each of the DSC Pull Mode requests to a designated PowerShell cmdlet.
During startup, Tug will initialize the Runspace and optionally load and
execute a *bootstrap script* which may define any number of PowerShell
elements, including global variables and cmdlets such as the ones that
are delegated to during normal operation.

For more information about the PowerShell v5 Handler, please see the
handler's [project details](https://github.com/PowerShellOrg/tug/tree/master/src/Tug.Server.Providers.Ps5DscHandler).

#### Requirements

The `Ps5DscHandler` is dependent on the PowerShell v5 runtime and thus
is limited to the environment and .NET framework upon which it operates.
Specifically, this restricts use of the PS5 Handler to Windows environments
that can host the *full* .NET Framework (v4.5.2 is required by Tug) and the
Windows Management Framework (WMF) v5.

#### Configuration Settings

The pre-packaged configuration comes with a sample PowerShell script that
defines a working setup of a basic DSC Pull Service.  The `Ps5DscHandler`
and the accompanying sample PowerShell script are driven by a number of
configuration elements which you can customize in the section
`appSettings:handler` of the `appsettings.json` file:

* `provider` - specifies the name of the DSC Pull Handler that will be
  used by the Tug server to service DSC Pull Mode requests.  In this
  pre-packaged configuration, this setting specifies `ps5` to indicate
  the PowerShell v5 Pull Handler.

* `params` - this sub-section of settings is used to define
  handler-specific parameters.  For the `Ps5DscHandler`, these parameters
  are also used to pass along any settings to the PowerShell Runspace
  described above and thus can be used to alter the behavior of the
  PowerShell cmdlets that service DSC Pull requests.
  
  The following parameter settings are used to specifically configure
  the `Ps5DscHandler`:
  * `BootstrapPath` - specifies an optional path that Tug will change to
  before creating the PowerShell Runspace.  Effectively all further
  activity will be executed out of this home directory and all file-based
  operations will be relative to this path.
  * `BootstrapScript` - specifies an optional path to a PowerShell script
  that will be executed after the PowerShell Runspace is created.  During
  normal operation, the handler will delegate DSC Pull requests to
  designated cmdlets in the Runspace.  The bootstrap script is ideally
  the mechanism to define those cmdlets within the Runsapce or import
  other resources that define them.
  This file must be readable by the Tug server.
  * `BootstrapWatch` - specifies an optional list of semicolon-separated
  files to watch for changes.  If a change is detected, the current
  PowerShell Runspace is disposed of and a new Runspace is initialized
  and put into effect.  This setting is very useful during initial
  development and testing of the PowerShell scripts that are used to
  drive the behavior of the Pull Service, as the script is frequently
  updated and tested.  The files can be specified as relative paths
  which are processed as relative to the resolved `BootstrapPath`. 

  The sample bootstrap script defines each of the DSC Pull service cmdlets
  directly and provides a good base for you to explore and customize the
  default behavior.  The following parameter settings are used by the
  sample boostrap script to implement a very basic, but working DSC Pull
  Service:
  * `RegistrationSavePath` - specifies a path where node registration data
    is persisted.  This is used by the registration action to save details
    that are provided by the node, and then referenced by subsequent calls
    to resolve node state information.
    This location must be writable by the Tug server.
  * `ConfigurationPath` - specifies a path where node configuration objects
    in the form of MOF files can be found.  At present, Tug *only* supports
    DSC Pull Mode v2 configurations, which are user-friendly configuration
    names that can be shared by multiple nodes.  Tug does not currently
    support node-specific configurations that are named after a node
    configuration ID (i.e. a GUID).  Configuration files are actually
    searched for in the sub-folder `SHARED` under the path defined by this
    configuration setting.  For example, for a configuration named
    `MailServer`, Tug would expect this file to be located at:
    ```
        <ConfigurationPath>\SHARED\MailServer.mof
    ```
    This location must be readable by the Tug server.
  * `ModulePath` - specifies a path where Tug will search for DSC Resource
    Modules that are requested by nodes.  In this location, modules should
    be stored as ZIP files in the form `<ModuleName>/<ModuleVersion>.zip`.
    For example, for the DSC Resource Module `xWebAdministration`, with
    versions `1.15.0.0` and `1.16.0.0`, Tug would expect these files to be
    located at:
    ```
        <ModulePath>\xWebAdministration\1.15.0.0.zip
        <ModulePath>\xWebAdministration\1.16.0.0.zip
    ```
    This location must be readable by the Tug server.

## Server Hosting Configuration

In addition to the application configuration that drives the behavior of
Tug Server and it DSC service functionality, you can override some settings
of the Server's *Hosting* behavior using one of the following methods:
* Provide a local hosting configuration file named `hosting.json`
* Set individual hosting settings as environment variables prefixed with the name `TUG_HOST_`
* Specifify individual hosting settings as CLI arguments

The following settings can be overridden:

Setting Key Name         | Default Value | Comments
-------------------------|---------------|-----------
`applicationName`        | Tug.Server    |
`environment`            | PRODUCTION    | 
`captureStartupErrors`   | false         |
`contentRoot`            | .             | Defaults to the current working directory
`detailedErrors`         | false         |
`urls`                   | http://*:5000 | semi-colon-separate list of endpoints

***NOTE:  some of these settings are automatically overridden when
hosting using IIS Integration, such as `urls` and `contentRoot`***

## Limitations

The current version of the Tug Server and the companion PowerShell sample
script implement all the DSC actions to support the pull server
functionality, which includes node registration, status checking,
retrieval of node configurations and retrival of DSC Resource modules.

The do not currently implement or support the DSC Reporting actions.  This
functionality will be provided in a future update.
