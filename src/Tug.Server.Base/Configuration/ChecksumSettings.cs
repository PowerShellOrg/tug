namespace Tug.Server.Configuration
{
    public class ChecksumSettings
    {
        public ExtSettings Ext
        { get; set; }
        
        public string Default
        { get; set; } = "SHA-256";
    }
}