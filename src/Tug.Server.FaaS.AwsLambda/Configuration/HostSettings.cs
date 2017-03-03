namespace Tug.Server.FaaS.AwsLambda.Configuration
{
    /// <summary>
    /// Defines a minimal set of <i>bootstrap</i> settings that are loaded initially
    /// and define subsequent loading and resolution of runtime app settings.
    /// </summary>
    /// <remarks>
    /// In the Lambda environment the primary means of specifying configuration
    /// settings is through the use of <see
    /// cref="http://docs.aws.amazon.com/lambda/latest/dg/env_variables.html"
    /// >environment variables</see> but there are various limitations to this
    /// approach including maximum size of config data and naming conventions,
    /// and the mapping from a possibly complex and nested config structure to
    /// a set of flat key-value pairs, can be error-prone and cumbersome.
    /// 
    /// Instead we define an initial set of host settings that are used to
    /// bootstrap the environment and define where and how to pull the actual
    /// runtime <see cref="AppSettings>">application settings</see>.
    /// </remarks>
    public class HostSettings
    {

        public string AppSettingsS3Bucket
        { get; set; }

        public string AppSettingsS3Key
        { get; set; }
    }
}
