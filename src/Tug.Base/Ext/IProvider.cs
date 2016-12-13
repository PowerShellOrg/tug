using System.Collections.Generic;

namespace Tug.Ext
{
    public interface IProvider<TProd>
        where TProd : IProviderProduct
    {
        ProviderInfo Describe();

        IEnumerable<ProviderParameterInfo> DescribeParameters();

        void SetParameters(IDictionary<string, object> productParams);

        TProd Produce();
    }
}