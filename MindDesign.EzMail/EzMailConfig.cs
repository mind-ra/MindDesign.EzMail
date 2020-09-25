namespace MindDesign.EzMail
{
    public class EzMailConfig
    {
        public const string EzMail = "EzMail";
        public SmtpParameters SmtpParameters { get; set; }
        public DebugData DebugData { get; set; }
    }

    public class SmtpParameters
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool EnableSSL { get; set; }
    }

    public class DebugData
    {
        public bool Active { get; set; }
        public string Email { get; set; }
    }
}
