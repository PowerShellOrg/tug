/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Tug.Util
{
    /// <summary>
    /// A <see cref="IConfiguration">configuration</see> implementation
    //  that wraps another instance and prevents modifications to the wrapped instance.
    /// </summary>
    /// <remarks>
    /// When wrapping another instance, you can optionally indicate whether write
    /// attempts generate exceptions or are silently ignored.
    /// <p>
    /// Upon first access, nested configuration elements are themselves wrapped in read-only
    /// implementations before being returned.
    /// </p>
    /// </remarks>
    public class ReadOnlyConfiguration : IConfiguration
    {
        private IConfiguration _inner;
        private bool _throwOnWrite;

        private Dictionary<string, ReadOnlyConfigurationSection> _children;

        /// <param name="throwOnWrite"><c>true</c> by default which indicates
        ///     exceptions will be thrown for any write attempts</param>
        public ReadOnlyConfiguration(IConfiguration inner, bool throwOnWrite = true)
        {
            if (inner == null)
                throw new ArgumentNullException(nameof(inner));
            _inner = inner;
            _throwOnWrite = throwOnWrite;
        }

        public string this[string key]
        {
            get { return _inner[key]; }
            set
            {
                if (_throwOnWrite)
                {
                    throw new InvalidOperationException(
                            /*SR*/"Attempt to write a read-only configuration");
                }
            }
        }

        public IConfigurationSection GetSection(string key)
        {
            var cs = _inner.GetSection(key);
            return cs == null ? null : GetReadOnlySection(key);
        }

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            foreach (var cs in _inner.GetChildren())
            {
                yield return GetReadOnlySection(cs.Key);
            }
        }

        public IChangeToken GetReloadToken()
        {
            return _inner.GetReloadToken();
        }

        private ReadOnlyConfigurationSection GetReadOnlySection(string key)
        {
            if (_children == null)
                _children = new Dictionary<string, ReadOnlyConfigurationSection>();
            
            if (!_children.ContainsKey(key))
                _children.Add(key, new ReadOnlyConfigurationSection(_inner.GetSection(key)));
            
            return _children[key];
        }
    }
}