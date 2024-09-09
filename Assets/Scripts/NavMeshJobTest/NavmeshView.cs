using System;
using UnityEngine;

internal class NavmeshView
{
    public NavmeshView NavmeshSO { get; internal set; }
    public NavmeshView Navmesh { get; internal set; }
    public Vector3[] CenterPoints { get; internal set; }
    public int[] TriangleConnections { get; internal set; }
    public object[] TriangleConnectionsDistances { get; internal set; }

    internal int GetClosestPointOnNavmesh_Partitioning(Ray ray, float maxRayDistance, ref Vector3 navmeshStartPos)
    {
        throw new NotImplementedException();
    }
}
