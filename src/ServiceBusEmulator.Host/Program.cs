using Amqp;
using ServiceBusEmulator.Host;


Trace.TraceLevel = TraceLevel.Frame;
Trace.TraceListener = (l, f, a) => Console.WriteLine(DateTime.Now.ToString("[hh:mm:ss.fff]") + " " + string.Format(f, a));


var builder = new AppBuilderWrapper(WebApplication.CreateBuilder());
builder.ConfigurationBuilder.AddBackendCommandLineSwitch(args);
var switchMapper = Backends.CreateSwitchMapper(builder.Configuration);
builder.ConfigurationBuilder.AddEmulatorHostCommandline(args, switchMapper);

var debugView = builder.DebugConfig;

foreach(var backend in Backends.All)
{
    if (backend.ShouldUse(builder.Configuration))
    {
        backend.ApplyConfiguration(builder);
    }
}

WebApplication app = builder.Build();
app.UseHealthChecks("/health");
app.UseRouting();
app.UseCertificateExportEndpoints();
app.Run();
