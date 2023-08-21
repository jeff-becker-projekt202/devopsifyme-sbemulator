using ServiceBusEmulator.Memory.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBusEmulator.Memory;
public class MemoryTransportOptions
{
    public List<Topic> Topics { get; set; } = new List<Topic>();
    public List<Queue> Queues { get; set; } = new List<Queue> ();
    public List<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
