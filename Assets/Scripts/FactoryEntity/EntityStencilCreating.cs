using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FactoryEntity
{
    public abstract class EntityStencilCreating
    {
        protected readonly bool _isStaticItem;

        private static GameObject entityGroup;
        public static GameObject EntityGroup { get
            {
                if (entityGroup == null && (entityGroup = GameObject.Find("Entityes")) == null)
                    entityGroup = new GameObject("Entityes");
                return entityGroup;
            }
        }
        public GameObject GetPrefab { get; private set; }
        private void DebugWarning(TypeEntity typeEntity, string Name)
        {
            Debug.LogWarning($"Resource not found; TypeEntity: {typeEntity} NameEntity: {Name}");
        }
        private void DebugWarning(string path)
        {
            Debug.LogWarning($"Resource not found; Path: {path}");
        }
        public void LoadPrefab(GameObject gameObject)
        {
            GetPrefab = gameObject;
            
            GetPrefab.transform.SetParent(EntityGroup.transform);
        }
        private GameObject LoadInResource(string path)
        {
            GameObject game = Resources.Load<GameObject>(path);
            if (game)
                return game;
            DebugWarning(path);
            return null;
        }
        public EntityStencilCreating(Transform parent, bool isStatic, TypeEntity typeEntity, string Direction, string Name)
        {
            _isStaticItem = isStatic;
            string path = typeEntity + "\\" + Direction + "\\";
            GameObject obj = LoadInResource($"{path}{Name}");
            if (obj == null)
                return;
            LoadPrefab(Object.Instantiate(obj, parent));
        }
        public EntityStencilCreating(Vector3 vector, Quaternion quaternion, bool isStatic, TypeEntity typeEntity, string Direction, string Name)
        {
            _isStaticItem = isStatic;
            string path = typeEntity + "\\" + Direction + "\\";
            GameObject obj = LoadInResource($"{path}{Name}");
            if (obj == null)
                return;
            LoadPrefab(Object.Instantiate(obj, vector, quaternion));
        }
        public EntityStencilCreating(Vector3 vector, Quaternion quaternion, bool isStatic, TypeEntity typeEntity, string Name)
        {
            _isStaticItem = isStatic;
            string path = typeEntity + "\\";
            #region LoadInResources
            GameObject obj = Resources.Load<GameObject>($"{path}Prefabs\\{Name}");
            if (obj)
            {
                LoadPrefab(GameObject.Instantiate(obj, vector, quaternion));
                GetPrefab.name = Name;
                return;
            }
            obj = LoadInResource($"{path}Mesh\\{Name}");

            if (obj == null) return;

            obj = Object.Instantiate(obj, vector, quaternion);

            obj.layer = LayerMask.NameToLayer("Entity");

            Material material = Resources.Load<Material>($"{path}Material\\{Name}");

            Transform[] Chields = obj.GetComponentsInChildren<Transform>();

            if (Chields.Count() == 1)
            {
                if (material)
                    obj.GetComponent<Renderer>().material = material;
                obj.AddComponent<MeshCollider>().convex = !isStatic;
            }
            else
            {
                List<GameObject> Destroed = new List<GameObject>();
                bool IsColliders = false;
                foreach (Transform Chield in Chields)
                {
                    GameObject gameObject = Chield.gameObject;
                    gameObject.layer = obj.layer;
                    MeshCollider collider = gameObject.GetComponent<MeshCollider>();
                    if (collider == null)
                        collider = gameObject.AddComponent<MeshCollider>();
                    if (Chield.name.Contains("Collider"))
                    {
                        if (isStatic)
                        {
                            Destroed.Add(gameObject);
                        }
                        else
                        {
                            collider.convex = true;
                            gameObject.GetComponent<MeshRenderer>().enabled = false;
                        }
                        IsColliders = true;
                    }
                    else
                    {
                        Material ChieldMaterial = (Material)Resources.Load($"{path}Material\\{Chield.name}");
                        collider.enabled = false;
                        if (ChieldMaterial != null)
                            gameObject.GetComponent<Renderer>().material = ChieldMaterial;
                    }
                }
                foreach (GameObject Obj in Destroed)
#if UNITY_EDITOR
                    Object.DestroyImmediate(Obj);
#else
                    Object.DestroyImmediate(Obj);
#endif
                if (!IsColliders || IsColliders && isStatic)
                    foreach (MeshCollider Chield in obj.transform.GetComponentsInChildren<MeshCollider>())
                    {
                        Chield.enabled = true;
                        if (!isStatic)
                            Chield.convex = true;
                    }
            }
            #endregion
            LoadPrefab(obj);
            GetPrefab.name = Name;
        }
    }
}
