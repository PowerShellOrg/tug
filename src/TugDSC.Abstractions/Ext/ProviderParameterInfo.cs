// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

namespace TugDSC.Ext
{
    public class ProviderParameterInfo
    {
        public ProviderParameterInfo(string name,
            bool isRequired = false,
            string label = null,
            string description = null)
        {
            Name = name;

            IsRequired = false;
            Label = label;
            Description = description;
        }

        public string Name
        { get; private set; }

        public bool IsRequired
        { get; private set; }

        public string Label
        { get; private set; }

        public string Description
        { get; private set; }
    }
}