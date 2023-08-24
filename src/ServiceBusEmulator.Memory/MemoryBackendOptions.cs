namespace ServiceBusEmulator.Memory;
public class MemoryBackendOptions
{
    public List<string> Topics { get; set; } = new List<string>();
    public List<string> Queues { get; set; } = new List<string> ();
    public List<string> Subscriptions { get; set; } = new List<string>();
}
