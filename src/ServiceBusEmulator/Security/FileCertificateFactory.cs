using System.Security.Cryptography.X509Certificates;

namespace ServiceBusEmulator.Security;
public class FileCertificateFactory : CertificateFactory
{
    private readonly string _path;
    private readonly string _password;

    public FileCertificateFactory(string path, string password, bool autoInstall) : base(autoInstall)
    {
        _path = path;
        _password = password;
    }

    protected override X509Certificate2 LoadCertificate() => new X509Certificate2(_path, _password);
}
