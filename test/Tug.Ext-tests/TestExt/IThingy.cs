// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

namespace Tug.TestExt
{
    public interface IThingy : Tug.Ext.IProviderProduct
    {
        void SetThing(string value);

        string GetThing();
    }
}