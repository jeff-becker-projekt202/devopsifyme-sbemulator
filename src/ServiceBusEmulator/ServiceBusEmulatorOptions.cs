using Amqp.Types;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ServiceBusEmulator
{
    /// <summary>
    /// Service bus simulator settings.
    /// </summary>
    public class ServiceBusEmulatorOptions
    {

        public string HostName { get; set; } = "localhost";
        /// <summary>
        /// Gets the preferred service bus port.
        /// </summary>
        public int Port { get; set; } = 5671;
        public ServerCertificateOptions ServerCertificate { get; set; } = new ServerCertificateOptions();

    }
    public class ServerCertificateOptions
    {
        // installing the server cert will show the "Do you want to trust this cert" prompt on windows
        // we default to "true" here so that those devs can just run the emulator with fewer install 
        // steps
        public bool AutoInstall { get; set; } = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        public string Thumbprint { get; set; } = null;
        public string Path { get; set; } = null;
        public string Password { get; set; } = null;
        public string Value { get; set; } = null;
        public List<string> AlternativeNames { get; set; } = new();
        public string DistinguishedName { get; set; } = "CN=devopsifyme-local.servicebus.windows.net,O=server";
    }
}
