

using System;

namespace FactoryEntity
{
    internal interface IParametersEntityes
    {
        Type EngineComponent { get; }
        object SetParametrs(EntityEngine Prefab);
    }
}
