/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

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