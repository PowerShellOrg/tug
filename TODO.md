# Brainstorming

This is a place to capture random thoughts and ideas.  If/when these start to get developed,
they should be moved to a [ticket](https://github.com/PowerShellOrg/tug/issues) where they
can be discussed, defined and designed.

* Tug Server CLI - make use of a combination of
  [`CommandLineUtils`](https://www.nuget.org/packages/Microsoft.Extensions.CommandLineUtils/)
  and/or [`Configuration.CommandLine`](https://www.nuget.org/packages/Microsoft.Extensions.Configuration.CommandLine/)
  to support a few different modes of operation and altering operational behavior:
  * Mode(s) to describe/validate configuration file - we would need to propery attributes/tagging
    to config model to support self-describing documenation and validation
    * e.g. `Tug.Server config-help` -> print out documentation on configurable settings and
      terminate
    * e.g. `Tug.Server config-check` -> validate the current config settings files/env/cli-args and
      terminate
  * Mode(s) to explore available providers and their details:
    * e.g. `Tug.Server list-handlers` -> lists out the discovered DSC Handler providers and exists
    * e.g. `Tug.Server list-auth` -> lists out the discovered authentication/authorization
    providers and exists
    * e.g. `Tug.Server show-handler <handler-name>` -> print out details (labels, description,
      platforms, etc) and parameter details (names, optionality, data types, value enums)

* Continuous Integration (CI)
  * We should setup one or more CI services to build continuously:
    * Windows:
      * AppVeyor
      * MyGet - [build services](http://docs.myget.org/docs/reference/build-services)
      * TeamCity @ [JetBrains](https://teamcity.jetbrains.com/) - [hosted version for OSS](https://blog.jetbrains.com/teamcity/2016/10/hosted-teamcity-for-open-source-a-new-home/)
      * TeamCity @ [POSH.org](https://powershell.org/build-server/) - for OSS POSH
      * [VSTS](https://www.visualstudio.com/team-services/continuous-integration/) - not sure it would be totally free, may be overkill
    * Linux (.NET Core) Only:
      * Circle CI - .NET Core supported via [docker image](https://discuss.circleci.com/t/net-projects/307/6)
      * Codeship - .NET Core supported via docker image (confirmed with tech chat)  
      * Travis CI - [.NET Support](https://docs.travis-ci.com/user/languages/csharp/)
      * [Others](https://github.com/ligurio/Continuous-Integration-services/blob/master/continuous-integration-services-list.md)
  
  * Auto deployments to some hosting service for simple testing?
    * Heroku - .NET Core support appears to be limited right now

* Tug Client - develop a compatible client library to address a couple immediate needs
  * Useful to interact with existing standard DSC Pull Server (xDscWebService) to explore protocol
    and behavior corner cases that might be difficult or inconvenient to setup *naturally* - for
    example, to answer questions like what happens if parameter X is too large, or Y is an unknown
    value; this would be useful for establishing 100% compatibility between Tug Server and standard
  * Useful to support automated unit testing, particularly of the protocol layer of the server;
    could also be useful in other types of testing, such as integration and performance (e.g.
    bombarding Tug Server with 1000's of simultaneous *simulated* clients)

* MOF Parser - this is more of a "nice to have" idea:
  * if we were able to parse MOF files, we could extract all the referenced DSC Resource
    modules which could be used by the client to support better test automation, i.e.
    fetch a Configuration, parse it and fetch all the modules referenced within
  * on the server side -- this could be used by a handler to parse any configurations that
    are stored within, for example, when they are uploaded or upon startup, and could be
    used to do any number of things, such as:
    * *pre-fetch* the DSC Resource modules so they are available to nodes if needed
    * determine if the modules pass some set of validations (i.e. blacklisting modules
      or allowing only known, tested modules)
    * simply validating the MOF is a valid file
  * The latest [MOF Specification](http://www.dmtf.org/sites/default/files/standards/documents/DSP0221_3.0.0.pdf)
    can be found on the [DMTF site for the CIM standard](http://www.dmtf.org/standards/cim).
  * Looks like the most [recent](https://github.com/antlr/antlr4/issues/1142), or
    perhaps [prerelease](https://github.com/antlr/antlrcs/issues/42) versions of
    [ANTLR](http://antlr.org) already have support for .NET Standard
