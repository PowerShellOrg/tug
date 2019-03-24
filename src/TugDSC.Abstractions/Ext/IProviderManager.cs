// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace TugDSC.Ext
{
    public interface IProviderManager<TProv, TProd>
        where TProv : IProvider<TProd>
        where TProd : IProviderProduct
    {
        IEnumerable<string> FoundProvidersNames
        { get; }

        TProv GetProvider(string name);
    }
}