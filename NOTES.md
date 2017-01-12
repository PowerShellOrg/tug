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

