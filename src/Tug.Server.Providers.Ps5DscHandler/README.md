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

### PS5 Handler Cmdlet Invocation

Once the PS5 Handler is initialized, the handler is ready to receive requests
from the Tug Server for any requests received from DSC Nodes.  There are six
(6) protocol messages that are supported in MS-DSCPM (DSC Pull Mode) v2 protocol
and each of these corresponds to a PowerShell cmdlet name that will be invoked
by the handler in the context of the previously allocated Runspace:

#### Registering a DSC Node:  `Register-TugNode`

This message corresponds to the MS-DSCPM v2 message `RegisterDscAgent`.

This cmdlet is invoked with two parameters and is not expected to return
anything if the Node is successfully registered.  If there are any failures
or unexpected conditions, the cmdlet should throw an exception that will
propagate up the Tug Server request/response pipeline as an error response.

##### Cmdlet:  `Register-TugNode`

##### Parameters:
* `[guid] $AgentId`
* `[[Tug.Model.RegisterDscAgentRequestBody](https://github.com/PowerShellOrg/tug/blob/master/src/Tug.Base/Model/RegisterDscAgentRequestBody.cs)] $Details`

##### Return:
* SUCCESS - no return expected
* FAILURE - throw an exception

#### Getting the Node's Configuration Status:  `Get-TugNodeAction`

This message corresponds to the MS-DSCPM v2 message `GetDscAction`.

This cmdlet is invoked with two parameters and is expected to return
a status object indicating whether the calling Node needs to retrieve
and updated DSC configuration (MOF) or whether the Node already has
a current configuration.

##### Cmdlet:  `Get-TugNodeAction`

##### Parameters:
* `[guid] $AgentId`
* `[[Tug.Model.GetDscActionRequestBody](https://github.com/PowerShellOrg/tug/blob/master/src/Tug.Base/Model/GetDscActionRequestBody.cs)] $Details`

##### Return:
* SUCCESS - `[[Tug.Server.ActionStatus](https://github.com/PowerShellOrg/tug/blob/master/src/Tug.Server.Base/ActionStatus.cs)]`
* FAILURE - throw an exception

#### Getting a DSC Configuration (MOF):  `Get-TugNodeConfiguration`

#### Getting a DSC Resource Module:  `Get-TugModule`

#### Sending a Node Status Report:  `New-TugNodeReport`

***TBD***

#### Getting Node Status Reports:  `Get-TugNodeReports`

***TBD***

### Logging from your script - `$handlerLogger`

### Accessing app-wide settings - `$handlerAppConfiguration`

