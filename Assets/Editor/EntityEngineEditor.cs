using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

using Random = UnityEngine.Random;

namespace Assets.Editors
{
    //[CustomEditor(typeof(EntityEngine), true), CanEditMultipleObjects]
    //public class EntityEngineEditor : Editor
    //{
    //    EntityEngine entity;
    //    public void OnEnable()
    //    {
    //        entity = (EntityEngine)target;
    //    }
    //    public Transform transform = null;
    //    public override void OnInspectorGUI()
    //    {
    //        base.OnInspectorGUI();

    //        GUILayout.Label("Stationary: " + entity.Stationary + "; Type Entity: " + entity.typeEntity.ToString());
    //        if (entity is AnimalEngine engine)
    //        {
    //            GUILayout.Label($"TypeAnimal: {engine.TypeAnimal}");
    //            GUILayout.Label($"AI: {engine.NameAI} / Behavior: {engine.Behavior}");
    //            GUILayout.Label($"All Animal: {AnimalEngine.AnimalList.Count}");
    //        }
    //        if (entity is ItemEngine Item)
    //        {
    //            GUILayout.Label($"TypeItem: {Item.itemType}");
    //            if (Item.isController)
    //                GUILayout.Label($"IsPlayerController");
    //            GUILayout.Label($"All Item: {ItemEngine.GetItems.Length}");
    //        }
    //        if (entity is PlantEngine plant)
    //        {
    //            GUILayout.Label($"TypePlant: {plant.typePlant}");
    //            GUILayout.Label($"All Plant: {PlantEngine.GetPlants.Length}");
    //        }
    //    }
    //}
}
