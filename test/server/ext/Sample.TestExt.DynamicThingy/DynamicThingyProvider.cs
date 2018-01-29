/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licensed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System;
using System.Collections.Generic;
using Sample.TestExt.Thingy;
using TugDSC.Ext;

namespace Sample.TestExt.DynamicThingy.Impl
{
    public class DynamicThingyProvider : IThingyProvider
    {
        private Func<string, string> _func;

        private static readonly ProviderInfo INFO = new ProviderInfo("dynaThingy");
        private static readonly IEnumerable<ProviderParameterInfo> PARAMS = new[]
        {
            new ProviderParameterInfo(nameof(DynamicThingy.Func),
                    label: "Thingy Func",
                    description: "A Func that will be applied to derive a new thingy",
                    isRequired: true),
        };

        public ProviderInfo Describe() => INFO;
        public IEnumerable<ProviderParameterInfo> DescribeParameters() => PARAMS;

        public void SetParameters(IDictionary<string, object> productParams)
        {
            if (productParams.ContainsKey(nameof(DynamicThingy.Func)))
                throw new KeyNotFoundException("missing required parameter 'Func'");

            _func = (Func<string, string>)productParams[nameof(DynamicThingy.Func)];
        }

        public IThingy Produce()
        {
            if (_func == null)
                throw new InvalidOperationException("parameters have not been set");

            return new DynamicThingy
            {
                Func = _func,
            };
        }
    }
}