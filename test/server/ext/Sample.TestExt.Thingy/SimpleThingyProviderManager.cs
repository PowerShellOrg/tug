/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licensed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System.Collections.Generic;

namespace Sample.TestExt.Thingy
{
    public class SimpleThingyProviderManager : TugDSC.Ext.IProviderManager<IThingyProvider, IThingy>
    {
        private IThingyProvider _provider = new Impl.BasicThingyProvider();

        public IEnumerable<string> FoundProvidersNames
        {
            get
            {
                yield return "basic";
            }
        }

        public IThingyProvider GetProvider(string name)
        {
            if (name == "basic")
                return _provider;
            
            return null;
        }
    }
}