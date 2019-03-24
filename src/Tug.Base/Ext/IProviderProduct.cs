// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

using System;

namespace Tug.Ext
{
    // Alternative Names:
    // public interface IProviderYield
    // public interface IProviderResult
    // public interface IProviderOutput
    public interface IProviderProduct : IDisposable
    {
        bool IsDisposed
        { get; }
    }
}