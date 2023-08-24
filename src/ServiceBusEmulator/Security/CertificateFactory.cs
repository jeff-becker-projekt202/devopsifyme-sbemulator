using Microsoft.Extensions.Configuration;
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

    public static IServerCertificateFactory FromConfig(IConfiguration cfg)
    {
        var thumbprint = cfg.GetSection("Emulator:ServerCertificate:Thumbprint")?.Value;
        var (path, password) = (cfg.GetSection("Emulator:ServerCertificate:Path")?.Value, cfg.GetSection("Emulator:ServerCertificate:Password")?.Value);
        var certificateBinary = cfg.GetSection("Emulator:ServerCertificate:Value")?.Value;
        bool autoInstall = GetInstallServerCert(cfg); ;
        if (!string.IsNullOrEmpty(thumbprint))
        {
            return new LocalStoreCertificateFactory(thumbprint, autoInstall);
        }
        else if (!string.IsNullOrEmpty(path) && password != null /*having an empty string as the password is a valid case*/)
        {
            return new FileCertificateFactory(path, password, autoInstall);
        }
        else if (!string.IsNullOrEmpty(certificateBinary))
        {
            return new LiteralCertificateFactory(certificateBinary, autoInstall);
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
            return new TransientCertificateFactory(distingushedName, altNames, autoInstall);
        }
    }

    private static bool GetInstallServerCert(IConfiguration cfg)
    {
        var cfgValue = cfg.GetSection("Emulator:ServerCertificate:AutoInstall")?.Value?.ToLowerInvariant();
        if (!string.IsNullOrEmpty(cfgValue))
        {
            return cfgValue == "true" || cfgValue == "1" || cfgValue == "yes";
        }
        // installing the server cert will show the "Do you want to trust this cert" prompt on windows
        // we default to "true" here so that those devs can just run the emulator with fewer install 
        // steps
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows); 
    }
}
