/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System;
using System.Collections.Generic;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;

namespace Tug.Ext.Util
{
    /// <summary>
    /// Defines a base implementation of <see cref="IProviderManager<TProv, TProd>"/>
    /// that supports dynamic extension provider discovery and loading from
    /// built-in assemblies and file paths using the Managed Extensibility Framework
    /// (MEF) version 2, or otherwise known as the <i>light</i> edition of MEF.
    /// </summary>
    public abstract class ProviderManagerBase<TProv, TProd> : IProviderManager<TProv, TProd>
        where TProv : IProvider<TProd>
        where TProd : IProviderProduct
    {
        private ILogger _logger;

        private ServiceProviderExportDescriptorProvider _adapter;

        private List<Assembly> _BuiltInAssemblies = new List<Assembly>();

        private List<Assembly> _SearchAssemblies = new List<Assembly>();

        private List<string> _SearchPaths = new List<string>();

        private TProv[] _foundProviders = null;

        /// <summary>
        /// Constructs a base Provider Manager with default configuration settings. 
        /// </summary>
        /// <param name="managerLogger">logger to be used internally</param>
        /// <param name="adapter">an optional adapter to allow MEF dependencies
        ///    to be resolved by external DI sources</param>
        /// <remarks>
        /// The default configuration of the base Manager adds the assemblies
        /// containing the generic typic parameters (TProv, TProd) to be added
        /// as <i>built-in</i> assemblies, and to include all other loaded and
        /// active assemblies as searchable assemblies (and no search paths).
        /// <para>
        /// Additionally if an adapter is provided it will be added to the internal
        /// MEF resolution process as a last step in resolving dependencies.
        /// </para>
        /// </remarks>
        public ProviderManagerBase(ILogger managerLogger,
                ServiceProviderExportDescriptorProvider adapter = null)
        { 
            _logger = managerLogger;
            _adapter = adapter;

            Init();
        }

        protected virtual void Init()
        {
            // By default we include the assemblies containing the
            // principles a part of the built-ins and every other
            // assembly in context a part of the search scope
            _logger.LogInformation("Adding BUILTINS");
            AddBuiltIns(
                    typeof(TProv).GetTypeInfo().Assembly,
                    typeof(TProd).GetTypeInfo().Assembly);

#if DOTNET_FRAMEWORK
            var asms = AppDomain.CurrentDomain.GetAssemblies();
            _logger.LogInformation("Adding [{asmCount}] resolved Search Assemblies", asms.Length);
            AddSearchAssemblies(asms);
#else
            _logger.LogInformation("Resolving active runtime assemblies");
            // TODO: see if this works as expected on .NET Core
            var dc = DependencyContext.Load(Assembly.GetEntryAssembly());

            var libs = dc.RuntimeLibraries;
            var asms = new List<AssemblyName>();
            _logger.LogInformation("  discovered [{libCount}] runtime libraries", libs.Count());
            foreach (var lib in dc.RuntimeLibraries)
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug("  Runtime Library:  {rtLib}", lib.Name);

                foreach (var rtasm in lib.Assemblies)
                {
                    if (asms.Contains(rtasm.Name))
                        continue;

                    asms.Add(rtasm.Name);
                    if (_logger.IsEnabled(LogLevel.Debug))
                        _logger.LogDebug("    Adding newly found Runtime Assembly:  {rtAsm}", rtasm.Name.FullName);

                    //AssemblyLoadContext.Default.LoadFromAssemblyName(rtasm.Name);
                    AddSearchAssemblies(Assembly.Load(rtasm.Name));
                }
            }

            _logger.LogInformation("Added [{asmCount}] resolved Search Assemblies", asms.Count);
#endif
        }

        /// <summary>
        /// Lists the built-in assemblies that will be
        /// searched first for matching providers.
        /// </summary>
        public IEnumerable<Assembly> BuiltInAssemblies
        {
            get { return _BuiltInAssemblies; }
        }

        /// <summary>
        /// Lists the built-in assemblies that will be
        /// searched first for matching providers.
        /// </summary>
        public IEnumerable<Assembly> SearchAssemblies
        {
            get { return _SearchAssemblies; }
        }

        /// <summary>
        /// Lists the built-in assemblies that will be
        /// searched first for matching providers.
        /// </summary>
        public IEnumerable<string> SearchPaths
        {
            get { return _SearchPaths; }
        }

        /// <summary>
        /// Returns all the matching provider implementations that
        /// have previously been discovered.  If necessary, invokes
        /// the resolution process to find matching providers.
        /// </summary>
        public IEnumerable<string> FoundProvidersNames
        {
            get
            {
                if (_foundProviders == null)
                {
                    _logger.LogInformation("providers have not yet been resolved -- resolving");
                    this.FindProviders();
                }
                return _foundProviders.Select(p => p.Describe().Name);
            }
        }

        public TProv GetProvider(string name)
        {
            return _foundProviders.FirstOrDefault(p => name.Equals(p.Describe().Name));
        }

        /// <summary>
        /// Resets the list of built-in assemblies to be searched.
        /// </summary>
        protected ProviderManagerBase<TProv, TProd> ClearBuiltIns()
        {
            _BuiltInAssemblies.Clear();
            return this;
        }

        /// <summary>
        /// Adds one or more built-in assemblies to be searched for matching provider
        /// implementations.
        /// </summary>
        protected ProviderManagerBase<TProv, TProd> AddBuiltIns(params Assembly[] assemblies)
        {
            return AddBuiltIns((IEnumerable<Assembly>)assemblies);
        }

        /// <summary>
        /// Adds one or more built-in assemblies to be searched for matching provider
        /// implementations.
        /// </summary>
        protected ProviderManagerBase<TProv, TProd> AddBuiltIns(IEnumerable<Assembly> assemblies)
        {
            foreach (var a in assemblies)
                if (!_BuiltInAssemblies.Contains(a))
                    _BuiltInAssemblies.Add(a);

            return this;
        }

        /// <summary>
        /// Resets the list of external assemblies to be searched.
        /// </summary>
        protected ProviderManagerBase<TProv, TProd> ClearSearchAssemblies()
        {
            _SearchAssemblies.Clear();
            return this;
        }

        /// <summary>
        /// Adds one or more exteranl assemblies to be searched for matching provider
        /// implementations.
        /// </summary>
        protected ProviderManagerBase<TProv, TProd> AddSearchAssemblies(params Assembly[] assemblies)
        {
            return AddSearchAssemblies((IEnumerable<Assembly>)assemblies);
        }

        /// <summary>
        /// Adds one or more exteranl assemblies to be searched for matching provider
        /// implementations.
        /// </summary>
        protected ProviderManagerBase<TProv, TProd> AddSearchAssemblies(IEnumerable<Assembly> assemblies)
        {
            foreach (var a in assemblies)
                if (!_BuiltInAssemblies.Contains(a) && !_SearchAssemblies.Contains(a))
                    _SearchAssemblies.Add(a);

            return this;
        }

        protected static AssemblyName GetAssemblyName(string asmName)
        {
#if DOTNET_FRAMEWORK
                return AssemblyName.GetAssemblyName(asmName);
#else
                return System.Runtime.Loader.AssemblyLoadContext.GetAssemblyName(asmName);
#endif
        }

        protected static Assembly GetAssembly(AssemblyName asmName)
        {
#if DOTNET_FRAMEWORK
                return Assembly.Load(asmName);
#else
                return System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromAssemblyName(asmName);
#endif
        }

        /// <summary>
        /// Resets the list of directory paths to be searched.
        /// </summary>
        protected ProviderManagerBase<TProv, TProd> ClearSearchPaths()
        {
            _SearchPaths.Clear();
            return this;
        }

        /// <summary>
        /// Adds one or more directory paths to be searched for matching provider
        /// implementations.
        /// </summary>
        protected ProviderManagerBase<TProv, TProd> AddSearchPath(params string[] paths)
        {
            return AddSearchPath((IEnumerable<string>)paths);
        }

        /// <summary>
        /// Adds one or more directory paths to be searched for matching provider
        /// implementations.
        /// </summary>
        protected ProviderManagerBase<TProv, TProd> AddSearchPath(IEnumerable<string> paths)
        {
            foreach (var p in paths)
                if (!_SearchPaths.Contains(p))
                    _SearchPaths.Add(p);

            return this;
        }

        /// <summary>
        /// Evaluates whether a candidate type is a provider type. 
        /// </summary>
        /// <remarks>
        /// The default implementation simply tests if the candidate type
        /// is a qualified descendent of the TProv provider type.
        /// <para>
        /// Subclasses may add, or replace with, other conditions such as testing
        /// for the presence of a particular class-level custom attribute or
        /// testing for the presence of other features of the class definition
        /// such as a qualifying constructor signature.
        /// </para>
        /// </remarks>
        protected bool MatchProviderType(Type candidate)
        {
            return MefExtensions.IsDescendentOf(candidate, typeof(TProv));
        }

        /// <summary>
        /// Each time this is invoked, the search paths and patterns
        /// (built-in assemblies and directory paths + patterns) are
        /// searched to resolve matching components.  The results are
        /// cached and available in <see cref="FoundProviders">. 
        /// </summary>
        protected virtual IEnumerable<TProv> FindProviders()
        {
            try
            {
                
            _logger.LogInformation("resolving providers");

            var conventions = new ConventionBuilder();
            // conventions.ForTypesDerivedFrom<TProv>()
            conventions.ForTypesMatching<TProv>(MatchProviderType)
                    .Export<TProv>()
                    .Shared();

            var existingPaths = _SearchPaths.Where(x => Directory.Exists(x));
            var configuration = new ContainerConfiguration()
                    .WithAssemblies(_BuiltInAssemblies, conventions)
                    .WithAssemblies(_SearchAssemblies, conventions)
                    .WithAssembliesInPaths(existingPaths.ToArray(), conventions);

            if (_adapter != null)
                configuration.WithProvider(_adapter);

            using (var container = configuration.CreateContainer())
            {
                _foundProviders = container.GetExports<TProv>().ToArray();
            }

            return _foundProviders;

            }
            catch (System.Reflection.ReflectionTypeLoadException ex)
            {
                Console.Error.WriteLine(">>>>>> Load Exceptions:");
                foreach (var lex in ex.LoaderExceptions)
                {
                    Console.Error.WriteLine(">>>>>> >>>>" + lex);
                }
                throw ex;
            }
        }
    }
}