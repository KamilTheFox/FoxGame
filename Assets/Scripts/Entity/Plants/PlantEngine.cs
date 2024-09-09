using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Tweener;

public class PlantEngine : EntityEngine, IDiesing
{

    [SerializeField] private SpriteRenderer objectSimplePlant;

    private MeshRenderer[] meshRenderers;

    private MeshRenderer[] MeshRenderers
    {
        get
        {
            if(meshRenderers == null)
                meshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();
            return meshRenderers;
        }
    }
    private IExpansionColor tween;
    public static PlantEngine[] GetPlants
    {
        get
        {
#if UNITY_EDITOR
            return GameObject.FindObjectsOfType<PlantEngine>();
#else
            List<PlantEngine> itemEngines = new List<PlantEngine>();
            Entities[TypeEntity.Plant].ForEach(engine => { if (engine is PlantEngine plant) { itemEngines.Add(plant); } });
            return itemEngines.ToArray();
#endif
        }
    }

    public override TypeEntity typeEntity => TypeEntity.Plant;
    [HideInInspector] public TypePlant typePlant;
    public bool IsDie { get; protected set; }

    [field: SerializeField] public Sprite DistanceVersion { get; private set; }

    protected override void OnAwake()
    {
        base.OnAwake();
        EnableDistanceVersion(false);
        distanceOrder = Settings.DrawingRangePlant;
    }
    [ContextMenu("GenerateSimpleVersion")]
    private void GenerateSimpleVersion()
    {
        objectSimplePlant = new GameObject("SimpleVersion").AddComponent<SpriteRenderer>();
        objectSimplePlant.sprite = DistanceVersion;
        Bounds spriteBound = objectSimplePlant.bounds;
        Bounds bounds = GetMinMaxBoundsPlant();
        objectSimplePlant.transform.position = bounds.center;
        Vector3 Divise = Division(spriteBound.size, bounds.size);
        Vector3 newScale = transform.localScale - Divise + objectSimplePlant.transform.localScale;
        objectSimplePlant.transform.localScale = newScale * 1.15F;
        objectSimplePlant.transform.SetParent(transform);
    }
    public void EnableDistanceVersion(bool enabled = true)
    {
        if (DistanceVersion == null)
            return;
        if (objectSimplePlant == null)
        {
            GenerateSimpleVersion();
        }
        if (objectSimplePlant.gameObject.activeSelf == enabled)
            return;
        objectSimplePlant.gameObject.SetActive(enabled);
        foreach(MeshRenderer meshes in MeshRenderers)
        {
            if(meshes != null)
            meshes.gameObject.SetActive(!enabled);
        }
    }
    private Vector3 Division(Vector3 value1 ,Vector3 value2)
    {
        return new Vector3(value1.x / value2.x, value1.y / value2.y, value1.z / value2.z);
    }
    private Bounds GetMinMaxBoundsPlant()
    {
        Bounds bounds = new Bounds();
        foreach (MeshRenderer renderer in MeshRenderers)
        {
            Vector3 min = renderer.bounds.min;
            Vector3 max = renderer.bounds.max;
            bounds.max += max;
            bounds.min += min;
        }
        bounds.max /= meshRenderers.Length;
        bounds.min /= meshRenderers.Length;
        return bounds;
    }
    public override void OnBatchDistanceCalculated(bool enable)
    {
        if (objectSimplePlant == null)
            return;
        Vector3 target = CameraControll.instance.transform.position;
        EnableDistanceVersion(!enable);
        target = new Vector3 (target.x, objectSimplePlant.transform.position.y, target.z);
        objectSimplePlant.transform.LookAt(target, Vector3.up);
    }
    public static PlantEngine AddPlant(TypePlant plant, Vector3 vector, Quaternion quaternion)
    {
        return AddEntity<PlantEngine>(plant, vector, quaternion);
    }
    protected override void onDestroy()
    {
        if(tween!=null)
            Tween.Stop((IExpansionTween)tween);
        base.onDestroy();
    }
    public virtual void Death()
    {
        if (IsDie) return;
        IsDie = true;
        tween = Tween.SetColor(Transform, new Color(1, 1, 1, 0), 3F).IgnoreAdd(IgnoreARGB.RGB).ChangeEase(Ease.CubicRoot).TypeOfColorChange(TypeChangeColor.ObjectAndHierarchy).ToCompletion(() => Delete());
    }
   
    public override TextUI GetTextUI()
    {
        return (typeEntity.ToString() + " " + typePlant.ToString()).GetTextUI();
    }
}

