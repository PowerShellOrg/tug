/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

namespace Tug.TestExt
{
    public interface IThingy : Tug.Ext.IProviderProduct
    {
        void SetThing(string value);

        string GetThing();
    }
}