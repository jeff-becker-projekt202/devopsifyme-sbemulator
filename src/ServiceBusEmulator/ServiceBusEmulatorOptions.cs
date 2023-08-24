namespace ServiceBusEmulator
{
    /// <summary>
    /// Service bus simulator settings.
    /// </summary>
    public class ServiceBusEmulatorOptions
    {
        ///// <summary>
        ///// Returns the <see cref="X509Certificate2"/> used to setup secure links, or null if none set.
        ///// </summary>
        //public X509Certificate2 ServerCertificate { get; set; } = null!;

        ///// <summary>
        ///// Loads the <see cref="ServerCertificate"/> from user certificate store.
        ///// </summary>
        //public string? ServerCertificateThumbprint { get; set; }

        ///// <summary>
        ///// Loads the <see cref="ServerCertificate"/> from disk.
        ///// </summary>
        //public string? ServerCertificatePath { get; set; }

        ///// <summary>
        ///// Password for the <see cref="ServerCertificatePath"/>
        ///// </summary>
        //public string? ServerCertificatePassword { get; set; }

        public string HostName { get; set; } = "localhost";
        /// <summary>
        /// Gets the preferred service bus port.
        /// </summary>
        public int Port { get; set; } = 5671;


    }
}
