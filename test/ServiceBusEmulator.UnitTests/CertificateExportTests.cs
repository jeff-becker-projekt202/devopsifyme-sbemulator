using ServiceBusEmulator.Security;

namespace ServiceBusEmulator.UnitTests;
public class CertificateExportTests
{
    [Fact]
    public void CanExportPemFiles()
    {
        var certFactory = new TransientCertificateFactory($"CN=devopsifyme-local.servicebus.windows.net,O=server", new() { "localhost", "emulator", "sbemulator" }, false);
        var cert = certFactory.Load();
        var str = cert.ExportFullPem();
        Assert.NotEmpty(str);
    }
}
