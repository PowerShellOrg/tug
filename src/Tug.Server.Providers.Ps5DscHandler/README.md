# Ps5DscHandler

This project implements a DSC Handler that delegates its handling logic
to PowerShell v5 cmdlets.

Because the dependencies on PowerShell hosting are very specific to PS5
and in turn depend on .NET Framework assemblies, this project had to be
split out into its own assembly and dynamically discovered and loaded
using the DSC Handler Provider mechanism.

Thus, this handler is *only* supported when the Tug server is hosted on
.NET Framework.

## How to Use the PS5 Handler

The PS5 Handler delegates the handling of all MS-DSCPM (DSC Pull Mode) protocol
messages to PowerShell cmdlets.  When the handler is first initialized upon
server startup, it performs the following steps:

---
### PS5 Handler Initialization

When the Tug Server starts up, it will determine which **DSC Pull Handler**
implementation is to be loaded and used to handle DSC Pull Mode server messages.
At startup it will call upon the handler provider to instantiate the handler,
pass in any handler-specific settings, and initialize the handler for use.

During this initialization period, the PS5 Pull Handler will do the following:

* Optionally relocate the current working directory (CWD) to a path
  specified in app settings
* Construct a new PowerShell Runspace
* Make several components available to the Runspace as global-scope variables:
  * `$handlerLogger` - an `ILogger` instance that can be used by user script
    code to log to the Tug Server's logging system (more details below)
  * `$handlerAppConfiguration` - an `IConfiguration` instance that holds the
    Tug Server's app-wide settings (more details below)
  * `$handlerContext` - TBD
* Optionally execute a PowerShell script file located at a path specified
  in the app settings

---
### PS5 Handler Cmdlet Invocation

Once the PS5 Handler is initialized, the handler is ready to receive requests
from the Tug Server for any requests received from DSC Nodes.  There are six
(6) protocol messages that are supported in MS-DSCPM (DSC Pull Mode) v2 protocol
and each of these corresponds to a PowerShell cmdlet name that will be invoked
by the handler in the context of the previously allocated Runspace:

---
#### Registering a DSC Node:  `Register-TugNode`

This message corresponds to the MS-DSCPM v2 message `RegisterDscAgent`.

This cmdlet is invoked with two parameters and is not expected to return
anything if the Node is successfully registered.  If there are any failures
or unexpected conditions, the cmdlet should throw an exception that will
propagate up the Tug Server request/response pipeline as an error response.

##### CMDLET:
> `Register-TugNode`

##### PARAMETERS:

* `[ guid ] $AgentId`
* `[`[`Tug.Model.RegisterDscAgentRequestBody`](https://github.com/PowerShellOrg/tug/blob/master/src/Tug.Base/Model/RegisterDscAgentRequestBody.cs)`] $Details`

##### RETURN:
* success: *no return expected*
* failure: *throw an exception*

---
#### Getting the Node's Configuration Status:  `Get-TugNodeAction`

This message corresponds to the MS-DSCPM v2 message `GetDscAction`.

This cmdlet is invoked with two parameters and is expected to return
a status object indicating whether the calling Node needs to retrieve
and updated DSC configuration (MOF) or whether the Node already has
a current configuration.

##### CMDLET:
> `Get-TugNodeAction`

##### PARAMETERS:
* `[ guid ] $AgentId`
* `[`[`Tug.Model.GetDscActionRequestBody`](https://github.com/PowerShellOrg/tug/blob/master/src/Tug.Base/Model/GetDscActionRequestBody.cs)`] $Details`

##### RETURN:
* SUCCESS - `[`[`Tug.Server.ActionStatus`](https://github.com/PowerShellOrg/tug/blob/master/src/Tug.Server.Base/ActionStatus.cs)`]`
* FAILURE - *throw an exception*

---
#### Getting a DSC Configuration (MOF):  `Get-TugNodeConfiguration`

This message corresponds to the MS-DSCPM v2 message `GetConfiguration`.

This cmdlet is invoked with two parameters and is expected to return
a file object containing a DSC Configuration in the form of a MOF file
and related meta-data.

##### CMDLET:
>`Get-TugNodeConfiguration`

##### PARAMETERS:
* `[ guid ] $AgentId`
* `[ string ] $ConfigName`

##### RETURN:
* success: `[`[`Tug.Server.FileContent`](https://github.com/PowerShellOrg/tug/blob/master/src/Tug.Server.Base/FileContent.cs)`]`
* failure: *throw an exception*

---
#### Getting a DSC Resource Module:  `Get-TugModule`

This message corresponds to the MS-DSCPM v2 message `GetModule`.

This cmdlet is invoked with two parameters and is expected to return
a file object containing a DSC Resource Module in the form of a ZIP
archive file and related meta-data.

##### CMDLET:
> `Get-TugModule`

##### PARAMETERS:
* `[ string ] $ModuleName`
* `[ string ] $ModuleVersion`

##### RETURN:
* success: `[`[`Tug.Server.FileContent`](https://github.com/PowerShellOrg/tug/blob/master/src/Tug.Server.Base/FileContent.cs)`]`
* failure: *throw an exception*

---
#### Sending a Node Status Report:  `New-TugNodeReport`

***TBD***

---
#### Getting Node Status Reports:  `Get-TugNodeReports`

***TBD***

---
### Logging from your script - `$handlerLogger`

The globally-scoped, read-only variable `$handlerLogger` is made available
in the context of the Runspace invoked during initialization and when
invoking handler cmdlets and can be used to log messages at different
verbosity levels to the server's logging system.  The variable is an
instance of an
[`ILogger`](https://docs.microsoft.com/en-us/aspnet/core/api/microsoft.extensions.logging.ilogger)
which is defined by the
[Microsoft Logging Extension](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging).

To be more precise, the variable is concrete instance of a
[`PsLogger`](https://github.com/PowerShellOrg/tug/blob/master/src/Tug.Server.Providers.Ps5DscHandler/PsLogger.cs)
which is a specialized `ILogger` that is tailored to work well in a
PowerShell scripting context.  The Logging Extension package makes many
of the fundamental logging primitives available as .NET *extension methods*
however PowerShell does not have a natural or native way to invoke these
methods except to treat them as static method calls which can be cumbersome
and lengthy.  Instead, the `PsLogger` class redefines the most common of
these extension methods as first-class instance methods.

---
### Accessing app-wide settings - `$handlerAppConfiguration`

The Tug Server's operating behavior is largely driven by a set of app
settings that are resolved at startup by the culmination of files,
environment variables and command-line switches.  This includes settings
that are used to resolve, configure and initialize the DSC Pull Handler
provider.

These settings are passed to the PS5 Pull Handler and it will
apply those settings of which it is aware to drive its own behavior.
But it will also ignore any additional settings of which it is not
aware and you can make use of these to pass additional settings to
the PowerShell script and cmdlets that are invoked in the Runspace.

The app-wide settings are passed in as the globally-scoped, readonly
variable `$handlerAppConfiguration`.  This variable is an instance of
an [`IConfiguration`](https://docs.microsoft.com/en-us/aspnet/core/api/microsoft.extensions.configuration.iconfiguration)
which is defined by the
[Microsoft Configuration Extension](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration).

The exact implementation that is made available to the Runspace is
a specialized variation that is readonly so while the extension
interface normally allows modification to the configuration instance
or any of its children, this particular implementation will silently
ignore any such modifications.
