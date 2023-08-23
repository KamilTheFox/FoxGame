using System.Collections;
using UnityEngine;
using Tweener;


public class Box : ItemEngine, IDiesing
{
    [SerializeField] private TypeItem ContainsItem;

    [SerializeField] private bool IsMetallBox;

    private GameObject Object;

    protected override void OnAwake()
    {
        base.OnAwake();
        Stationary = true;
        if (ContainsItem == TypeItem.None)
            ContainsItem = GetContainsItem;
        CreateItem();
    }

    private TypeItem GetContainsItem
    {
        get
        {
            int rnd = Random.Range(0, 100);
            if( rnd < 50)
            {
                return TypeItem.Apple;
            }
            if (rnd == 50)
            {
                return TypeItem.TNT;
            }
            if (rnd > 50 && rnd < 70)
            {
                return TypeItem.Basket;
            }
            if (rnd > 70 && rnd < 90)
            {
                return TypeItem.CocaCola;
            }
            return TypeItem.None;
        }
    }

    public bool IsDie { get; private set; }

    public void Death()
    {
        if (IsDie)
            return;
        IsDie = true;

        Tween.SetColor(Transform, new Color(0F, 0F, 0F, 0F), 2F).
            ChangeEase(Ease.CubicRoot).
            IgnoreAdd(IgnoreARGB.RGB).
            TypeOfColorChange(TypeChangeColor.ObjectAndHierarchy).
            ToCompletion(() => Delete());
        if (Object == null)
            return;
        Object.SetActive(true);
        Object.transform.SetParent(GetGroup.transform);
    }
    private void CreateItem()
    {
        if (ContainsItem == TypeItem.None)
            return;
        Object = AddItem(ContainsItem, Transform.Find("Box").position, Quaternion.identity, false).gameObject;

        Invoke(nameof(DiactiveObject), 0.1F);
    }
    private void DiactiveObject()
    {
        Object.transform.SetParent(Transform);
        Object.SetActive(false);
    }
    public override TextUI GetTextUI()
    {
        return new TextUI(() => new object[]
        {
             itemType.ToString() 
        });
    }
    private void OnCollisionEnter(Collision collision)
    {
        Rigidbody rigidbody;
        if (rigidbody = collision.gameObject.GetComponentInParent<Rigidbody>())
            if (rigidbody.mass * rigidbody.velocity.magnitude > (IsMetallBox ? 300F : 30F))
                Death();
    }
}
