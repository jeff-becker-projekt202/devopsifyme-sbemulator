using Amqp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace ServiceBusEmulator.Memory;
public class ChannelMap
{
    private readonly Dictionary<string, ConcurrentQueue<Message>> _queues = new Dictionary<string, ConcurrentQueue<Message>>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, ConcurrentQueue<Message>> _topicSubscriptions = new Dictionary<string, ConcurrentQueue<Message>>(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _topics = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private readonly ILogger<ChannelMap> _logger;

    public ChannelMap(ILogger<ChannelMap> logger, IOptions<MemoryBackendOptions> options)
    {
        _logger = logger;
        MemoryBackendOptions o = options.Value;
        var topicsWithSubscriptions = o.Subscriptions
            // force everything into the form 'topic/subscription'
            .Select(s => s.Trim('/').Replace("/Subscriptions/", "/", StringComparison.OrdinalIgnoreCase))
            //ensure that we've got a topic and subscription
            .Where(s => s?.Contains('/') ?? false)
            // Split
            .Select(s => s.Split('/', 2))
            .Select(x => new { Topic = x[0], Subscriber = x[1] })
            // Add records for the topics with empty subscribers
            .Concat(o.Topics.Select(t => new { Topic = t.Trim('/'), Subscriber = String.Empty }))
            .GroupBy(g => g.Topic)
            .Select(g => new
            {
                Name = g.Key,
                Subscribers = g.Where(t => !String.IsNullOrEmpty(t.Subscriber)).Select(t => t.Subscriber).ToList()
            });
        var allQueues = o.Queues
            .Select(x=>x.Trim('/'))
            .Distinct(StringComparer.OrdinalIgnoreCase)            
            .ToList();
        foreach(var queueName in allQueues)
        {
            _queues.Add(queueName, new ConcurrentQueue<Message>());
            _queues.Add(queueName+ "/$deadletterqueue", new ConcurrentQueue<Message>());
            _queues.Add(queueName + "/$management", new ConcurrentQueue<Message>());
        }
        foreach(var topic in topicsWithSubscriptions)
        {
            if (!_queues.ContainsKey(topic.Name))
            {
                _queues.Add(topic.Name, new ConcurrentQueue<Message>());
                _queues.Add(topic.Name + "/$deadletterqueue", new ConcurrentQueue<Message>());
                _queues.Add(topic.Name + "/$management", new ConcurrentQueue<Message>());
            }
            _topics.Add(topic.Name);
            foreach(var subscriber in topic.Subscribers)
            {
                _topicSubscriptions.Add($"{topic.Name}/Subscriptions/{subscriber}", new ConcurrentQueue<Message>());
            }
        }

    }
    public ConcurrentQueue<Message>? GetIncoming(string name) => _queues.GetValueOrDefault(name.Trim('/'));
    public ConcurrentQueue<Message>? GetOutgoing(string name)
    {
        name = name.Trim('/');
        if (_topics.Contains(name))
        {
            //todo throw an error here
            return null;
        }
        return _queues.GetValueOrDefault(name);
    }
}
