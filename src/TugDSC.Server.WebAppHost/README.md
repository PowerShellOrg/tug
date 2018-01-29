# README - TugDSC Server Web App Host

This is an implementation of a DSC Pull Server implemented as a ASP.NET Core MVC Web App.

## Running as a Windows Service

You can run this TugDSC Server implementation as a Windows Service if you following these
guidelines.

* First, you must be running this TugDSC Server on the .NET Framework 4.6.1 or greater,
  so be sure that it's installed on the target host and be sure to run the version of
  the TugDSC Server application that has been compiled to target that platform.
* Next, be sure the required `appsettings.json` file is located in the same path as the
  binary which will be executed (`TugDSC.Server.WebAppHost.exe`).  Additionally, if you
  want to provide an optional hosting configuration file (`hosting.json`), it too must
  be located in the same path as the binary executable.
* Finally, you need to install the application as a Windows Service that gets invoked with the
  `--service` CLI flag, for example:
```batch
sc.exe create TugDSC binPath= "\"c:\path\to\binary\TugDSC.Server.WebAppHost.exe\" --service "
sc.exe start TugDSC
```

> NOTE: The above example is assumed to be executed from a Windows command shell (cmd.exe).
> If executing the same set of commands from PowerShell, make sure the use the appropriate
> method of escaping within quoted string (the backtick).
