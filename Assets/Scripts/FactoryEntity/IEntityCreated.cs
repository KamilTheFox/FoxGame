using UnityEngine;

    public interface IEntityCreated
    {
        TypeEntity TypeEntity { get; }
        bool isStatic { get; }
        EntityEngine GetEngine { get; }
        GameObject GetPrefab { get; }
    }
