namespace ServiceBusEmulator.Abstractions.Configuration;

//TODO: Maybe consider pulling the configuration provider out into a separate library?
public interface IMapSwitches
{
    bool CanHandle(string arg);
    string Transform(string arg);
}
