namespace MindDesign.EzMail
{
	public class EzMailConfig
	{
		public const string EzMail = "EzMail";
		public SmtpParameters SmtpParameters { get; set; } = new SmtpParameters();
		public DebugData DebugData { get; set; } = new DebugData();
	}

	public class SmtpParameters
	{
		public string Host { get; set; } = string.Empty;
		public int Port { get; set; } = 0;
		public string Username { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
		public bool EnableSSL { get; set; } = true;
	}

	public class DebugData
	{
		public bool Active { get; set; } = true;
		public string Email { get; set; } = string.Empty;
	}
}
