namespace tug.Messages
{
    public static class DscContentTypes
    {
        public const string OCTET_STREAM = "application/octet-stream";
        public const string JSON = "application/json";
    }

    public enum DscTrueFalse
    {
        True,
        False,
    }

    public enum DscRefreshMode
    {
        Push,
        Pull,
    }

    public enum DscActionStatus
    {
        OK,
        RETRY,
        GetConfiguration,
        UpdateMetaConfiguration,
    }
}