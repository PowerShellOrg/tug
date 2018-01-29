/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licensed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

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