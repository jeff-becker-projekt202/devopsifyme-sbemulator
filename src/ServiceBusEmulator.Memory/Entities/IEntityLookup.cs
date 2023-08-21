﻿namespace ServiceBusEmulator.Memory.Entities
{
    public interface IEntityLookup : IEnumerable<(string Address, IEntity Entity)>
    {
        IEntity Find(string name);
    }
}
