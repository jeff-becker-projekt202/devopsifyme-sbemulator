using Amqp.Types;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace ServiceBusEmulator.Security;
public class TransientCertificateFactory : CertificateFactory
{
    private readonly string _distingushedName;
    private readonly List<string> _alternativeNames;

    public TransientCertificateFactory(string distingushedName, List<string> alternativeNames)
    {
        _distingushedName = distingushedName;
        _alternativeNames = alternativeNames;
    }

    private static byte[] EncodeAlternativeNames(List<string> names)
    {
        var writer = new AsnWriter(AsnEncodingRules.DER);
        using (var scope = writer.PushSequence())
        {
            foreach (var name in names)
            {
                writer.WriteCharacterString(UniversalTagNumber.IA5String, name, new Asn1Tag(TagClass.ContextSpecific, 2));
            }
        }
        return writer.Encode();
    }
    /// <summary>
    /// Registers (and generates if necessary) a user-specific development encryption certificate.
    /// </summary>
    /// <param name="subject">The subject name associated with the certificate.</param>
    /// <returns>The <see cref="OpenIddictServerBuilder"/> instance.</returns>
    public static X509Certificate2 CreateDevelopmentCertificate(string distingushedName, List<string> alternativeNames)
    {
        var subject = new X500DistinguishedName(distingushedName);

        //using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        //using X509Store store = new(StoreName.Root, StoreLocation.CurrentUser);
        //store.Open(OpenFlags.ReadOnly);

        // Try to retrieve the existing development certificates from the specified store.
        // If no valid existing certificate was found, create a new encryption certificate.
        //var certificate = store.Certificates.Find(X509FindType.FindBySubjectDistinguishedName, subject.Name, validOnly: false)
        //    .OfType<X509Certificate2>()
        //    .FirstOrDefault(static cc => cc.NotBefore < DateTime.Now && cc.NotAfter > DateTime.Now);

        using var algorithm = RSA.Create(2048); 

        var request = new CertificateRequest(subject, algorithm, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.CrlSign, critical: true));
        request.CertificateExtensions.Add(new X509SubjectAlternativeNameExtension(EncodeAlternativeNames(alternativeNames)));

        var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(2));

        // Note: setting the friendly name is not supported on Unix machines (including Linux and macOS).
        // To ensure an exception is not thrown by the property setter, an OS runtime check is used here.
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            certificate.FriendlyName = "ServiceBusEmulator Certificate";
        }

        //// Note: CertificateRequest.CreateSelfSigned() doesn't mark the key set associated with the certificate
        //// as "persisted", which eventually prevents X509Store.Add() from correctly storing the private key.
        //// To work around this issue, the certificate payload is manually exported and imported back
        //// into a new X509Certificate2 instance specifying the X509KeyStorageFlags.PersistKeySet flag.
        //var data = certificate.Export(X509ContentType.Pfx, string.Empty);

        //try
        //{
        //    var flags = X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable;
        //    certificate = new X509Certificate2(data, string.Empty, flags);
        //}

        //finally
        //{
        //    Array.Clear(data, 0, data.Length);
        //}
        return certificate;
    }

    protected override X509Certificate2 LoadCertificate() => CreateDevelopmentCertificate(_distingushedName, _alternativeNames);
}
