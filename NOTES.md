# Implementation Notes

## PowerShell Challenges

This is how to run a PowerShell command programmatically:

```
                    PowerShellAssemblyLoadContextInitializer.SetPowerShellAssemblyLoadContext(AppContext.BaseDirectory);
                    using (PowerShell PowerShellInstance = PowerShell.Create()) {
                        PowerShellInstance.AddScript("Write 'hello'");
                        Collection<PSObject> PSOutput = PowerShellInstance.Invoke();
                        foreach (PSObject outputItem in PSOutput) {
                            logger.LogDebug("Item");
                        }
                    }
```

However, it's presently tossing an error because it can't find several API Sets and I haven't
figured out how to get that dealt with. They're part of the core Windows C++ runtime. Microsoft
obviously has figured this out to get PowerShell _console_ running, but I don't know what I need
to do, manually, to get just the PowerShell _engine_ running. I need to disassemble the Unix
console app code and see.

In full (e.g., not Core) .NET:

```
using System.Management.Automation;
using System.Collections.ObjectModel;

# ...

using (PowerShell PowerShellInstance = PowerShell.Create())
{
        // use "AddScript" to add the contents of a script file to the end of the execution pipeline.
        // use "AddCommand" to add individual commands/cmdlets to the end of the execution pipeline.
        PowerShellInstance.AddScript("param($param1) $d = get-date; $s = 'test string value'; " +
                "$d; $s; $param1; get-service");

        // use "AddParameter" to add a single parameter to the last command/script on the pipeline.
        PowerShellInstance.AddParameter("param1", "parameter 1 value!");  

    // invoke execution on the pipeline (collecting output)
    Collection<PSObject> PSOutput = PowerShellInstance.Invoke();

    // loop through each output object item
    foreach (PSObject outputItem in PSOutput)
    {
        // if null object was dumped to the pipeline during the script then a null
        // object may be present here. check for null to prevent potential NRE.
        if (outputItem != null)
        {
            //TODO: do something with the output item 
            // outputItem.BaseOBject
        }
    }
}
```

You need a reference to System.Management.Automation, and must have the reference assembly installed (it's on NuGet and in the Windows SDK). See https://blogs.msdn.microsoft.com/kebab/2014/04/28/executing-powershell-scripts-from-c/

## Self-contained .NET Core app deployment and Linux Daemon Process

* http://cloudauthority.blogspot.co.uk/2017/01/deploying-self-contained-net-core.html

Be sure to have installed:
* [libunwind](http://www.nongnu.org/libunwind/)

This could be handy:
* https://github.com/bmc/daemonize
* man page:  http://software.clapper.org/daemonize/daemonize.html
* e.g.  `daemonize -a -c /var/appRoot -e /var/appRoot/stderr.txt -o /var/appRoot/stdout.txt -p /var/appRoot/app-pid.txt -l /var/appRoot/app-lock.txt dotnet run /path/to/app/foo.dll`

## Windows Service Hosting

### On Windows w/ .NET Framework
* https://github.com/aspnet/Home/issues/1386
* https://github.com/aspnet/Hosting/tree/dev/src/Microsoft.AspNetCore.Hosting.WindowsServices
* https://docs.microsoft.com/en-us/aspnet/core/fundamentals/hosting
  * "Hosting as a Windows Service" in the *Additional Resources* section is **not written yet** :-(
* http://stackoverflow.com/questions/37346383/hosting-asp-net-core-as-windows-service/37464074#37464074
* Older (DNX):
  * http://taskmatics.com/blog/host-asp-net-in-a-windows-service/
  * http://taskmatics.com/blog/run-dnx-applications-windows-service/

### On Windows w/ .NET Core
* https://github.com/dasMulli/dotnet-win32-service

## Pre-release Nuget Feed

Hosted on MyGet:  https://www.myget.org/F/tug/api/v2

Register as PowerShellGet Repo:
```PowerShell
PS> Register-PSRepository -Name tug-pre -SourceLocation https://www.myget.org/F/tug/api/v2 -PackageManagementProvider nuget -Verbose
```

List all available pre-release:
```PowerShell
PS> Find-Module -Repository tug-pre
```

Install Tug Server with PS5 back-end:
```PowerShell
PS> Install-Module -Repository tug-pre Tug.Server-ps5
PS> ipmo Tug.Server-ps5
PS> Install-TugServer -Verbose
```


Original SLN file:
```
C:\prj\zyborg\PowerShell-tug\tug>dotnet sln TugDSC.sln list
Project reference(s)
--------------------
src
src\Tug.Base\Tug.Base.csproj
src\Tug.Client\Tug.Client.csproj
src\Tug.Server.Base\Tug.Server.Base.csproj
src\Tug.Server\Tug.Server.csproj
src\Tug.Server.Providers.Ps5DscHandler\Tug.Server.Providers.Ps5DscHandler.csproj
src\Tug.Server.FaaS.AwsLambda\Tug.Server.FaaS.AwsLambda.csproj
src\Tug.Ext-WORK\Tug.Ext-WORK.csproj
test
test\Tug.UnitTesting\Tug.UnitTesting.csproj
client
test\client\Tug.Client-tests\Tug.Client-tests.csproj
server
test\server\Tug.Server-itests\Tug.Server-itests.csproj
test\server\Tug.Server.FaaS.AwsLambda-tests\Tug.Server.FaaS.AwsLambda-tests.csproj
test\Tug.Ext-tests\Tug.Ext-tests.csproj
test\Tug.Ext-tests-aux\Tug.Ext-tests-aux.csproj
bundles
src\bundles\Tug.Server-ps5\Tug.Server-ps5.csproj
```

## Related Links:
* https://github.com/PowerShellOrg/dsc-traek - Node.js impl of DSC pull server (from MSFT)
* https://github.com/grayzu/DSCPullServerUI - Sample Pull Server WebApp (UI) (from MSFT PS team member)
* https://www.youtube.com/watch?v=y3-_XBQTpS8 - "What's up with DSC PS?" from 2015-Sep
