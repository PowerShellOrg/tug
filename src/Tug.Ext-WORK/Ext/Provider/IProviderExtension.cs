using System;
using System.Collections.Generic;

namespace Tug.Ext.Provider
{
    public interface IProviderExtension<TProd> : IExtension
        where TProd : IProviderProduct
    {
        IEnumerable<ExtParameterInfo> DescribeParameters();

        void SetParameters(IDictionary<string, object> productParams);

        TProd Produce();
    }
}