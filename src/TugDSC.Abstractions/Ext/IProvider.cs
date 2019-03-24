// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace TugDSC.Ext
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