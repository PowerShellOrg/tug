# Ps5DscHandler

This project implements a DSC Handler that delegates its handling logic
to PowerShell v5 cmdlets.

Because the dependencies on PowerShell hosting are very specific to PS5
and in turn depend on .NET Framework assemblies, this project had to be
split out into its own assembly and dynamically discovered and loaded
using the DSC Handler Provider mechanism.

Thus, this handler is *only* supported when the Tug server is hosted on
.NET Framework.
