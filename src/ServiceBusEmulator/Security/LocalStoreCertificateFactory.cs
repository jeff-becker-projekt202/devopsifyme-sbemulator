using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace ServiceBusEmulator.Security;

public class LocalStoreCertificateFactory : CertificateFactory
{
    private readonly string _thumbprint;

    public LocalStoreCertificateFactory(string thumbprint)
    {
        _thumbprint = thumbprint;
    }

    protected override X509Certificate2 LoadCertificate()
    {
        using X509Store store = new(StoreName.Root, StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadOnly);
        // Try to retrieve the existing development certificates from the specified store.
        // If no valid existing certificate was found, create a new encryption certificate.
        return store.Certificates.Find(X509FindType.FindByThumbprint, _thumbprint, validOnly: false)
            .OfType<X509Certificate2>()
            .First(static cc => cc.NotBefore < DateTime.Now && cc.NotAfter > DateTime.Now);

    }
}