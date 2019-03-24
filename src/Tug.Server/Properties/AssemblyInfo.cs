// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

using System.Reflection;
using Microsoft.Extensions.Configuration.UserSecrets;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Tug Server")]
[assembly: AssemblyDescription("Tug Server Implementation")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCulture("")]

/////////////////////////////////////////////////////////////////
// Additional "shared" assembly-level attributes are defined in
// SharedAssemblyInfo.cs and SharedAssemblyVersionInfo.cs files
/////////////////////////////////////////////////////////////////

// Defines the ID for User Secrets
[assembly: UserSecretsId("Tug.Server")]
