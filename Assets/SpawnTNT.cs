using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnTNT : MonoBehaviour
{
    private Queue<EntityEngine> pool = new Queue<EntityEngine>();
    void Start()
    {
        StartCoroutine(Spawn());
    }
    private void OnTriggerEnter(Collider other)
    {
        EnterTrigger(other);
    }

    private void EnterTrigger(Collider collider)
    {
        var entity = collider.GetComponentInParent<EntityEngine>();
        if (entity.gameObject == null)
        {
            Debug.LogWarning("gameObject Null");
            return;
        }
        entity.gameObject.SetActive(false);
        pool.Enqueue(entity);
    }

    IEnumerator Spawn()
    {
        while (true)
        {
            if (pool.Count <= 0)
            {
                ItemEngine.AddItem(
                Random.Range(0, 2) == 0 ? TypeItem.TNT_3 : TypeItem.TNT,
                transform.position,
                Quaternion.identity,
                false);
            }

            else
            {
                EntityEngine rigidbody = pool.Dequeue();
                rigidbody.gameObject.SetActive(true);
                rigidbody.Rigidbody.velocity = Vector3.zero;
                rigidbody.Rigidbody.angularVelocity = Vector3.zero;
                rigidbody.Rigidbody.MovePosition(transform.position);
            }
            yield return new WaitForSeconds(5f);
        }

    }
}
