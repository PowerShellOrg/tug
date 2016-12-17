/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System;
using System.Collections.Generic;
using Tug.Ext;

namespace Tug.TestExt.Impl
{
    public class FuncThingyProvider : IThingyProvider
    {
        private Func<string, string> _func;

        private static readonly ProviderInfo INFO = new ProviderInfo("func");
        private static readonly IEnumerable<ProviderParameterInfo> PARAMS = new[]
        {
            new ProviderParameterInfo(nameof(FuncThingy.Func),
                    label: "Thingy Func",
                    description: "A Func that will be applied to derive a new thingy",
                    isRequired: true),
        };

        public ProviderInfo Describe() => INFO;
        public IEnumerable<ProviderParameterInfo> DescribeParameters() => PARAMS;

        public void SetParameters(IDictionary<string, object> productParams)
        {
            if (productParams.ContainsKey(nameof(FuncThingy.Func)))
                throw new KeyNotFoundException("missing required parameter 'Func'");

            _func = (Func<string, string>)productParams[nameof(FuncThingy.Func)];
        }

        public IThingy Produce()
        {
            if (_func == null)
                throw new InvalidOperationException("parameters have not been set");

            return new FuncThingy
            {
                Func = _func,
            };
        }
    }
}