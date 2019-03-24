// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;

namespace TugDSC.Ext.Util
{
    /// <summary>
    /// Defines extension methods on components of the MEF 2 framework.
    /// </summary>
    public static class MefExtensions
    {
        public static readonly IEnumerable<string> DEFAULT_PATTERNS = new string[] { "*.dll" };

        /// <summary>
        /// Adds one or more directory paths to be searched for assemblies
        /// that are to be inspected for any matching assemblies when resolving
        /// for components.
        /// </summary>
        /// <param name="configuration">the configuration to add paths to</param>
        /// <param name="paths">one or more paths to include in the search</param>
        /// <param name="searchOption">specifies to search either for top-level directories
        ///             only or to descend into child dirs too</param>
        /// <param name="patterns">one or more wildcard patterns to search for;
        ///             defaults to '*.dll'</param>
        /// <returns></returns>
        /// <remarks>
        /// Based on:
        ///    http://weblogs.asp.net/ricardoperes/using-mef-in-net-core
        /// </remarks>
        public static ContainerConfiguration WithAssembliesInPaths(
                this ContainerConfiguration configuration,
                IEnumerable<string> paths,
                AttributedModelProvider conventions = null,
                SearchOption searchOption = SearchOption.TopDirectoryOnly,
                IEnumerable<string> patterns = null)
        {
            if (patterns == null)
                patterns = DEFAULT_PATTERNS;

            foreach (var p in paths)
            {
                foreach (var r in patterns)
                {
                    var assemblies = Directory
                        .GetFiles(p, r, searchOption)
                        .Select(LoadFromAssembly)
                        .Where(x => x != null)
                        .ToList();

                    configuration.WithAssemblies(assemblies, conventions);
                }
            }

            return configuration;
        }

        public static Assembly LoadFromAssembly(string path)
        {
            return Assembly.LoadFile(path);
        }

        // Stolen from the guts of MEF ConventionBuilder code,
        // implements the default type selection logic of
        //    ConventionBuilder.ForTypesDerivedFrom<T>()
        internal static bool IsDescendentOf(Type type, Type baseType)
        {
            if (type == baseType || type == typeof(object) || type == null)
                return false;

            TypeInfo typeInfo1 = type.GetTypeInfo();
            TypeInfo typeInfo2 = baseType.GetTypeInfo();
            if (typeInfo1.IsGenericTypeDefinition)
                return MefExtensions.IsGenericDescendentOf(typeInfo1, typeInfo2);
            return typeInfo2.IsAssignableFrom(typeInfo1);
        }

        // Stolen from the guts of MEF ConventionBuilder code,
        // supports the default type selection logic of
        //    ConventionBuilder.ForTypesDerivedFrom<T>()
        internal static bool IsGenericDescendentOf(TypeInfo openType, TypeInfo baseType)
        {
            if (openType.BaseType == null)
                return false;
            if (openType.BaseType == baseType.AsType())
                return true;
            foreach (Type type in openType.ImplementedInterfaces)
            {
                if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == baseType.AsType())
                    return true;
            }
            return MefExtensions.IsGenericDescendentOf(IntrospectionExtensions.GetTypeInfo(openType.BaseType), baseType);
        }
    }
}