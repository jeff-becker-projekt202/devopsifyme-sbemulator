using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServiceBusEmulator.Abstractions.Options;
using ServiceBusEmulator.Abstractions.Security;
using ServiceBusEmulator.Security;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text;

namespace ServiceBusEmulator
{
    public static class Extensions
    {
        public static IWebAppBuilder AddServiceBusEmulator(this IWebAppBuilder builder, Action<ServiceBusEmulatorOptions> configure = null)
        {
            configure ??= (o) => { };
            _ = builder.Services.AddTransient<ISecurityContext>(sp => SecurityContext.Default);
            _ = builder.Services.AddTransient<CbsRequestProcessor>();
            _ = builder.Services.AddTransient<ITokenValidator>(sp => CbsTokenValidator.Default);
            _ = builder.Services.AddOptions<ServiceBusEmulatorOptions>().Configure(configure).BindConfiguration("Emulator");
            _ = builder.Services.AddTransient<ServiceBusEmulatorHost>();
            _ = builder.Services.AddSingleton<IHostedService, ServiceBusEmulatorWorker>();
            _ = builder.Services.AddSingleton(ctx => CertificateFactory.FromConfig(ctx.GetRequiredService<IConfiguration>()));

            return builder;
        }
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
