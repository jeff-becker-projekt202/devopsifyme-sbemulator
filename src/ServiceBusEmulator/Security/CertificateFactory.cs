using Microsoft.Extensions.Configuration;
using ServiceBusEmulator.Abstractions.Security;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace ServiceBusEmulator.Security;

public abstract class CertificateFactory : IServerCertificateFactory
{
    private readonly Lazy<X509Certificate2> _serverCert;
    protected CertificateFactory()
    {
        _serverCert = new Lazy<X509Certificate2>(() => LoadCertificateEnsurePrivateKey());
    }

    private X509Certificate2 LoadCertificateEnsurePrivateKey()
    {
        var cert = LoadCertificate();
        if (!cert.HasPrivateKey)
        {
            throw new InvalidOperationException($"The certificate {cert.Thumbprint} is missing a private key which is required for the server to work.");
        }
        return cert;
    }
    protected abstract X509Certificate2 LoadCertificate();

    public X509Certificate2 Load() => _serverCert.Value;

    public static IServerCertificateFactory FromConfig(IConfiguration cfg)
    {
        var thumbprint = cfg.GetSection("Emulator:ServerCertificateThumbprint")?.Value;
        var (path, password) = (cfg.GetSection("Emulator:ServerCertificatePath")?.Value, cfg.GetSection("Emulator:ServerCertificatePassword")?.Value);
        var certificateBinary = cfg.GetSection("Emulator:ServerCertificateValue")?.Value;
        if (!string.IsNullOrEmpty(thumbprint))
        {
            return new LocalStoreCertificateFactory(thumbprint);
        }
        else if (!string.IsNullOrEmpty(path) && !string.IsNullOrEmpty(password))
        {
            return new FileCertificateFactory(path, password);
        }
        else if (!string.IsNullOrEmpty(certificateBinary))
        {
            return new LiteralCertificateFactory(certificateBinary);
        }
        else
        {
            var altNamesConfig = cfg.GetSection("Emulator:ServerCertificate:AlternativeNames");
            var distingushedName = cfg.GetSection("Emulator:ServerCertificate:DistinguishedName")?.Value ?? $"CN=devopsifyme-local.servicebus.windows.net,O=server";
            var altNames = altNamesConfig.GetChildren().Select(c => c.Value).ToList();
            if (!altNames.Any())
            {
                altNames.Add("sbemulator");
                altNames.Add("emulator");
                altNames.Add("localhost");

            }
            if (!altNames.Contains("localhost"))
            {
                altNames.Add("localhost");
            }
            return new TransientCertificateFactory(distingushedName, altNames);
        }
    }
}
