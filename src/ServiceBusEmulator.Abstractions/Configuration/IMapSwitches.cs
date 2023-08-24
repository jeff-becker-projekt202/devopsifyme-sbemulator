namespace ServiceBusEmulator.Abstractions.Configuration;

public interface IMapSwitches
{
    bool CanHandle(string arg);
    string Transform(string arg);
}
