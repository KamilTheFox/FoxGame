﻿using System.Collections.Generic;
using System;
using UnityEngine;
using VulpesTool;

public class Clothes : VulpesMonoBehaviour
{
    [SerializeField] private int[] startPutOn;
    
    [SerializeField] private SkinnedMeshRenderer mainMeshRenderer;

    //[ButtonField(nameof(Select), buttonText: "Select", ColorsGUI.Green, ButtonPosition.Left, isChangeScene: true)]
    [HideInInspector]
    [SerializeField] private int indexSelect;

    [SerializeField] private ClothesParametress ClothesParametress;
    [HideInInspector]
    [SerializeField] private DictionaryClothes currentSelects;

    [Serializable]
    private class DictionaryClothes
    {
        [SerializeField] public List<int> indexs = new();
        [field: SerializeField] public List<SkinnedMeshRenderer> Values { get; private set; } = new();

        public bool ContainsKey(int _index)
        {
            return indexs.Contains(_index);
        }
        public void Add(int i, SkinnedMeshRenderer mesh)
        {
            indexs.Add(i);
            Values.Add(mesh);
        }
        public SkinnedMeshRenderer this[int i]
        {
            get
            {
                return Values[indexs.IndexOf(i)];
            }
        }
        public void Clear()
        {
            indexs = new List<int>();
            Values = new List<SkinnedMeshRenderer>();
        }
    }


    [Button("ResetList", isChangeScene: true)]
    private void Reset()
    {
        if (currentSelects != null)
        {
            foreach (var mesh in currentSelects.Values)
            {
#if UNITY_EDITOR
                if(mesh != null)
                    DestroyImmediate(mesh.gameObject);
#else
                Destroy(mesh.gameObject);
#endif
            }
            currentSelects.Clear();
        }

    }
    private void Awake()
    {
        foreach (var cloth in startPutOn)
        {
            SelectClothes(cloth);
        }
    }
#if UNITY_EDITOR
    private void Select()
    {
        SelectClothes(indexSelect);
    }
    [CreateGUI(title: "Select", group: "SelectClothes",color: ColorsGUI.SuccessGreen)]
    private void SelectClothesGUI()
    {
        foreach(var nameCloth in ClothesParametress.GetNameClothes())
        {
            int currentIndex = ClothesParametress.GetIndexOfName(nameCloth);
            if (GUILayout.Button($"{nameCloth}: " + IsActiveClothes(currentIndex)))
            {
                SelectClothes(currentIndex);
            }
        }
    }
#endif
    public bool IsActiveClothes(int indexClothes)
    {
        if (ClothesParametress.Count - 1 < indexClothes || indexClothes < 0)
        {
            return false;
        }
        if (currentSelects == null)
            currentSelects = new DictionaryClothes();
        if(!currentSelects.ContainsKey(indexClothes))
            return false;
        if (currentSelects[indexClothes] == null)
            return false;
        return currentSelects[indexClothes].gameObject.activeSelf;
    }

    public void SelectClothes(int indexClothes)
    {
        if (ClothesParametress.Count - 1 < indexClothes || indexClothes < 0)
        {
            Debug.LogError("The clothing index is larger than the clothing list");
            return;
        }
        if(currentSelects == null)
            currentSelects = new DictionaryClothes();
        foreach(int opposition in ClothesParametress.GetClothesNotCompatible(indexClothes))
        {
            if (currentSelects.ContainsKey(opposition))
            {
                currentSelects[opposition].gameObject.SetActive(false);
            }
        }
        if(!currentSelects.ContainsKey(indexClothes))
        {
            currentSelects.Add(indexClothes, Initalize(ClothesParametress[indexClothes]));
            return;
        }
        currentSelects[indexClothes].gameObject.SetActive(!currentSelects[indexClothes].gameObject.activeSelf);
        currentSelects[indexClothes].shadowCastingMode = mainMeshRenderer.shadowCastingMode;
    }


    private SkinnedMeshRenderer Initalize(SkinnedMeshRenderer Clothes)
    {
        SkinnedMeshRenderer skinned = GameObject.Instantiate<SkinnedMeshRenderer>(Clothes);
        skinned.bones = mainMeshRenderer.bones;
        skinned.rootBone = mainMeshRenderer.rootBone;
        skinned.transform.SetParent(mainMeshRenderer.transform.parent);
        skinned.shadowCastingMode = mainMeshRenderer.shadowCastingMode;
        return skinned;
    }
}
