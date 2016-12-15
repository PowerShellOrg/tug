using System.Collections.Generic;
using Tug.Ext;

namespace Tug.TestExt.Impl
{
    public class BasicThingyProvider : IThingyProvider
    {
        // IMPL NOTE:  The implementation of this provider is not the most
        // efficient, as it take advantage of any opportunities to cache
        // any generated values or do any paramater validations, etc.
        // However, it satisfies the minimum necessary requirements to
        // support the test cases

        private IDictionary<string, object> _productParams;

        public ProviderInfo Describe()
        {
            return new ProviderInfo("basic",
                    label: "Basic Thingy",
                    description: "A thingy with support for basic things.");
        }

        public IEnumerable<ProviderParameterInfo> DescribeParameters()
        {
            return new[]
            {
                new ProviderParameterInfo(nameof(BasicThingy.Prefix),
                        label: "Thingy Prefix",
                        description: "Put something before the thingy"),

                new ProviderParameterInfo(nameof(BasicThingy.Suffix),
                        label: "Thingy Suffix",
                        description: "Put something after the thingy"),
            };
        }

        public void SetParameters(IDictionary<string, object> productParams)
        {
            _productParams = productParams;
        }

        public IThingy Produce()
        {
            return new BasicThingy
            {
                Prefix = (_productParams?.ContainsKey(nameof(BasicThingy.Prefix))).GetValueOrDefault()
                        ? $"{_productParams[nameof(BasicThingy.Prefix)]}"
                        : null,
                
                Suffix = (_productParams?.ContainsKey(nameof(BasicThingy.Suffix))).GetValueOrDefault()
                        ? $"{_productParams[nameof(BasicThingy.Suffix)]}"
                        : null,
                
            };
        }
    }
}