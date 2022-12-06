using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class NavMeshRebuild : MonoBehaviour
{
    public void Start()
    {
        NavMeshSurface MeshSurface = GetComponent<NavMeshSurface>();

        PlantEngine[] plants = PlantEngine.GetPlants;

        foreach (PlantEngine plant in plants)
            foreach (Transform transform in plant.GetComponentsInChildren<Transform>())
            {
                transform.gameObject.layer = LayerMask.NameToLayer("Terrain");
            }

        MeshSurface.BuildNavMesh();

        foreach (PlantEngine plant in plants)
            foreach (Transform transform in plant.GetComponentsInChildren<Transform>())
                transform.gameObject.layer = LayerMask.NameToLayer("Entity");
    }
}
