using UnityEngine.Rendering;
using UnityEngine;

public struct ResultsJobProperty
{
    // Базовые флаги
    public bool isVisible;
    public bool isSpriteVisible;
    public bool isActive;
    public bool isInActionDistance;
    public bool isAboveMaxBottom;
    public bool isBelowMaxTop;

    // LOD и дистанция
    public int lodLevel;
    public bool useLODlogics;
    public float distanceToCamera;

    // Угловые расчеты
    public float angleToCamera;
    public bool isInViewFrustum;

    // Приоритеты и важность
    public int updatePriority;
    public float importanceScore;

    // Флаги состояния
    public bool needsUpdate;
    public bool needsRender;

    // Тени
    public ShadowCastingMode castShadow;

    // Дополнительные метрики
    public float visibilityFactor; // 0-1, насколько хорошо объект виден
    public float occlusionFactor; // 0-1, насколько объект закрыт другими объектами

    // Производительность
    public bool isBottleneck; // Флаг, если объект влияет на производительность
    public float performanceImpact; // Оценка влияния на производительность

    public Quaternion rotationSpriteToCamera;

    // Диагностика
    public int calculationIterations;
}
