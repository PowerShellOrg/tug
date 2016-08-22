# tug
Open-source, cross-platform Pull/Reporting Server for DSC

## Status
* Currently waiting on open-source reference assemblies to be published on NuGet.
* Currently awaiting protocol clarification on how Authorization: headers are calculated in Registration traffic.

## Overview
Tug is intended to be a drop-in replacement for the Pull Server feature included in WMF5. It is written in ASP.NET Core 1.0, is open source, and should be able to run on any machine or OS that can run ASP.NET Core. The machine must also be able to run PowerShell.

Tug is essentially a thin layer. It accepts, via a web server, requests from DSC nodes. When a request is received, Tug attempts to run specific PowerShell commands (which can be cmdlets, functions, or scripts). It is these commands which provide the actual pull server functionality. This means you can write custom pull server functionality simply by writing the appropriate PowerShell commands. The Tug project is intended to include various sample commands for different scenarios, and you can obviously customize those or write entirely new ones from scratch.

Tug only supports v2 of the Pull Server protocol, which means it only supports nodes running WMF5 or later (inclusive of the open-source LCM client for Linux). Tug will not respond to v1.0 (WMF4) requests. WMF5 nodes must implement ConfigurationNames, not ConfigurationId, or they will not work with Tug.

## Authentication
Tug itself does not provide authentication per se (aside from node registration); neither does the original Microsoft pull server. However, you can apply whatever authentication your web server permits. For example, IIS can be configured to demand client certificate authentication.

## Standards and References
Tug is written to correspond to the [officially documented MS-DSCPM protocol](https://msdn.microsoft.com/en-us/library/dn393548.aspx), with details of that being verified from the open-source Linux LCM code and numerous WireShark traces of unencrypted node-server communications. Those traces are included in this project for reference.
