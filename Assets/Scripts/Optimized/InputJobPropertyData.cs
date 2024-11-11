using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public struct InputJobPropertyData
{
    // Базовые данные
    [HideInInspector]
    public Vector3 position;
    [HideInInspector]
    public Quaternion rotation;
    [HideInInspector]
    public Vector3 scale;

    [Header("Данные для расчета дистанции и видимости")]
    public float orderDistance;
    public float maxVisibilityDistance;
    public float VisibilitySprite;
    public float shadowVisibilityDistance;

    [Header("Данные для вертикальных ограничений")]
    public float maxTopY;
    public float maxBottomY;

    [Header("Приоритет и важность")]
    public int basePriority;
    public float importanceFactor;

    [Header("Дополнительные параметры для специфических расчетов")]
    [Tooltip("Угол для отсечения по видимости")]
    public float cullAngle;
    [Tooltip("Пускать ли тень")]
    public bool isCastingShadow;
    [Tooltip("Получать ли тень")]
    public bool isReceivingShadow;

    [Tooltip("Считать ли угол к камере?")]
    public bool calculateAngleToCamera;
    [Tooltip("Считать ли LOD?")]
    public bool useCustomLODLogic;

    // Данные для LOD

    private long LODData;

    public byte GetLODCount()
    {
        return (byte)(LODData & 0xFF);
    }

    public byte GetValueLOD(byte index)
    {
        if (index >= 7) throw new ArgumentOutOfRangeException("index Max levelLOD Length array 7");

        if (index >= GetLODCount()) throw new ArgumentOutOfRangeException("index Out Of Range levelLOD Length array");

        return (byte)((LODData >> 8 * index) & 0xFF);
    }

    public InputJobPropertyData SetArrayLODs(byte[] levelLOD)
    {
        if (levelLOD.Length >= 7) throw new ArgumentOutOfRangeException(nameof(levelLOD) + " Max Length array 7");

        long lodData = levelLOD.Length; // Первый байт - количество уровней LOD

        for (int i = 0; i < levelLOD.Length; i++)
        {
            lodData |= (long)levelLOD[i] << (8 * (i + 1));
        }

        this.LODData = lodData;

        return this;
    }

    public InputJobPropertyData SetTransform(Transform transform)
    {
        position = transform.position;
        rotation = transform.rotation;
        scale = transform.localScale;
        return this;
    }

    public InputJobPropertyData SetCullAngle(float angle)
    {
        calculateAngleToCamera = true;
        cullAngle = angle;
        return this;
    }

}
