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

        private static EntityEngineBase entityGroup;
        public static EntityEngineBase EntityGroup { get
            {
                if (entityGroup == null)
                {
                    GameObject game = GameObject.Find("Entityes");
                    if(game == null) 
                        return null;
                    entityGroup = game.GetComponent<EntityEngineBase>();
                    if (entityGroup == null)
                    {
                        entityGroup = game.AddComponent<EntityEngineBase>();
                    }
                }
                if(entityGroup == null)
                {
                    Debug.LogError("entityGroup == null");
                }
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
        private void CheckNullGroupEntity()
        {
            if (entityGroup == null)
                entityGroup = new GameObject("Entityes").AddComponent<EntityEngineBase>();
        }
        public void SetParent(GameObject gameObject)
        {
            GetPrefab = gameObject;
            if (EntityGroup)
                GetPrefab.transform.SetParent(EntityGroup.transform);
        }
        private GameObject LoadInResource(string path, bool debug = false)
        {
            GameObject game = Resources.Load<GameObject>(path);
            if (game)
                return game;
            if(debug)
                DebugWarning(path);
            return null;
        }
        public EntityStencilCreating(Transform parent, bool isStatic, TypeEntity typeEntity, string Direction, string Name)
        {
            CheckNullGroupEntity();
            _isStaticItem = isStatic;
            string path = typeEntity + "\\" + Direction + "\\";
            GameObject obj = LoadInResource($"{path}{Name}", true);
            if (obj == null)
                return;
            SetParent(Object.Instantiate(obj, parent));
        }
        public EntityStencilCreating(Vector3 vector, Quaternion quaternion, bool isStatic, TypeEntity typeEntity, string Direction, string Name)
        {
            CheckNullGroupEntity();
            _isStaticItem = isStatic;
            string path = typeEntity + "\\" + Direction + "\\";
            GameObject obj = LoadInResource($"{path}{Name}", true);
            if (obj == null)
                return;
            SetParent(Object.Instantiate(obj, vector, quaternion));
        }
        public EntityStencilCreating(Vector3 vector, Quaternion quaternion, bool isStatic, TypeEntity typeEntity, string Name)
        {
            CheckNullGroupEntity();
            _isStaticItem = isStatic;
            string path = typeEntity + "\\";
            #region LoadInResources
            GameObject obj = LoadInResource($"{path}Prefabs\\{Name}");
            if (obj != null)
            {
                SetParent(GameObject.Instantiate(obj, vector, quaternion));
                GetPrefab.name = Name;
                return;
            }
            obj = LoadInResource($"{path}Prefabs\\EmptyProperty\\{Name}");

            if (obj == null)
            {
                obj = LoadInResource($"{path}Mesh\\{Name}", true);
            }
                

            if (obj == null)
            {
#if !UNITY_EDITOR
                Menu.Error(LText.Temporarily_unavailable.GetTextUI().ToString());
#endif
                return;
            }

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
#if UNITY_EDITOR
                            if (Application.isPlaying)
                            {
                                Object.Destroy(gameObject.GetComponent<MeshRenderer>());
                            }
                            else
#endif
                                Object.DestroyImmediate(gameObject.GetComponent<MeshRenderer>());
                        }
                        IsColliders = true;
                    }
                    else if(Chield.name.Contains("Trigger"))
                    {
#if UNITY_EDITOR
                        if (Application.isPlaying)
                        {
                            Object.Destroy(gameObject.GetComponent<MeshRenderer>());
                        }
                        else
#endif
                            Object.DestroyImmediate(gameObject.GetComponent<MeshRenderer>());
                        gameObject.AddComponent<Rigidbody>().isKinematic = true;
                        gameObject.AddComponent<TriggerDetect>();
                        collider.convex = true;
                        collider.isTrigger = true;
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
            SetParent(obj);
            GetPrefab.name = Name;
        }
    }
}
