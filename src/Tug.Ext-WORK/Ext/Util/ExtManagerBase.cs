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
    /// Defines a base implementation of <see cref="IExtManager<TE, TExt>"/>
    /// that supports dynamic extension provider discovery and loading from
    /// built-in assemblies and file paths. 
    /// </summary>
    public abstract class ExtManagerBase<TExt, TAtt> : IExtManager<TExt, TAtt>
        where TExt : IExtension
        where TAtt : ExtensionAttribute
    {

        private List<Assembly> _BuiltInAssemblies = new List<Assembly>();

        private List<Assembly> _SearchAssemblies = new List<Assembly>();

        private List<string> _SearchPaths = new List<string>();

        private TExt[] _foundProviders = null;

        protected ExtManagerBase()
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
        public IEnumerable<string> FoundExtensionNames
        {
            get
            {
                if (_foundProviders == null)
                    FindProviders();
                return new string[0]; // _foundProviders;
            }
        }

        public TExt GetExtension(string name)
        {
            // return FoundProviders.FirstOrDefault(
            //     ep => name.Equals(ep.Describe().Name));
            throw new NotImplementedException();
        }

        /// <summary>
        /// Resets the list of built-in assemblies to be searched.
        /// </summary>
        protected ExtManagerBase<TExt, TAtt> ClearBuiltIns()
        {
            _BuiltInAssemblies.Clear();
            return this;
        }

        /// <summary>
        /// Adds one or more built-in assemblies to be searched for matching provider
        /// implementations.
        /// </summary>
        protected ExtManagerBase<TExt, TAtt> AddBuiltIns(params Assembly[] assemblies)
        {
            foreach (var a in assemblies)
                if (!_BuiltInAssemblies.Contains(a))
                    _BuiltInAssemblies.Add(a);

            return this;
        }

        /// <summary>
        /// Resets the list of external assemblies to be searched.
        /// </summary>
        protected ExtManagerBase<TExt, TAtt> ClearSearchAssemblies()
        {
            _SearchAssemblies.Clear();
            return this;
        }

        /// <summary>
        /// Adds one or more exteranl assemblies to be searched for matching provider
        /// implementations.
        /// </summary>
        protected ExtManagerBase<TExt, TAtt> AddSearchAssemblies(params Assembly[] assemblies)
        {
            foreach (var a in assemblies)
                if (!_SearchAssemblies.Contains(a))
                    _SearchAssemblies.Add(a);

            return this;
        }

        /// <summary>
        /// Resets the list of directory paths to be searched.
        /// </summary>
        protected ExtManagerBase<TExt, TAtt> ClearSearchPaths()
        {
            _SearchPaths.Clear();
            return this;
        }

        /// <summary>
        /// Adds one or more directory paths to be searched for matching provider
        /// implementations.
        /// </summary>
        protected ExtManagerBase<TExt, TAtt> AddSearchPath(params string[] paths)
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
        protected IEnumerable<TExt> FindProviders()
        {
            var conventions = new ConventionBuilder();
            // conventions.ForTypesDerivedFrom<TExt>()
            conventions.ForTypesMatching<TExt>(MatchProviderType)
                    .Export<TExt>()
                    .Shared();

            var existingPaths = _SearchPaths.Where(x => Directory.Exists(x));
            var configuration = new ContainerConfiguration()
                    .WithAssemblies(_BuiltInAssemblies, conventions)
                    .WithAssemblies(_SearchAssemblies, conventions)
                    .WithAssembliesInPaths(existingPaths.ToArray(), conventions);

            using (var container = configuration.CreateContainer())
            {
                _foundProviders = container.GetExports<TExt>().ToArray();
            }

            return _foundProviders;
        }

        /// <summary>
        /// Evaluates whether a candidate type is a provider type. 
        /// </summary>
        /// <remarks>
        /// The default implementation simply tests if the candidate type
        /// is a qualified descendent of the TExt provider type and is
        /// decorated with a custom attribute of type TAtt.
        /// </remarks>
        protected bool MatchProviderType(Type candidate)
        {
            return MefExtensions.IsDescendentOf(candidate, typeof(TExt))
                    && candidate.GetTypeInfo().GetCustomAttribute<TAtt>() != null;
        }
    }
}