using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class NavMeshRebuild : MonoBehaviour
{
    public void Reset()
    {
        NavMeshSurface MeshSurface = GetComponent<NavMeshSurface>();

        EntityEngine[] entity = GameObject.FindObjectsOfType<EntityEngine>();

        foreach (EntityEngine plant in entity)
            if(plant is PlantEngine || plant.Stationary)
            foreach (MeshCollider transform in plant.GetComponentsInChildren<MeshCollider>())
            {
                    transform.gameObject.layer = LayerMask.NameToLayer("Terrain");
            }

        MeshSurface.BuildNavMesh();

        foreach (EntityEngine plant in entity)
            foreach (MeshCollider transform in plant.GetComponentsInChildren<MeshCollider>())
                transform.gameObject.layer = (int)Mathf.Log(plant.Layer.value, 2) ;
    }
}
