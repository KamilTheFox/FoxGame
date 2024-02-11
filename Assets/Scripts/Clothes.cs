using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clothes : MonoBehaviour
{
    [SerializeField] private int[] startPutOn;
    [SerializeField] private GameObject[] clothesPrefab;
    [SerializeField] private SkinnedMeshRenderer[] clothes;
    [SerializeField] private SkinnedMeshRenderer mainMeshRenderer;

    [SerializeField] private int indexSelect;

    private Dictionary<int, SkinnedMeshRenderer> currentSelects;
    
    [ContextMenu("ResetList")]
    private void Reset()
    {
        if (clothesPrefab == null)
        {
            Debug.LogWarning("Clothes prefabs are not inserted");
            return;
        }
        List<SkinnedMeshRenderer> clothesList = new List<SkinnedMeshRenderer>();
        foreach (var clothes in clothesPrefab)
        {
            clothesList.AddRange(clothes.GetComponentsInChildren<SkinnedMeshRenderer>());
        }
        clothes = clothesList.ToArray();
    }
    private void Awake()
    {
        if(clothes == null || clothes.Length<=0)
            Reset();
        foreach (var cloth in startPutOn)
        {
            SelectClothes(cloth);
        }
    }

    [ContextMenu("Select Clothes to Index")]
    private void Select()
    {
        SelectClothes(indexSelect);
    }
    public void SelectClothes(int indexClothes)
    {
        if (clothes.Length - 1 < indexClothes)
        {
            Debug.LogError("The clothing index is larger than the clothing list");
            return;
        }
        if(currentSelects == null)
            currentSelects = new Dictionary<int,SkinnedMeshRenderer>();
        if(!currentSelects.ContainsKey(indexClothes))
        {
            currentSelects.Add(indexClothes, Initalize(clothes[indexClothes]));
            return;
        }
        currentSelects[indexClothes].gameObject.SetActive(!currentSelects[indexClothes].gameObject.activeSelf);
    }


    private SkinnedMeshRenderer Initalize(SkinnedMeshRenderer Clothes)
    {
        SkinnedMeshRenderer skinned = GameObject.Instantiate<SkinnedMeshRenderer>(Clothes);
        skinned.bones = mainMeshRenderer.bones;
        skinned.rootBone = mainMeshRenderer.rootBone;
        skinned.transform.SetParent(mainMeshRenderer.transform.parent);
        return skinned;
    }
}
