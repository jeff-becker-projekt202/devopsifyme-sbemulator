using System.Security.Cryptography.X509Certificates;

namespace ServiceBusEmulator.Abstractions.Security;
public interface IServerCertificateFactory
{
    X509Certificate2 Load();
}
