This project provides a server entry-point to the Tug DSC service.

On the .NET Framework (full Framework for Windows), the service recognizes
the `--service` option to enable running in a Windows Service mode.

To properly use this mode, you need to install the application as a Windows Service.
For example, you can use the CLI `sc.exe` tool to complete this task:

```
    sc create TugService binPath= "\"c:\path\to\tug\Tug.Server.exe\" --service true --contentRoot \"c:\path\to\tug\""
    sc start TugService
```

NOTE:  the space after `binPath=` and the executable path/args is required.
