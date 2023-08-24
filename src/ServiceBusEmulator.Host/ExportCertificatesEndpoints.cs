using ServiceBusEmulator.Abstractions.Security;

using System.Text;

namespace ServiceBusEmulator.Host;

public static class ExportCertificatesEndpoints
{
    public static WebApplication UseCertificateExportEndpoints(this WebApplication app)
    {
        app.MapGet("/certificate.pfx", (HttpContext ctx) =>
        {
            var certFactory = app.Services.GetRequiredService<IServerCertificateFactory>();
            var cert = certFactory.Load();
            var bin = cert.Export(System.Security.Cryptography.X509Certificates.X509ContentType.Pfx, "");
            ctx.Response.Headers["Content-Type"] = "application/x-pkcs12";
            ctx.Response.Headers["Content-Disposition"] = $"attachment; filename=\"certificate.pfx\"";
            ctx.Response.Body.Write(bin);
        });

        app.MapGet("/", (HttpContext ctx) => {
            ctx.Response.Headers["Content-Type"] = "text/html";

            ctx.Response.Body.Write(
                Encoding.UTF8.GetBytes("<html><title>Service Bus Emulator</title><body><h1>Service Bus Emulator</h1> <a href=\"certificate.pfx\">Download Server Cert</a></body></html>")
            );
        });
        return app;
    }

}
