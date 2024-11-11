using System;
using UnityEngine;

public interface IPropertysRendererUpdate
{
    Transform Transform { get; }

    InputJobPropertyData inputJobProperty { get; }

    void SetPropertiesJob(ResultsJobProperty results);
}
