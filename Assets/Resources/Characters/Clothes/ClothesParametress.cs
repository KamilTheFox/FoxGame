using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[CreateAssetMenu(menuName = "ClothesParametress", fileName = "Origin")]
public class ClothesParametress : ScriptableObject
{
    [SerializeField] private GameObject[] clothesPrefab;

    [SerializeField] private SkinnedMeshRenderer[] clothes;

    [SerializeField] private List<string> names;

    [SerializeField] private List<Opposition> opposition;

    public SkinnedMeshRenderer this[int index]
    {
        get 
        { 
            return clothes[index];
        }
    }

    public int Count => clothes.Length;

    public string[] GetNames => clothes.Select(c => c.name).ToArray();

    public int GetIndexOfName(string name)
    {
        if (clothes == null) return -1;
        return clothes.ToList().FindIndex((x) => x.name == name);
    }

    public int[] GetClothesNotCompatible(int index)
    {
        var t = opposition.Find((x) => x.nameCurrentPut == clothes[index].name);
        if (t == null) return new int[0];
        return t.NotCompatible.Select((y) => GetIndexOfName(y)).ToArray();
    }



    [ContextMenu("ReadClothes")]
    private void ReadClothes()
    {
        if (clothesPrefab == null)
            return;
        List<SkinnedMeshRenderer> clothesList = new List<SkinnedMeshRenderer>();
        foreach (var clothes in clothesPrefab)
        {
            clothesList.AddRange(clothes.GetComponentsInChildren<SkinnedMeshRenderer>());
        }
        clothes = clothesList.ToArray();
    }

    [Serializable]
    private class Opposition
    {
        [SerializeField] public string nameCurrentPut;

        [SerializeField] public List<string> NotCompatible;
    }
}
