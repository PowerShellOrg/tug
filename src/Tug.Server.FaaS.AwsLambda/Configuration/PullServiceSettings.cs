/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

namespace Tug.Server.FaaS.AwsLambda.Configuration
{
    public class PullServiceSettings
    {
        public string S3BucketName
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
    }
}