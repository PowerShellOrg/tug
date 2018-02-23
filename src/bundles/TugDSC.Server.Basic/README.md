# README - TugDSC Server Bundle

***To view this file as formatted HTML,
 go [here](https://github.com/PowerShellOrg/tug/blob/master/src/bundles/TugDSC.Server.Bundle/README.md)***

Updated for v0.7.0

## Overview

This is a pre-packaged installation of the TugDSC Pull Server.  This server
is meant to be a drop-in replacement for the *classic* DSC Pull Server v2
that ships with WMF 5.0.  It is compatible with the DSC Pull Service protocol
v2.

### Hosting Scenarios and Configuration

This server is based on ASP.NET Core 2.0 and runs on the Kestrel web server
implementation that is part of ASP.NET Core.  As of the 2.0 release, the
Kestrel server is supported as a stand-alone service that can accept and
handle requests by itself, or in concert with a *reverse proxy server* such
as IIS, Nginx or Apache.

This guide assumes that you will configure and run TugDSC Server using only
the built-in Kestrel server.  If you would like to know more about configuring
Kestrel to work with or without a reverse proxy server, please see the
[detailed documentation for Kestrel](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel?tabs=aspnetcore2x).

For details about configuring IIS as a reverse proxy for Kestrel, please see
the section [Hosting on Windows with IIS](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/?tabs=aspnetcore2x)
on the ASP.NET Core documentation site.

For details about configuring other Kestrel hosting scenarios, including
hosting on non-Windows platforms and hosting in Docker, please find
the appropriate sub-section under [Host and Deploy](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/?tabs=aspnetcore2x).

## Running

TugDSC Server can be run in a couple different *modes*.  The server can
be run interactively from the console (i.e. blocking mode).  In this
mode you can monitor log activity on the console.  You can also adjust
configuration settings using command-line parameters.  This is useful
for troubleshooting and debugging

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

