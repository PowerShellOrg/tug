using System;
using System.Collections.Generic;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Tug.Ext.Util
{
    /// <summary>
    /// Defines a base implementation of <see cref="IExtManager<TE, TEP>"/>
    /// that supports dynamic extension provider discovery and loading from
    /// built-in assemblies and file paths. 
    /// </summary>
    public abstract class ExtManagerBaseX<TE, TEP> : IExtManagerX<TE, TEP>
        where TE : IExtension
        where TEP: IExtProviderX<TE>
    {

        private List<Assembly> _BuiltInAssemblies = new List<Assembly>();

        private List<Assembly> _SearchAssemblies = new List<Assembly>();

        private List<string> _SearchPaths = new List<string>();

        private TEP[] _foundProviders = null;

        protected ExtManagerBaseX()
        { }

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
        public IEnumerable<TEP> FoundProviders
        {
            get
            {
                if (_foundProviders == null)
                    FindProviders();
                return _foundProviders;
            }
        }

        public TEP GetProvider(string name)
        {
            return FoundProviders.FirstOrDefault(
                ep => name.Equals(ep.Describe().Name));
        }

        /// <summary>
        /// Resets the list of built-in assemblies to be searched.
        /// </summary>
        protected ExtManagerBaseX<TE, TEP> ClearBuiltIns()
        {
            _BuiltInAssemblies.Clear();
            return this;
        }

        /// <summary>
        /// Adds one or more built-in assemblies to be searched for matching provider
        /// implementations.
        /// </summary>
        protected ExtManagerBaseX<TE, TEP> AddBuiltIns(params Assembly[] assemblies)
        {
            foreach (var a in assemblies)
                if (!_BuiltInAssemblies.Contains(a))
                    _BuiltInAssemblies.Add(a);

            return this;
        }

        /// <summary>
        /// Resets the list of external assemblies to be searched.
        /// </summary>
        protected ExtManagerBaseX<TE, TEP> ClearSearchAssemblies()
        {
            _SearchAssemblies.Clear();
            return this;
        }

        /// <summary>
        /// Adds one or more exteranl assemblies to be searched for matching provider
        /// implementations.
        /// </summary>
        protected ExtManagerBaseX<TE, TEP> AddSearchAssemblies(params Assembly[] assemblies)
        {
            foreach (var a in assemblies)
                if (!_SearchAssemblies.Contains(a))
                    _SearchAssemblies.Add(a);

            return this;
        }

        /// <summary>
        /// Resets the list of directory paths to be searched.
        /// </summary>
        protected ExtManagerBaseX<TE, TEP> ClearSearchPaths()
        {
            _SearchPaths.Clear();
            return this;
        }

        /// <summary>
        /// Adds one or more directory paths to be searched for matching provider
        /// implementations.
        /// </summary>
        protected ExtManagerBaseX<TE, TEP> AddSearchPath(params string[] paths)
        {
            foreach (var p in paths)
                if (!_SearchPaths.Contains(p))
                    _SearchPaths.Add(p);

            return this;
        }

        /// <summary>
        /// Each time this is invoked, the search paths and patterns
        /// (built-in assemblies and directory paths + patterns) are
        /// searched to resolve matching components.  The results are
        /// cached and available in <see cref="FoundProviders">. 
        /// </summary>
        protected IEnumerable<TEP> FindProviders()
        {
            var conventions = new ConventionBuilder();
            // conventions.ForTypesDerivedFrom<TEP>()
            conventions.ForTypesMatching<TEP>(MatchProviderType)
                    .Export<TEP>()
                    .Shared();

            var existingPaths = _SearchPaths.Where(x => Directory.Exists(x));
            var configuration = new ContainerConfiguration()
                    .WithAssemblies(_BuiltInAssemblies, conventions)
                    .WithAssemblies(_SearchAssemblies, conventions)
                    .WithAssembliesInPaths(existingPaths.ToArray(), conventions);

            using (var container = configuration.CreateContainer())
            {
                _foundProviders = container.GetExports<TEP>().ToArray();
            }

            return _foundProviders;
        }

        /// <summary>
        /// Evaluates whether a candidate type is a provider type. 
        /// </summary>
        /// <remarks>
        /// The default implementation simply tests if the candidate type
        /// is a qualified descendent of the TEP provider type and is
        /// decorated with a custom attribute of type TEA.  
        /// </remarks>
        protected bool MatchProviderType(Type candidate)
        {
            return MefExtensions.IsDescendentOf(candidate, typeof(TEP));
        }
    }
}