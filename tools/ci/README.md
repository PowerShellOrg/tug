# Tools/CI

In here we place various resources that are used in support of our continuous integration (CI)
environments.

## AppVeyor

We use [AppVeyor](appveyor.com) (AV) to host CI process on Windows:

 > [![Build status](https://ci.appveyor.com/api/projects/status/xw3k8flvys5g37ct?svg=true)](https://ci.appveyor.com/project/ebekker/tug)

Because AV does not have full support for discoverability of .NET Core-based projects and assets,
most of the build pipeline steps (setup, build, test, etc.) are manully crafted actions to make
use of the .NET Core CLI tooling (`dotnet`).  Once .NET Core tooling completes migration back to
MSBuild-based projects, this will pby not be required, but may still offer greater flexibility.


### Using Classic DSC Pull Server v2

In order to support testing and validation of the
[MS-DSCPM](https://msdn.microsoft.com/en-us/library/dn393548.aspx)
protocol messages and behaviors, we use the Windows-based CI to setup a local DSC Pull Server
instance, using the *classic* v2 Pull Server
([xDscWebService](https://github.com/PowerShell/xPSDesiredStateConfiguration#xdscwebservice)).

This is then used to support are protocol testing:
* [x] Setup local Class DSC Pull Server (ClassicPullServer)
* [x] Test `Tug.Client` against ClassicPullServer to validate client's compliance with DSCPM protocol
* [x] Test 'Tug.Server' using 'Tug.Client' to validate server's compliance with DSCPM protocol
* [ ] Test Classic Local Configuration Manager (ClassicLCM) against 'Tug.Server' to close the loop -- TODO: this is not done yet
