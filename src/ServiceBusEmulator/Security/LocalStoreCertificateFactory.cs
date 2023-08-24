using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace ServiceBusEmulator.Security;

public class LocalStoreCertificateFactory : CertificateFactory
{
    private readonly string _thumbprint;

    public LocalStoreCertificateFactory(string thumbprint, bool autoInstall) : base(autoInstall)
    {
        _thumbprint = thumbprint;
    }

    protected override X509Certificate2 LoadCertificate()
    {
        StoreLocation[] locations = new StoreLocation[] { StoreLocation.LocalMachine, StoreLocation.CurrentUser };
        X509FindType[] findTypes = new[] { X509FindType.FindBySubjectName, X509FindType.FindByThumbprint, X509FindType.FindBySubjectDistinguishedName };
        foreach (StoreLocation location in locations)
        {
            using (X509Store store = new X509Store(StoreName.My, location))
            {
                store.Open(OpenFlags.OpenExistingOnly);
                foreach (X509FindType findType in findTypes)
                {
                    var result = store.Certificates.Find(findType,  _thumbprint,  false)
                             .OfType<X509Certificate2>()
                             .FirstOrDefault(static cc => cc.NotBefore < DateTime.Now && cc.NotAfter > DateTime.Now);
                    if(result != null) { return result; }   
                }
                store.Close();
            }
        }

        throw new ArgumentException("No certificate can be found using the find value " + _thumbprint);
    }
}