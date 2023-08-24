using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using ServiceBusEmulator.Abstractions.Security;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace ServiceBusEmulator.Security;

public abstract class CertificateFactory : IServerCertificateFactory
{
    private readonly Lazy<X509Certificate2> _serverCert;
    private readonly bool _autoInstall;

    protected CertificateFactory(bool autoInstall)
    {
        _serverCert = new Lazy<X509Certificate2>(() => LoadCertificateEnsurePrivateKey());
        _autoInstall = autoInstall;
    }

    private X509Certificate2 LoadCertificateEnsurePrivateKey()
    {
        var cert = LoadCertificate();
        if (!cert.HasPrivateKey)
        {
            throw new InvalidOperationException($"The certificate {cert.Thumbprint} is missing a private key which is required for the server to work.");
        }
        if(_autoInstall)
        {
            using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Add(cert);
        }
        return cert;
    }
    protected abstract X509Certificate2 LoadCertificate();

    public X509Certificate2 Load() => _serverCert.Value;

    public static IServerCertificateFactory FromConfig(ServerCertificateOptions cfg)
    {
        
        if (!string.IsNullOrEmpty(cfg.Thumbprint))
        {
            return new LocalStoreCertificateFactory(cfg.Thumbprint, cfg.AutoInstall);
        }
        else if (!string.IsNullOrEmpty(cfg.Path) && cfg.Password != null /*having an empty string as the password is a valid case*/)
        {
            return new FileCertificateFactory(cfg.Path, cfg.Password, cfg.AutoInstall);
        }
        else if (!string.IsNullOrEmpty(cfg.Value))
        {
            return new LiteralCertificateFactory(cfg.Value, cfg.AutoInstall);
        }
        else
        {
            if (!cfg.AlternativeNames.Any())
            {
                cfg.AlternativeNames.Add("sbemulator");
                cfg.AlternativeNames.Add("emulator");
                cfg.AlternativeNames.Add("localhost");

            }
            if (!cfg.AlternativeNames.Contains("localhost"))
            {
                cfg.AlternativeNames.Add("localhost");
            }
            return new TransientCertificateFactory(cfg.DistinguishedName, cfg.AlternativeNames, cfg.AutoInstall);
        }
    }

}
