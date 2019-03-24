// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

namespace Tug.Server.FaaS.AwsLambda.Configuration
{
    public class PullServiceSettings
    {
        public const string DisabledSettingValue = "#OFF";

        public const int DefaultAuthzRegKeysRefreshMins = 15;

        public string S3Bucket
        { get; set; } //= "dsc-faas-work";
        public string S3KeyAuthzRegKeys
        { get; set; } //= "dsc-service/authz-reg-keys
        public string S3KeyPrefixAuthzRegistrations
        { get; set; } //= "dsc-service/authz
        public string S3KeyPrefixRegistrations
        { get; set; } //= "dsc-service/registrations";
        public string S3KeyPrefixConfigurations
        { get; set; } //= "dsc-service/configurations";
        public string S3KeyPrefixModules
        { get; set; } //= "dsc-service/modules";

        public int AuthzRegKeysRefreshMins
        { get; set; } = DefaultAuthzRegKeysRefreshMins;
    }
}