using Amqp;
using Microsoft.Extensions.Configuration;
using ServiceBusEmulator;
using ServiceBusEmulator.Abstractions;
using ServiceBusEmulator.Abstractions.Configuration;
using ServiceBusEmulator.Host;
using ServiceBusEmulator.Memory;
using ServiceBusEmulator.RabbitMq;

Trace.TraceLevel = TraceLevel.Frame;
Trace.TraceListener = (l, f, a) => Console.WriteLine(DateTime.Now.ToString("[hh:mm:ss.fff]") + " " + string.Format(f, a));

var backends = new IBackend[] { new RootBackend(),  new RabbitMqBackend(), new MemoryBackend() };
var builder = new AppBuilderWrapper(WebApplication.CreateBuilder());
builder.ConfigurationBuilder.Add(new EmulatorHostCommandLineConfigurationSource(args, new AggregateSwitchMapper(backends.Select(b => b.SwitchMapper)))); 
var root = (IConfigurationRoot)builder.Configuration;
var debugView = root.GetDebugView();

foreach(var backend in backends)
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
