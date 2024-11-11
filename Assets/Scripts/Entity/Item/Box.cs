using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Tweener;
using System;
using Random = UnityEngine.Random;

public class Box : ItemEngine, IDamaged, IDiesing, IAppliedExplosionForce, IExplosionDamaged
{
    [SerializeField] private TypeItem ContainsItem;

    [SerializeField] private bool IsMetallBox;

    private float explosionForce, explosionRadius;

    private Vector3 centerExplosion;

    [SerializeField] private Material fadeModeMain, fadeModeFace, fadeModeMelal;

    private ItemEngine[] Objects;

    private Dictionary<float, TypeItem> rangeRandomItem = new Dictionary<float, TypeItem>()
    {
        [0F] = TypeItem.None,
        [0.1F] = TypeItem.Apple,
        [0.2F] = TypeItem.Banana,
        [0.3F] = TypeItem.Melon,
        [0.4F] = TypeItem.Pineapple,
        [0.5F] = TypeItem.CocaCola,
        [0.51F] = TypeItem.TNT,
        [0.52F] = TypeItem.TNT_3
    };

    protected override void OnStart()
    {
        base.OnStart();
        Health = IsMetallBox ? 3F : 1F;
        foreach (Transform child in Transform)
        {
            child.gameObject.AddComponent<MeshCollider>().convex = true;
            //Renderer renderer = child.GetComponent<Renderer>();
            //Material oldMaterial = renderer.material;
            //renderer.material = new Material(renderer.sharedMaterial);
        }
        Stationary = true;
        if (ContainsItem == TypeItem.None)
            ContainsItem = GetContainsItem;
        CreateItem();
    }

    private TypeItem GetContainsItem
    {
        get
        {
            float rnd = Random.Range(0F, 1F);
            if (rnd > 0.52)
                rnd /= 2;
            return rangeRandomItem[float.Parse(rnd.ToString("0.0"))];
        }
    }

    public bool IsDie { get; private set; }

    public float Health { get; private set; }

    public void Death()
    {
        if (IsDie)
            return;
        IsDie = true;

        if (Objects != null)
            foreach (ItemEngine obj in Objects)
            {
                if (obj != null)
                {
                    obj.gameObject.SetActive(true);
                    obj.transform.SetParent(GetGroup.transform);
                    if (obj.Rigidbody)
                        obj.Rigidbody.AddExplosionForce(explosionForce, centerExplosion, explosionRadius);
                }
            }
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        foreach (Transform child in Transform)
        {
            child.gameObject.layer = LayerMask.NameToLayer("NotClimbinding");
            Renderer rend = child.gameObject.GetComponent<Renderer>();
            child.gameObject.AddComponent<Rigidbody>().AddExplosionForce(explosionForce, centerExplosion, explosionRadius);
            Material old = rend.material;
            if (old.name.Contains("Main"))
            {
                rend.sharedMaterial = fadeModeMain;
            }
            else if (old.name.Contains("Metal"))
                rend.sharedMaterial = fadeModeMelal;
            else
                rend.sharedMaterial = fadeModeFace;

        }
        Tween.SetColor(Transform, new Color(0F, 0F, 0F, 0F), 10F).
            ChangeEase(Ease.FiveDegree).
            IgnoreAdd(IgnoreARGB.RGB).
            TypeOfColorChange(TypeChangeColor.ObjectAndHierarchy).
            ToCompletion(() => Delete());
        Rigidbody.isKinematic = true;
    }
    
    
    private int GetCountRandomObject(TypeItem item)
    {
        if (item == TypeItem.Apple || item == TypeItem.Banana)
            return Random.Range(10, 30);
        if (item == TypeItem.CocaCola)
            return Random.Range(1, 5);
        if (item == TypeItem.Pineapple)
            return Random.Range(1, 3);

        return 1;
    }
    private void CreateItem()
    {
        if (ContainsItem == TypeItem.None)
            return;
        int countItem = GetCountRandomObject(ContainsItem);
        Objects = new ItemEngine[countItem];
        for (int i = 0; i < countItem; i++)
            Objects[i] = AddItem(ContainsItem, Transform.Find("Box").position, Quaternion.identity, false);

        DiactiveObject();
    }
    private void DiactiveObject()
    {
        if (Objects == null) return;
        foreach (ItemEngine obj in Objects)
        {
            obj.transform.SetParent(Transform);
            obj.gameObject.SetActive(false);
        }
    }
    public override TextUI GetTextUI()
    {
        return new TextUI(() => new object[]
        {
             itemType.ToString() 
        });
    }
    public override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);
        Rigidbody rigidbody;
        if (rigidbody = collision.gameObject.GetComponentInParent<Rigidbody>())
            if (rigidbody.mass * rigidbody.velocity.magnitude > (IsMetallBox ? 900F : 200F))
            {
                CrashBox(collision.contacts[0].point);
            }
    }

    public void CrashBox(Vector3 punchPoint)
    {
        if (!IsDie)
            SoundMeneger.PlayPoint(SoundMeneger.Sounds.CrushWoodBox, punchPoint, false, volume: 0.5F).pitch = Random.Range(0.7F, 1.3F);
        Death();
    }

    public void SetExplosionForce(float explosionForce, Vector3 centerExplosion, float explosionRadius)
    {
        this.centerExplosion = centerExplosion;
        this.explosionForce = explosionForce;
        this.explosionRadius = explosionRadius;
        ((Action)ResetForce).AddListnerNextFixedUpdate();
    }
    private void ResetForce()
    {
        this.centerExplosion = Vector3.zero;
        this.explosionForce = 0;
        this.explosionRadius = 0;
    }

    public void TakeHit(IStriker striker)
    {
        Health -= striker.Damage;
        if (Health <= 0)
        {
            Health = 0;
            CrashBox(transform.position);
        }
    }
}
