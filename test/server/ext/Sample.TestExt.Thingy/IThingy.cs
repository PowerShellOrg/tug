// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

namespace Sample.TestExt.Thingy
{
    public interface IThingy : TugDSC.Ext.IProviderProduct
    {
        void SetThing(string value);

        string GetThing();
    }
}