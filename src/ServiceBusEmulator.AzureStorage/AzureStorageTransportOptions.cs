namespace ServiceBusEmulator.AzureStorage;
public class MemoryTransportOptions
{
    public string ConnectionString { get; set; } = "UseDevelopmentStorage=true";
    public List<string> Topics { get; set; } = new List<string>();
    public List<string> Queues { get; set; } = new List<string>();
    public List<string> Subscriptions { get; set; } = new List<string>();
}
