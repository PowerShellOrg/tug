// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Tug.Util
{
    /// <summary>
    /// A <see cref="IConfigurationSection">configuration section</see> implementation
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
    public class ReadOnlyConfigurationSection : IConfigurationSection
    {
        private IConfigurationSection _inner;
        private bool _throwOnWrite;

        private Dictionary<string, ReadOnlyConfigurationSection> _children;

        /// <param name="throwOnWrite"><c>true</c> by default which indicates
        ///     exceptions will be thrown for any write attempts</param>
        public ReadOnlyConfigurationSection(IConfigurationSection inner, bool throwOnWrite = true)
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

        public string Key
        {
            get { return _inner.Key; }
        }

        public string Path
        {
            get { return _inner.Path; }
        }

        public string Value
        {
            get { return _inner.Value; }
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
            return cs == null ? null : GetReadOnlySection(cs.Key);
        }

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            foreach (var cs in _inner.GetChildren())
                yield return GetReadOnlySection(cs.Key);
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