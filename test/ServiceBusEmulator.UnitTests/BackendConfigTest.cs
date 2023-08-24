using Microsoft.Extensions.Configuration;
using ServiceBusEmulator.Abstractions.Configuration;
using ServiceBusEmulator.Host;
using ServiceBusEmulator.Memory;
using ServiceBusEmulator.RabbitMq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBusEmulator.UnitTests;
public class BackendConfigTests
{

    [Fact]
    public static void CanMapMemoryConfig()
    {
        ExecStatic(
            new[] {
                "--memory-queue","a",
                "--memory-topic","topic",
                "--memory-subscription","topic/subscriber1",
                "--memory-subscription","topic/subscriber2",
            },
            new MemoryBackend().SwitchMappings,
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Emulator:Memory:Queues:0","a" },
                { "Emulator:Memory:Topics:0","topic"},
                { "Emulator:Memory:Subscriptions:0","topic/subscriber1"},
                { "Emulator:Memory:Subscriptions:1","topic/subscriber2"},
            });
    }




    [Fact]
    public static void CanMapRabbitConfig()
    {
        ExecStatic(
            new[] {
                "--rabbitmq-host","localhost",
                "--rabbitmq-port","12345",
                "--rabbitmq-user","user",
                "--rabbitmq-password","pwd",
                "--rabbitmq-channel","abcd",
                "--rabbitmq-channel","def",
            },
            new RabbitMqBackend().SwitchMappings,
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Emulator:RabbitMq:Host","localhost" },
                { "Emulator:RabbitMq:Port","12345"},
                { "Emulator:RabbitMq:Username","user"},
                { "Emulator:RabbitMq:Password","pwd"},
                { "Emulator:RabbitMq:Channels:0","abcd"},
                { "Emulator:RabbitMq:Channels:1","def"},
            });
    }

    [Fact]
    public static void CanMapRootConfig()
    {
        ExecStatic(
            new[] {
                "--emulator-host","localhost",
                "--emulator-port","12345",
                "--cert-auto-install","true",
                "--cert-thumbprint","true",
                "--cert-path","/foo/bar/baz",
                "--cert-password=",
                "--cert-value","awehifaiwuefiuhaewifuaiuoef",
                "--cert-dn","aiuwehfuiafwe",
                "--cert-alt","abcd",
                "--cert-alt","def",
            },
            new RootBackend().SwitchMappings,
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Emulator:HostName","localhost" },
                { "Emulator:Port","12345"},
                { "Emulator:ServerCertificate:AutoInstall","true"},
                { "Emulator:ServerCertificate:Thumbprint","true"},
                { "Emulator:ServerCertificate:Path","/foo/bar/baz"},
                { "Emulator:ServerCertificate:Password",""},
                { "Emulator:ServerCertificate:Value","awehifaiwuefiuhaewifuaiuoef"},
                { "Emulator:ServerCertificate:DistinguishedName","aiuwehfuiafwe"},
                { "Emulator:ServerCertificate:AlternativeNames:0","abcd"},
                { "Emulator:ServerCertificate:AlternativeNames:1","def"},
            });
    }


    private static void ExecStatic(string[] args, IMapSwitches switches, Dictionary<string, string> expected)
    {

        var cfgBuilder = new ConfigurationBuilder();
        cfgBuilder.AddEmulatorHostCommandline(args, switches);
        var cfg = cfgBuilder.Build();   
        foreach(var (key, value) in expected)
        {
            Assert.Equal(value, cfg.GetSection(key).Value);
        }
    }
}
