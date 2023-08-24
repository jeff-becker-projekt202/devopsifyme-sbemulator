using Amqp.Framing;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace ServiceBusEmulator.Security;
public class AutoGenerateCert : CertificateFactory
{
    private readonly string _distingushedName;
    private readonly List<string> _alternativeNames;

    public AutoGenerateCert(string distingushedName, List<string> alternativeNames, bool autoInstall) : base(autoInstall)
    {
        _distingushedName = distingushedName;
        _alternativeNames = alternativeNames;
    }
    private class CachedCertManager
    {
        private readonly string _password;
        private readonly string _cachePath;
        public CachedCertManager(string password)
        {
            _password = password;
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create);
            _cachePath = Path.Combine(appData, "ServiceBusEmulator", "server_cert.pfx");
        }
        public X509Certificate2? Load()
        {
            if (File.Exists(_cachePath))
            {                
                try
                {
                    var bytes = File.ReadAllBytes(_cachePath);
                    var flags = X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable;
                    return new X509Certificate2(bytes, _password, flags);             
                }
                catch(Exception ex) {
                    // fall through as that returns null;
                }
            }
            return null;
        }

        public void Save(X509Certificate2 certificate)
        {
            var data = certificate.Export(X509ContentType.Pfx, _password);
            EnsurePath(Path.GetDirectoryName(_cachePath));
            File.WriteAllBytes(_cachePath, data);
        }
        private void EnsurePath(string dir)
        {
            string parent = Path.GetDirectoryName(dir);
            if (!Directory.Exists(parent))
            {
                EnsurePath(parent);
            }
            Directory.CreateDirectory(dir);
        }
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

        // Note: CertificateRequest.CreateSelfSigned() doesn't mark the key set associated with the certificate
        // as "persisted", which eventually prevents X509Store.Add() from correctly storing the private key.
        // To work around this issue, the certificate payload is manually exported and imported back
        // into a new X509Certificate2 instance specifying the X509KeyStorageFlags.PersistKeySet flag.
        var data = certificate.Export(X509ContentType.Pfx, string.Empty);

        try
        {
            var flags = X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable;
            certificate = new X509Certificate2(data, string.Empty, flags);
        }

        finally
        {
            Array.Clear(data, 0, data.Length);
        }
        return certificate;
    }

    private static string GeneratePassword(string distingushedName, List<string> alternativeNames)
    {
        var txt = String.Join("|", alternativeNames.Concat(new[] { distingushedName }));
        var bytes = System.Text.Encoding.UTF8.GetBytes(txt);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToBase64String(hashBytes);
        return "";
    }


    protected override X509Certificate2 LoadCertificate()
    {
        var manager = new CachedCertManager(GeneratePassword(_distingushedName, _alternativeNames));
        X509Certificate2 certificate = manager.Load();
        if (certificate == null)
        {
            certificate = CreateDevelopmentCertificate(_distingushedName, _alternativeNames);
            manager.Save(certificate);
        }
        return certificate;

    }
}
