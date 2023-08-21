using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBusEmulator.Abstractions.Security;
public interface IServerCertificateFactory
{
    X509Certificate2 Load();
}
