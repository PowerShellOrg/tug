using System.Reflection;

// Version information for an assembly consists of the following four values:
//
//      M.N.P.B
//
//      Major Version (M)
//      Minor Version (N)
//      Patch Number  (P)
//      Build Number  (B)
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion(ASMINFO.VERSION + ".0")]
[assembly: AssemblyFileVersion(ASMINFO.VERSION + ".0")]

// This is used for the NuSpec version tag replacement
// and is combined with nuget-specific rev and release
[assembly: AssemblyInformationalVersion(ASMINFO.VERSION)]

// ReSharper disable once InconsistentNaming
internal static class ASMINFO
{

    // DON'T FORGET TO UPDATE APPVEYOR.YML
    // ReSharper disable once InconsistentNaming
    public const string VERSION = "0.6.0";
}
