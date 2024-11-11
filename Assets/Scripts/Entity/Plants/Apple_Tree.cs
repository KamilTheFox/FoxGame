using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;
using Tweener;
using Random = System.Random;

public class Apple_Tree : Tree
{
    class PointerCreate
    {
        public PointerCreate(Transform _Transform, Action toFall)
        {
            apple = null;
            Transform = _Transform;
            ToFall = toFall;
        }
        public void CreateApple()
        {
            if (!Free) return;
            apple = (IGerminatable)ItemEngine.AddItem(TypeItem.Apple, Transform.position, Quaternion.identity, false);
            apple.OnRipen += ToFall;
            apple.OnRipen += ToFallApple;
            apple.Start(Transform.position);
        }
        public void ToFallApple()
        {
            if(!Free)
            apple.Stop();
            apple = null;
        }
        private Action ToFall;
        public Transform Transform;
        private IGerminatable apple;
        public bool Free { get => apple == null; }
        
    };
    private static Random random = new Random();

    [SerializeField] private int countApple;

    List<PointerCreate> pointerCreate;
    protected override void OnStart()
    {
        Transform[] Pointer = gameObject.GetComponentsInChildren<Transform>().Where(children => children.gameObject.name == "AppleCreate").ToArray();
        Action toFall = CheckItGrowing;
        toFall += RandomSpawnApple;
        pointerCreate = new();
        foreach (Transform Create in Pointer)
        {
            pointerCreate.Add(new PointerCreate(Create, toFall));
        }
        random = new Random();
        Invoke(nameof(RandomSpawnApple),10f);
    }
    public void CheckItGrowing()
    {
        int CountFreeApple = pointerCreate.Where(point => point.Free).Count();
        if (CountFreeApple == 0)
        {
            RandomSpawnApple();
        }
    }
    private void RandomSpawnApple()
    {
        if (IsDie) return;
        int i = 0;
        int max = random.Next(1, pointerCreate.Count);
        while (i < max)
        {
            if (pointerCreate.Where(point => point.Free).Count() == 0)
                break;
            pointerCreate.First((t) => t.Free).CreateApple();
            i++;
        }
        countApple = pointerCreate.Where(point => !point.Free).Count();
    }
    public override void Death()
    {
        pointerCreate.ForEach(apple =>
        {
            apple.ToFallApple();
        });
        base.Death();
        pointerCreate.Clear();
    }
}
