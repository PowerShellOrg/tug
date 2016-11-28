# tug
Tug is an open-source, cross-platform Web service that implements the Microsoft Desired State Configuration Pull Server Model. That is, Tug can be both a Pull Server and Reporting Server for your DSC nodes (WMF5 and above; WMF4 nodes are not supported). Rather than burying the server implementation in a compiled assembly of some kind, or requiring you to learn a new language in order to extend the server, Tug allows you to design your own Pull Server functionality simply by writing PowerShell commands. You are, of course, therefore limited to using Tug on platforms where PowerShell can run ;). Tug is written in C# and Asp.Net Core 1.0, in Visual Studio Code.

## Commands
When an incoming request is received, Tug will attempt to run one of five PowerShell commands, which must be installed on the Tug server. These must be located in a legal path as defined in the PSModulePath environment variable. For example, om Windows systems, \Program Files\WindowsPowerShell\Modules is usually a valid location.

The commands are:
* Register-TugNode (registers a node with the system)
* Get-TugNodeAction (tells a node whether or not it needs to download a configuration)
* New-TugNodeReport (logs a Report Server report from a node)
* Get-TugModule (gets a DSC resource module)
* Get-TugConfiguration (returns a DSC configuration MOF)

Tug does not currently support querying the Pull Server for node status. Instead, you should store node reports in a location - like a SQL Server instance - where you can query and report using other tools.

