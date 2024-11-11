using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ArmamentItem : ItemEngine, IStriker, IWieldable
{
    [SerializeField] private TriggerDetect triggerPunch;

    [field: SerializeField] public string UpAxis { get; set; }

    [field: SerializeField]public float Damage { get; private set; }

    public bool IsActive { get; set; }

    [field: SerializeField] public Vector3 WieldPoint { get; private set; }
    public Transform RootParent { private get; set; }

    private bool canDealDamage = true;

    [SerializeField] private float hitCooldown = 0.05f;

    protected override void OnStart()
    {
        base.OnStart();
        if (triggerPunch == null)
            return;
        triggerPunch.gameObject.SetActive(false);
        triggerPunch.Enter += OnTrigger;
    }
#if UNITY_EDITOR
    [SerializeField] bool isChangedWieldPoint;
    private void OnValidate()
    {
        if(isChangedWieldPoint)
            WieldPoint = transform.localPosition;
    }
#endif
    public override void Delete(float time = 0)
    {
        base.Delete(time);
        triggerPunch.Enter -= OnTrigger;
    }

    private void OnTrigger(Collider collider)
    {
        if (!canDealDamage) return;

        if (collider.transform.IsChildOf(RootParent)) return;

        Debug.Log("УДАР!: " + collider.name);

        var damageable = collider.GetComponentInParent<IDamaged>();
        if (damageable != null)
        {
            damageable.TakeHit(this);
            StartCoroutine(DamageCooldown());
        }
    }

    private IEnumerator DamageCooldown()
    {
        canDealDamage = false;
        yield return new WaitForSeconds(hitCooldown);
        canDealDamage = true;
    }

    public void EnableWeapon()
    {
        triggerPunch.gameObject.SetActive(true);
    }

    public void DisableWeapon()
    {
        triggerPunch.gameObject.SetActive(false);
    }

    public void SetWieldHand(Transform parent)
    {
        transform.SetParent(parent);
        Transform.localPosition = Vector3.zero;
        Transform.localRotation = Quaternion.Euler(Vector3.zero);
    }
}
