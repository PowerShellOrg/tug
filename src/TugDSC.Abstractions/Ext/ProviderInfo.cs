/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licensed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

namespace TugDSC.Ext
{
    public class ProviderInfo
    {
        public ProviderInfo(string name,
            string label = null, string description = null)
        {
            Name = name;
            Label = label;
            Description = description;
        }

        public string Name
        { get; private set; }

        public string Label
        { get; private set; }

        public string Description
        { get; private set; }
    }
}