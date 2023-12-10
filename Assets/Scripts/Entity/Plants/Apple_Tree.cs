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

    [SerializeField] private int countaple;

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
        RandomSpawnApple();
    }
    public void CheckItGrowing()
    {
        int CountFreeApple = pointerCreate.Where(point => !point.Free).Count();
        if (CountFreeApple == 0)
        {
            RandomSpawnApple();
        }
    }
    private void RandomSpawnApple()
    {
        if (IsDie) return;
        int i = 0;
        while (i < random.Next(pointerCreate.Count))
        {
            if (pointerCreate.Where(point => point.Free).Count() < 1)
                break;
            int y = random.Next(pointerCreate.Count);
            if (pointerCreate[y].Free)
            {
                pointerCreate[y].CreateApple();
                i++;
            }
        }
        countaple = pointerCreate.Where(point => !point.Free).Count();
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
