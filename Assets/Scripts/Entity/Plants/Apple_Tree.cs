using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;


    public class Apple_Tree : PlantEngine
    {
        struct PointerCreate
        {
        public PointerCreate(Transform _Transform, Apple _Apple)
        {
            Transform = _Transform;
            Apple = _Apple;
        }
        public Transform Transform;
        public Apple Apple;

        };
        List<PointerCreate> pointerCreate = new();
        protected override void OnStart()
        {
        Transform[] Pointer = gameObject.GetComponentsInChildren<Transform>().Where(children => children.gameObject.name == "AppleCreate").ToArray();
            foreach(Transform Create in Pointer)
            {
            pointerCreate.Add(new PointerCreate(Create, CreateApple(Create)));
            }
        StartCoroutine(Maturation());
        }
    public override void Dead()
    {
        base.Dead();
        pointerCreate.ForEach(apple => apple.Apple.Rigidbody.isKinematic = false);
        pointerCreate.Clear();
    }
    private Apple CreateApple(Transform Create)
    {
        Apple apple = ItemEngine.AddItem(TypeItem.Apple, Create.position, Create.rotation, false) as Apple;
        apple.Transform.localScale = Vector3.one * 0.2F;
        apple.Transform.position -= Vector3.down * (apple.Transform.position.y - apple.MeshRenderer.bounds.max.y);
        apple.MeshRenderer.material.color = new Color(0F, 1F, 0F);
        apple.Rigidbody.isKinematic = true;
        return apple;
    }
    private IEnumerator Maturation()
    {
        while (!IsDead)
        {
            yield return new WaitForSeconds(Random.Range(0.1F, 0.5F));
            if (IsDead) break;
            PointerCreate _pointerCreate = pointerCreate[Random.Range(0, pointerCreate.Count)];
            Apple apple = _pointerCreate.Apple;
            if (!apple.Rigidbody || !apple.Rigidbody.isKinematic)
            {
                pointerCreate.Add(new PointerCreate(_pointerCreate.Transform, CreateApple(_pointerCreate.Transform)));
                pointerCreate.Remove(_pointerCreate);
                continue;
            }
            Color oldColor = apple.MeshRenderer.material.color;
            apple.MeshRenderer.material.color = new Color(oldColor.r >= 1F?1F : oldColor.r + 0.06F, 1F, oldColor.b >= 1F ? 1F : oldColor.b + 0.06F);
            apple.Transform.localScale += Vector3.one * 0.01F;
            apple.Transform.localPosition = _pointerCreate.Transform.position - (apple.Transform.position.y - apple.MeshRenderer.bounds.max.y) * Vector3.down;
            if (apple.Transform.localScale.y >= 0.7F)
            {
                apple.Rigidbody.isKinematic = false;
                pointerCreate.Add(new PointerCreate(_pointerCreate.Transform, CreateApple(_pointerCreate.Transform)));
                pointerCreate.Remove(_pointerCreate);
            }
        }
        yield break;
    }

}