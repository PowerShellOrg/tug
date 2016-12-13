using System.Collections.Generic;

namespace Tug.Ext
{
    public interface IExtProviderX<TE>
        where TE: IExtension
    {
        ExtInfo Describe();

        IEnumerable<ExtParameterInfo> DescribeParameters();

        TE Provide(IDictionary<string, object> initParams);
    }
}