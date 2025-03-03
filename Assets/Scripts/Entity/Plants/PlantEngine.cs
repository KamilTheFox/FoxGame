using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Tweener;
using System;

public class PlantEngine : EntityEngine, IDiesing
{
    private SpriteRenderer objectSimplePlant;

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
            return Base[TypeEntity.Plant].Select(plant => (PlantEngine)plant).ToArray();
        }
    }

    public override TypeEntity typeEntity => TypeEntity.Plant;
    [HideInInspector] public TypePlant typePlant;

    [SerializeField] private RendererBuffer rendererBuffer;

    public bool IsDie { get; protected set; }

    [field: SerializeField] public Sprite DistanceVersion { get; private set; }

    protected override void OnAwake()
    {
        base.OnAwake();
        //rendererBuffer.SetGameObject(gameObject);
        //if (IsLittlePlant(typePlant))
        //    rendererBuffer.IsGenerateSprite = false;
        //else
        //    ((Action)rendererBuffer.GenerateLODSprite).AddListnerNextUpdate();
    }

    //Устаревшее. Использовать не валидно
    private void GenerateSimpleVersion()
    {
        objectSimplePlant = new GameObject("SimpleVersion").AddComponent<SpriteRenderer>();
        objectSimplePlant.sprite = DistanceVersion;
        Bounds spriteBound = objectSimplePlant.bounds;
        Bounds bounds = GetEncapsulateBoundsPlant();
        objectSimplePlant.transform.position = bounds.center;
        Vector3 Divise = Division(spriteBound.size, bounds.size);
        Vector3 newScale = transform.localScale - Divise + objectSimplePlant.transform.localScale;
        objectSimplePlant.transform.localScale = newScale * 1.15F;
        objectSimplePlant.transform.SetParent(transform);
    }
    private Vector3 Division(Vector3 value1 ,Vector3 value2)
    {
        return new Vector3(value1.x / value2.x, value1.y / value2.y, value1.z / value2.z);
    }
    private Bounds GetEncapsulateBoundsPlant()
    {
        Bounds bounds = new Bounds();
        foreach (MeshRenderer renderer in MeshRenderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }
        return bounds;
    }
    public bool IsLittlePlant(TypePlant type)
    {
        return new List<TypePlant>()
        {
            TypePlant.Agaric_Mushroom,TypePlant.Nasty_Mushroom,TypePlant.White_Mushroom
        }.Contains(type);
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
        tween = Tween.SetColor(Transform, new Color(1, 1, 1, 0), 3F).
            IgnoreAdd(IgnoreARGB.RGB).
            ChangeEase(Ease.CubicRoot).
            TypeOfColorChange(TypeChangeColor.ObjectAndHierarchy).
            ToCompletion(() => Delete());
    }
   
    public override TextUI GetTextUI()
    {
        return (typeEntity.ToString() + " " + typePlant.ToString()).GetTextUI();
    }
}

