#tug <img align="right" width="150" src="https://github.com/PowerShellOrg/tug/blob/master/doc/art/logo/tug-logo-trans-600b.png">
Open-source, cross-platform Pull/Reporting Server for DSC

[![Build status](https://ci.appveyor.com/api/projects/status/xw3k8flvys5g37ct?svg=true)](https://ci.appveyor.com/project/ebekker/tug)

## Status
* Currently at proof-of-concept; correctly calculates Authorization header given a hardcoded Registration Key
* Unfortunately, load-testing suggests that spawning a new PowerShell process for each request is going to suck
  from a performance perspective. Because .NET Core is presently not capable of hosting the PowerShell Core engine,
  this presents some difficulties. Going to do some back-end testing just on Windows in normal ASP.NET to see if
  hosting the shell directly provides sufficient performance gain.

## Overview
Tug is intended to be a drop-in replacement for the Pull Server feature included in WMF5.
It is written in ASP.NET Core 1.0, is open source, and should be able to run on any machine
or OS that can run ASP.NET Core. The machine must also be able to run PowerShell.

Tug is essentially a thin layer. It accepts, via a web server, requests from DSC nodes.
When a request is received, Tug attempts to run specific PowerShell commands (which can be
cmdlets or functions).  It is these commands which provide the actual pull server functionality.
This means you can write custom pull server functionality simply by writing the appropriate
PowerShell commands. The Tug project is intended to include various sample commands for different
scenarios, and you can obviously customize those or write entirely new ones from scratch.

Tug only supports v2 of the Pull Server protocol, which means it only supports nodes running WMF5
or later (inclusive of the open-source LCM client for Linux). Tug will not respond correctly to
v1.0 (WMF4) requests. WMF5 nodes must implement ConfigurationNames, not ConfigurationId, or they
will not work with Tug (using ConfigurationId forces WMF5 clients into protocol v1 behavior).

## Authentication
Tug itself does not provide authentication per se (aside from node registration); neither does
the original Microsoft pull server. However, you can apply whatever authentication your web
server permits. For example, IIS can be configured to demand client certificate authentication.
Nodes do send client certificate information (a self-signed certificate with a 1-year life) on
registration, so you could potentially capture that information, and then configure your web
server to allow future access attempts from that client.

## Standards and References
Tug is written to correspond to the [officially documented MS-DSCPM protocol](https://msdn.microsoft.com/en-us/library/dn393548.aspx),
with details of that being verified from the open-source Linux LCM code and numerous WireShark
traces of unencrypted node-server communications. Those traces are included in this project for
reference.

<table style="border:solid blue; width: 100%!" border="0" width="100%"><tr><td><img src="https://raw.githubusercontent.com/PowerShellOrg/tug/master/doc/art/logo/tug-logo-trans-75.png"></td><td><h1>tug</h1></td></tr></table>
