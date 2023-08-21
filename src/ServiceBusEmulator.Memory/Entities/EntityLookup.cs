using Microsoft.Extensions.Options;
using System.Collections;

namespace ServiceBusEmulator.Memory.Entities;

internal class EntityLookup : IEntityLookup
{
    private readonly Dictionary<string, IEntity> _entities;
    private class NamedEntity
    {
        public NamedEntity() { }
        public NamedEntity(IEntity entity) {
            Name = entity.Name;
            Entity = entity;
        }
        public NamedEntity(string name, IEntity entity)
        {
            Name = name;
            Entity = entity;
        }
        public string Name { get; set; }    
        public IEntity Entity { get; set; }
    }

    public EntityLookup(IOptions<MemoryTransportOptions> options)
    {
        MemoryTransportOptions o = options.Value;
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
            .Select(g => new {
                Topic = g.Key,
                Subscribers = g.Where(t => !String.IsNullOrEmpty(t.Subscriber)).Select(t => t.Subscriber).ToList()
            })
            .Select(t => new TopicEntity(t.Topic, t.Subscribers.Select(s => new QueueEntity(s)))).ToList();
            

        var queues = o.Queues.Select(q => new QueueEntity(q.Trim('/'))).ToList();

        _entities = topicsWithSubscriptions
            .SelectMany(t =>
                new[] { new NamedEntity(t) }
                .Concat(t.Subscriptions.Select(p => new NamedEntity(p.Key, (IEntity)p.Value)))
            )
            .Concat(queues.Select(q => new NamedEntity(q)))
            .ToDictionary(
                item => item.Name,
                item => item.Entity,
                StringComparer.OrdinalIgnoreCase
            );

        //MemoryTransportOptions o = options.Value;

        //var topics = o.Topics
        //    .Select(topic => new
        //    {
        //        topic.Name,
        //        Entity = (IEntity)topic
        //    });

        //var subscriptions = o.Topics
        //    .SelectMany(topic => topic
        //        .Subscriptions
        //        .Select(subscription => new
        //        {
        //            Name = $"{topic.Name}/Subscriptions/{subscription.Name}",
        //            Entity = (IEntity)subscription
        //        })
        //    );

        //var queues = o.Queues
        //    .Select(queue => new
        //    {
        //        Name = $"/{queue.Name}",
        //        Entity = (IEntity)new QueueEntity(queue.Name)
        //    });

        //_entities = topics
        //    .Concat(subscriptions)
        //    .Concat(queues)
        //    .ToDictionary(
        //        item => item.Name,
        //        item => item.Entity,
        //        StringComparer.OrdinalIgnoreCase
        //    );
    }

    public IEntity Find(string name)
    {
        return _entities.TryGetValue(name, out IEntity entity)
                    ? entity
                    : null;
    }

    public IEnumerator<(string Address, IEntity Entity)> GetEnumerator()
    {
        foreach (KeyValuePair<string, IEntity> item in _entities)
        {
            yield return (item.Key, item.Value);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
