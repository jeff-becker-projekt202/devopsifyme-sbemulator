using ServiceBusEmulator.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBusEmulator.IntegrationTests;
public class CertificateExportTests
{
    [Fact]
    public void CanExportPemFiles()
    {
        var certFactory = new TransientCertificateFactory($"CN=devopsifyme-local.servicebus.windows.net,O=server", new() { "localhost", "emulator", "sbemulator" });
        var cert = certFactory.Load();
        var str = cert.ExportFullPem();
        Assert.NotEmpty(str);
    }
}
