﻿using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace ServiceBusEmulator.RabbitMq.Links
{
    public class RabbitMqInitializer : IRabbitMqInitializer
    {
        private readonly RabbitMqOptions _options;
        private readonly IRabbitMqUtilities _utilities;
        private bool _initialized;

        public RabbitMqInitializer(IRabbitMqUtilities utilities, IOptions<RabbitMqOptions> options)
        {
            _options = options.Value;
            _utilities = utilities;
        }

        public void Initialize(IModel channel)
        {
            if (_initialized)
            {
                return;
            }

            var entities = _options.Channels?.SelectMany(c=>c.Split(',', ';')).Where(c=>!String.IsNullOrEmpty(c)) ?? Enumerable.Empty<string>();
            foreach (string entity in entities)
            {
                _utilities.EnsureExists(channel, entity);
            }

            _initialized = true;
        }
    }
}
