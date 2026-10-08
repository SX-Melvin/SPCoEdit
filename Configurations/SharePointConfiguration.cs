namespace SPCoEdit.Configurations
{
    public class SharePointConfiguration
    {
        public string Mode { get; set; } = "OnPrem";
        public string SiteUrl { get; set; } = "";
        public string WebUrl { get; set; } = "";
        public string LibraryTitle { get; set; } = "Documents";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string TenantId { get; set; } = "";
        public string ClientId { get; set; } = "";
        public string CertificatePath { get; set; } = "";
        public string CertificatePassword { get; set; } = "";
        public string CertificateKeyStorage { get; set; } = "Ephemeral";

        public bool IsOnline => Mode.Equals("Online", StringComparison.OrdinalIgnoreCase);
    }
}
