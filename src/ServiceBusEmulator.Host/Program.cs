using Amqp;
using ServiceBusEmulator;
using ServiceBusEmulator.Abstractions.Security;
using ServiceBusEmulator.Host;
using ServiceBusEmulator.RabbitMq;

Trace.TraceLevel = TraceLevel.Frame;
Trace.TraceListener = (l, f, a) => Console.WriteLine(DateTime.Now.ToString("[hh:mm:ss.fff]") + " " + string.Format(f, a));


var builder = new AppBuilderWrapper(WebApplication.CreateBuilder(args));
builder.AddServiceBusEmulator();
switch (builder.Configuration.GetSection("Emulator:Backend").Value?.ToLowerInvariant())
{
    case "rabbitmq":
        builder.AddServiceBusEmulatorRabbitMqBackend();
        break;
    case "azurestorage":
        builder.AddAzureStorage();
        break;
}
//
builder.AddServiceBusEmulatorMemoryBackend();


WebApplication app = builder.Build();
app.UseHealthChecks("/health");
app.UseRouting();

app.Run();
