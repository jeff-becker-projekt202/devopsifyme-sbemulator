using ServiceBusEmulator.Abstractions.Security;
using System;
using System.Linq;
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
            //When running on windows this will automaticall install the generated cert with a prompt to the user about if they want to trust it
            //Think what happens with visual studio when you're first setting up a dotnet core web project with SSL
            using var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);
            var existing = store.Certificates.Find(X509FindType.FindByThumbprint, cert.Thumbprint, false)
                .OfType<X509Certificate2>();
            if(existing == null)
            {
                store.Add(cert);
            }            
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
            //Todo Consider writing this out to a temp directory and reloading instead of recreating every time
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
            return new AutoGenerateCert(cfg.DistinguishedName, cfg.AlternativeNames, cfg.AutoInstall);
        }
    }

}
