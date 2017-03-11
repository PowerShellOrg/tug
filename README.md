# tug <img align="right" width="150" src="https://raw.githubusercontent.com/PowerShellOrg/tug/master/doc/art/logo/tug-logo-trans-600b.png">
Open-source, cross-platform Pull/Reporting Server for PowerShell DSC

[![Build status](https://ci.appveyor.com/api/projects/status/xw3k8flvys5g37ct?svg=true)](https://ci.appveyor.com/project/ebekker/tug)

## Status

* All Pull Mode [protocol messages](https://github.com/PowerShellOrg/tug/wiki/Standards-and-References)
  have been implemented, including Reporting
* Support for *Registration Key Authorization* has been [implemented](https://github.com/PowerShellOrg/tug/wiki/Registration-Key-Authorization)
* Several concrete implementations are being made available as pre-built *bundles* (see below)

## Overview

Tug is a collection of components that implement the PowerShell DSC Pull Mode [protocol](https://github.com/PowerShellOrg/tug/wiki/Standards-and-References) and various supporting
[behaviors](https://github.com/PowerShellOrg/tug/wiki/Registration-Key-Authorization).

Additionally, Tug implements several variations of DSC Pull Servers.  Each of these
variations will be packaged up into *bundles* with a curated set of components and
configurations that work together well for their target use case.  See the list of
current and future bundles down below.

For more details about this project, please see the [wiki pages](https://github.com/PowerShellOrg/tug/wiki).

## Pull Server Implementations

In most cases, the intent is for each of the server implementations to be a
drop-in replacement for the *Classic* Pull Server that ships with WMF v5
(i.e. Pull Server v2).  Each of these are implemented using ASP.NET Core 1.x

Because these implementations only target support and compatibility with
DSC Pull Mode protocol v2, this means nodes must implement the ConfigurationName
(not ConfigurationID) style of configuration definitions.  (This also means that
they should be compatible with the open source LCM client for Linux.)

### Bundles

Here we present a list of the current set of bundles that are being worked on.

* **Basic**:
  * a simple, strictly file-based implementation that stores all DSC assets in a
    configurable folder structure
  * current state - works but **not** yet bundled up into an easily usable form
  * the relatively simple design and implementation details make this a good
    example of how to *wire up* a custom DSC server using the components provided
    by Tug for anyone who would like to build their own

* **EFCore**:
  * builds on the **Basic** implementation but adds an EF Core back-end to store
    much of the state and meta-data for improved performance, efficiency and easy
    querying/reporting
  * current state - this is still in the design and prototyping stage, so nothing
    to show yet
  * this will be implemented in a DB-agnostic manner so that you can plug-in SQL
    Server, SQLite or whatever flavor of RDBMS you like (maybe even NoSQL when
    EF Core eventually [implements "non-relational databases"](https://github.com/aspnet/EntityFramework/wiki/Roadmap))

* [**PS5**](https://github.com/PowerShellOrg/tug/tree/master/src/Tug.Server.Providers.Ps5DscHandler):
  * a pull server that is powered by PowerShell v5 -- protocol messagess are
    implemented as callbacks to cmdlets that can be overridden/extended by users
  * current state - **fully working** and early access bundle is availble for
    [installation](https://github.com/PowerShellOrg/tug/wiki/HOWTO%3A--Deploy-Prerelease-Tug-Server-%26-PS5-Bundle)
    and [configuration](https://github.com/PowerShellOrg/tug/tree/master/src/bundles/Tug.Server-ps5/posh-res/samples)
  * includes a default set of cmdlets that implement the same behaviors as the
    **Basic** implemenation

* **PS6**:
  * a pull server that is powered by PowerShell v6
  * the design will mimic that of PS5 but the intention is to make it portable
    across any platform that supports hosting PS v6, including Windows Nano and
    Linux
  * current state - some early work has been started but mostly on hold until
    the hosting story for PS6 gets fleshed out
  * *in theory* - once PS v6 is stable this implementation would supplant the
    **PSv5** variation

* **FaaS**:
  * a pull server that is implemented using the Function-as-a-Service paradigm
    (aka *Serverless*)

  * [**FaaS - AwsLambda**](https://github.com/PowerShellOrg/tug/tree/master/src/Tug.Server.FaaS.AwsLambda):
    * a FaaS pull server for the AWS Lambda platform
    * mimics the **Basic** server with its simple design that uses S3 for back-end
      DSC asset storage with some caching for performance improvements
    * current state - core Pull Service features fully working (Reporting not
      implemented) and can be deployed from code to your own environment

  * **FaaS - AzureFunctions**:
    * a FaaS pull server built atop Azure Functions
    * this variation is only in the *planning* stage and there is no implementation yet
    

    
