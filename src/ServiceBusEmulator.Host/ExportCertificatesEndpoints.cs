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
        app.MapGet("/certificate.pem", (HttpContext ctx) =>
        {
            var certFactory = app.Services.GetRequiredService<IServerCertificateFactory>();
            var cert = certFactory.Load();
            var fullPem = cert.ExportFullPem();
            ctx.Response.Headers["Content-Type"] = "application/x-pem-file; charset=utf-8";
            ctx.Response.Headers["Content-Disposition"] = $"attachment; filename=\"certificate.pem\"";
            ctx.Response.Body.Write(Encoding.UTF8.GetBytes(fullPem));
        });
        return app;
    }

}
