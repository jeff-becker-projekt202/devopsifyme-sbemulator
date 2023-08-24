using System;
using System.Security.Cryptography.X509Certificates;

namespace ServiceBusEmulator.Security;

public class LiteralCertificateFactory : CertificateFactory
{
    private readonly string _dataInBase64;

    public LiteralCertificateFactory(string dataInBase64, bool autoInstall) : base(autoInstall)
    {
        _dataInBase64 = dataInBase64;
    }

    protected override X509Certificate2 LoadCertificate() =>new X509Certificate2(Convert.FromBase64String(_dataInBase64));
}
