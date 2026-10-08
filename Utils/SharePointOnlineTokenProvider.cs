using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using SPCoEdit.Configurations;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;

namespace SPCoEdit.Utils;

// Keep the MSAL application alive across requests so its token cache can be reused.
public sealed class SharePointOnlineTokenProvider : IDisposable
{
    private readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
    private readonly Lazy<(X509Certificate2 Certificate, IConfidentialClientApplication App, string Scope)> _authentication;

    public SharePointOnlineTokenProvider(IOptions<SharePointConfiguration> options,
        IConfiguration configuration, IHostEnvironment environment)
    {
        _authentication = new(() =>
        {
            var config = options.Value;
            if (string.IsNullOrWhiteSpace(config.TenantId) ||
                string.IsNullOrWhiteSpace(config.ClientId) ||
                string.IsNullOrWhiteSpace(config.CertificatePath))
                throw new InvalidOperationException("SharePoint Online requires TenantId, ClientId and CertificatePath.");

            if (!Uri.TryCreate(config.SiteUrl, UriKind.Absolute, out var site) || site.Scheme != Uri.UriSchemeHttps)
                throw new InvalidOperationException("SharePoint Online requires an HTTPS SiteUrl.");

            var certificatePath = Path.GetFullPath(config.CertificatePath, environment.ContentRootPath);
            var keyStorage = config.CertificateKeyStorage.ToUpperInvariant() switch
            {
                "EPHEMERAL" => X509KeyStorageFlags.EphemeralKeySet,
                "MACHINE" when OperatingSystem.IsWindows() => X509KeyStorageFlags.MachineKeySet,
                _ => throw new InvalidOperationException(
                    "SharePoint:CertificateKeyStorage must be Ephemeral, or Machine on Windows.")
            };
            var logger = NLog.LogManager.GetCurrentClassLogger();
            var passwordSource = "unknown";
            if (configuration is IConfigurationRoot root)
            {
                var provider = root.Providers.Reverse().FirstOrDefault(p =>
                    p.TryGet("SharePoint:CertificatePassword", out _));
                passwordSource = provider is Microsoft.Extensions.Configuration.Json.JsonConfigurationProvider json
                    ? $"JSON file: {json.Source.Path}"
                    : provider?.GetType().Name ?? "not configured";
            }
            logger.Info($"Loading SharePoint certificate: path={certificatePath}, contentRoot={environment.ContentRootPath}, " +
                $"environment={environment.EnvironmentName}, passwordConfigured={!string.IsNullOrEmpty(config.CertificatePassword)}, " +
                $"passwordSource={passwordSource}, runtime={System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");

            X509Certificate2 certificate;
            try
            {
                certificate = X509CertificateLoader.LoadPkcs12FromFile(
                    certificatePath, config.CertificatePassword, keyStorage);
            }
            catch (CryptographicException ex)
            {
                _logger.Error($"Error when reading SharePoint certificate: {ex.Message}, HRESULT=0x{ex.HResult:X8}, " +
                    $"keyStorage={keyStorage}, processId={Environment.ProcessId}, " +
                    $"runtime={System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
                throw new InvalidOperationException(
                    $"Cannot load SharePoint PFX '{certificatePath}'. Verify it is a valid PFX and that " +
                    $"SharePoint:CertificatePassword from {passwordSource} matches its export password. " +
                    $"Key storage={keyStorage}, HRESULT=0x{ex.HResult:X8}. " +
                    "If this PFX loads outside IIS, compare the runtime identity and key-storage mode. " +
                    "Restart the application after correcting the file or settings.", ex);
            }
            try
            {
                if (!certificate.HasPrivateKey)
                    throw new InvalidOperationException("The SharePoint Online certificate must contain its private key.");
                if (DateTime.UtcNow < certificate.NotBefore.ToUniversalTime() ||
                    DateTime.UtcNow >= certificate.NotAfter.ToUniversalTime())
                    throw new InvalidOperationException("The SharePoint Online certificate is not currently valid.");

                var app = ConfidentialClientApplicationBuilder.Create(config.ClientId)
                    .WithAuthority($"https://login.microsoftonline.com/{Uri.EscapeDataString(config.TenantId)}")
                    .WithCertificate(certificate)
                    .Build();
                return (certificate, app, site.GetLeftPart(UriPartial.Authority) + "/.default");
            }
            catch
            {
                certificate.Dispose();
                throw;
            }
        });
    }

    public string GetAccessToken()
    {
        var authentication = _authentication.Value;
        // MSAL returns cached tokens and refreshes them when necessary.
        return authentication.App.AcquireTokenForClient([authentication.Scope])
            .ExecuteAsync().GetAwaiter().GetResult().AccessToken;
    }

    public void Dispose()
    {
        if (_authentication.IsValueCreated)
            _authentication.Value.Certificate.Dispose();
    }
}
