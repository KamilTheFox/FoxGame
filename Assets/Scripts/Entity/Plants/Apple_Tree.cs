using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Tweener;


public class Apple_Tree : PlantEngine
{
    struct PointerCreate
    {
        public PointerCreate(Transform _Transform, int hashRand, UnityAction toFallApple)
        {
            Transform = _Transform;
            int rand = Random.Range(0, 5);
            
            if (hashRand != rand)
            {
                apple = null;
                tweenApple = null; tweenColor = null; tweenPostion = null;
                action = null;
                return;
            }
            action = toFallApple;
            Debug.Log("Create Apple");
            apple = (Apple)ItemEngine.AddItem(TypeItem.Apple,Transform.position, Quaternion.identity);
            apple.Rigidbody.isKinematic = true;

            float timeSpeedTween = Random.Range(5F,30F);

            tweenPostion = Tween.AddPosition(apple.Transform,
                new Vector3(0, -apple.Transform.GetComponent<Renderer>().bounds.size.y, 0),
                timeSpeedTween
                );
            tweenColor = (IExpansionTween)Tween.SetColor(apple.Transform, Color.green, timeSpeedTween).ReverseProgress();
            tweenApple = Tween.SetScale(apple.Transform, Vector3.zero, timeSpeedTween).ReverseProgress();

            tweenApple.ToCompletion(ToFallApple);
        }
        public void ToFallApple()
        {
            if (apple == null && apple.Rigidbody == null)
                return;

            apple.Rigidbody.isKinematic = false;
            Tween.Stop(tweenApple); Tween.Stop(tweenPostion); Tween.Stop(tweenColor);
            action?.Invoke();
        }
        private UnityAction action;
        public Transform Transform;
        private Apple apple;
        public bool Free { get => apple == null; }
        public IExpansionTween tweenApple, tweenPostion, tweenColor;
    };
    List<PointerCreate> pointerCreate = new();
    protected override void OnStart()
    {
        Transform[] Pointer = gameObject.GetComponentsInChildren<Transform>().Where(children => children.gameObject.name == "AppleCreate").ToArray();
        foreach(Transform Create in Pointer)
        {
        pointerCreate.Add(new PointerCreate(Create, Random.Range(0,5), RandomSpawnApple));
        }
    }
    private void RandomSpawnApple()
    {
        if (IsDead) return;

        for(int i = 0; i < pointerCreate.Count; i++)
        {
            if(pointerCreate[i].Free)
            {
                pointerCreate[i] = new PointerCreate(pointerCreate[i].Transform, Random.Range(0, 5), RandomSpawnApple);
            }
        }
        if(pointerCreate.Where(point => !point.Free).Count() == 0)
        {
            RandomSpawnApple();
        }
    }
    public override void Dead()
    {
        base.Dead();
        pointerCreate.ForEach(apple =>
        {
            apple.ToFallApple();
        });
        pointerCreate.Clear();
    }
}
