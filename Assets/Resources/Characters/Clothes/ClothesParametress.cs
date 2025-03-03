using VulpesTool;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "ClothesParametress", fileName = "Origin")]
public class ClothesParametress : VulpesScriptableObject
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

    public string[] GetNameClothes()
    {
        return names.ToArray();
    }

    [Button("ReadClothes")]
    [ContextMenu("ReadClothes")]
    private void ReadClothes()
    {
        if (clothesPrefab == null)
            return;
        List<SkinnedMeshRenderer> clothesList = new List<SkinnedMeshRenderer>();
        names.Clear();
        foreach (var clothes in clothesPrefab)
        {
            clothesList.AddRange(clothes.GetComponentsInChildren<SkinnedMeshRenderer>());
        }
        names.AddRange(clothesList.Select(cl => cl.name));
        clothes = clothesList.ToArray();
    }
#if UNITY_EDITOR
    [CreateGUI(title: "Clothes Opposition Matrix", color: ColorsGUI.White)]
    private void DrawClothesMatrix()
    {
        if (clothes == null || clothes.Length == 0)
        {
            EditorGUILayout.HelpBox("No clothes found! Please use ReadClothes first!", MessageType.Warning);
            return;
        }

        if (opposition == null)
            opposition = new List<Opposition>();

        // Константы для настройки сетки
        const float CELL_SIZE = 30f;
        const float HEADER_HEIGHT = 120f;
        const float LEFT_MARGIN = 100f;
        const float TOP_MARGIN = 40f;

        // Создаем стили
        var headerStyle = new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleLeft,
            fontSize = 12
        };

        var rotatedStyle = new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleLeft,
            fontSize = 12,
            wordWrap = true
        };

        // Получаем прямоугольник для всей матрицы
        Rect matrixRect = EditorGUILayout.GetControlRect(
            false,
            HEADER_HEIGHT + (clothes.Length * CELL_SIZE) + TOP_MARGIN,
            GUILayout.ExpandWidth(true)
        );

        // Создаем временную матрицу
        bool[,] oppositionMatrix = new bool[clothes.Length, clothes.Length];
        foreach (var opp in opposition)
        {
            int rowIndex = GetIndexOfName(opp.nameCurrentPut);
            if (rowIndex != -1)
            {
                foreach (var notComp in opp.NotCompatible)
                {
                    int colIndex = GetIndexOfName(notComp);
                    if (colIndex != -1)
                    {
                        oppositionMatrix[rowIndex, colIndex] = true;
                        oppositionMatrix[colIndex, rowIndex] = true;
                    }
                }
            }
        }

        // Рисуем вертикальные заголовки
        float startY = matrixRect.y + TOP_MARGIN + HEADER_HEIGHT;
        for (int i = 0; i < clothes.Length; i++)
        {
            Rect labelRect = new Rect(
                matrixRect.x ,
                startY + (i * CELL_SIZE),
                LEFT_MARGIN - 10,
                CELL_SIZE
            );
            EditorGUI.LabelField(labelRect, clothes[i].name, headerStyle);
        }

        // Рисуем горизонтальные (повернутые) заголовки
        for (int i = 0; i < clothes.Length; i++)
        {
            Rect headerRect = new Rect(
                matrixRect.x + LEFT_MARGIN + (i * CELL_SIZE) + 65,
                matrixRect.y + TOP_MARGIN - 20,
                140,
                HEADER_HEIGHT - 20
            );

            GUIUtility.RotateAroundPivot(-90,
                new Vector2(headerRect.x + (CELL_SIZE / 2), headerRect.y + HEADER_HEIGHT));

            EditorGUI.LabelField(headerRect, clothes[i].name, rotatedStyle);
            GUI.matrix = Matrix4x4.identity;
        }

        // Рисуем чекбоксы
        for (int i = 0; i < clothes.Length; i++)
        {
            for (int j = 0; j < clothes.Length; j++)
            {
                if (i <= j && i != j) // Только верхний треугольник, исключая диагональ
                {
                    Rect toggleRect = new Rect(
                        matrixRect.x + LEFT_MARGIN + (j * CELL_SIZE),
                        startY + (i * CELL_SIZE),
                        CELL_SIZE,
                        CELL_SIZE
                    );

                    bool newValue = EditorGUI.Toggle(toggleRect, oppositionMatrix[i, j]);
                    if (newValue != oppositionMatrix[i, j])
                    {
                        oppositionMatrix[i, j] = newValue;
                        oppositionMatrix[j, i] = newValue;
                        UpdateOppositionList(i, j, newValue);
                    }
                }
            }
        }
    }

    // Вспомогательный метод для обновления списка opposition
    private void UpdateOppositionList(int index1, int index2, bool isOpposite)
    {
        string name1 = clothes[index1].name;
        string name2 = clothes[index2].name;

        // Обновляем или создаем запись для первого предмета
        var opp1 = opposition.FirstOrDefault(x => x.nameCurrentPut == name1);
        if (opp1 == null && isOpposite)
        {
            opp1 = new Opposition { nameCurrentPut = name1, NotCompatible = new List<string>() };
            opposition.Add(opp1);
        }

        // Обновляем или создаем запись для второго предмета
        var opp2 = opposition.FirstOrDefault(x => x.nameCurrentPut == name2);
        if (opp2 == null && isOpposite)
        {
            opp2 = new Opposition { nameCurrentPut = name2, NotCompatible = new List<string>() };
            opposition.Add(opp2);
        }

        if (isOpposite)
        {
            // Добавляем несовместимость
            if (!opp1.NotCompatible.Contains(name2))
                opp1.NotCompatible.Add(name2);
            if (!opp2.NotCompatible.Contains(name1))
                opp2.NotCompatible.Add(name1);
        }
        else
        {
            // Удаляем несовместимость
            if (opp1 != null)
                opp1.NotCompatible.Remove(name2);
            if (opp2 != null)
                opp2.NotCompatible.Remove(name1);

            // Удаляем пустые записи
            opposition.RemoveAll(x => x.NotCompatible.Count == 0);
        }
    }
#endif

    [Serializable]
    private class Opposition
    {
        [SerializeField] public string nameCurrentPut;

        [SerializeField] public List<string> NotCompatible;
    }
}
