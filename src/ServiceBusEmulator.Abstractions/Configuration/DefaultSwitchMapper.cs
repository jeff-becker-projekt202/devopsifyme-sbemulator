namespace ServiceBusEmulator.Abstractions.Configuration;

public class DefaultSwitchMapper : IMapSwitches
{
    public bool CanHandle(string arg) => false;

    public string Transform(string arg) => arg;
}