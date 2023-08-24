using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServiceBusEmulator.Abstractions.Options;
using ServiceBusEmulator.Abstractions.Security;
using ServiceBusEmulator.Security;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text;
using Amqp.Listener;
using ServiceBusEmulator.Azure;
using Amqp.Handler;

namespace ServiceBusEmulator
{
    public static class Extensions
    {

        public static string ExportFullPem(this X509Certificate2 cert)
        {
            byte[] certificateBytes = cert.RawData;
            var key = ((AsymmetricAlgorithm?)cert.GetRSAPrivateKey() ?? (AsymmetricAlgorithm?)cert.GetECDsaPrivateKey());
            byte[] pubKeyBytes = key!.ExportSubjectPublicKeyInfo();
            byte[] privKeyBytes = key.ExportPkcs8PrivateKey();

            var sb = new StringBuilder();
            sb.Append(cert.ExportCertificatePem())
                .Append("\n")
                .Append(PemEncoding.Write("PUBLIC KEY", pubKeyBytes))
                .Append("\n")
                .Append(PemEncoding.Write("PRIVATE KEY", privKeyBytes));
            return sb.ToString();
        }
    }

}
