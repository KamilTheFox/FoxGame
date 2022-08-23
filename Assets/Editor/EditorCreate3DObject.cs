using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using FactoryLesson;
using Random = UnityEngine.Random;

public class EditorCreate3DObject : EditorWindow
{
    public static Type TypeEntity;
    public static EditorWindow This;

    [MenuItem("CONTEXT/Transform/RandomScale")]
    public static void RandomScale(MenuCommand command)
    {
        Transform _object = (Transform)command.context;
        Undo.RecordObject(_object, _object.name);
        _object.localScale = Vector3.one * Random.Range(0.2F, 2F);
    }

    [MenuItem("GameObject/3D Object/Item")]
    public static void CreateItems()
    {
        This = GetWindow<EditorCreate3DObject>("Create Item");
        TypeEntity = typeof(TypeItem);
    }
    [MenuItem("GameObject/3D Object/Plants")]
    public static void CreatePlants()
    {
        This = GetWindow<EditorCreate3DObject>("Create Plant");
        TypeEntity = typeof(TypePlant);
    }
    [MenuItem("GameObject/3D Object/Animal")]
    public static void CreateAnimals()
    {
        This = GetWindow<EditorCreate3DObject>("Create Animal");
        TypeEntity = typeof(TypeAnimal);
    }
    private float Scroll;
    private void OnGUI()
    {
        Array array = Enum.GetValues(TypeEntity);
        Scroll =GUI.VerticalScrollbar(new Rect(0,0, 20, This.position.height), Scroll, 1F, 0F, This.position.height + 1F);
        for(int i =0; i < array.Length; i++)
        {
            if ( GUI.Button(new Rect(20, - Scroll + i * 25F, This.position.width - 20F, 20), array.GetValue(i).ToString()) )
                {
                CreateEntity((Enum)Enum.Parse(TypeEntity, array.GetValue(i).ToString()));
                This.Close();
                };
        }
    }
    private static void CreateEntity(Enum Entity)
    {
        EntityFactory.GetEntity(Entity,Vector3.zero, Quaternion.identity);
    }
}
