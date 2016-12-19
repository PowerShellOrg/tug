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
