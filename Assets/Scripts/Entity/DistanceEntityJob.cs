using System;
using Unity.Collections;
using UnityEngine.Jobs;
using UnityEngine.ParticleSystemJobs;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

public struct DistanceEntityJob : IJobParallelFor
{
    public DistanceEntityJob(NativeArray<InputJobEntityData> _positionsOrders, Vector3 positionCamera, NativeArray<ResultsJobEntity> _resultBooleans)
    {
        positionsOrders = _positionsOrders;
        cameraPosition = positionCamera;
        result = _resultBooleans;
    }

    private NativeArray<InputJobEntityData> positionsOrders;

    private Vector3 cameraPosition;

    private NativeArray<ResultsJobEntity> result;

    public void Execute(int index)
    {
        ResultsJobEntity result = new ResultsJobEntity();
        InputJobEntityData input = positionsOrders[index];

        // Расчет расстояния до камеры
        float distanceToCamera = Vector3.Distance(input.position, cameraPosition);
        result.distanceToCamera = distanceToCamera;

        // Проверка видимости и активности
        result.isVisible = distanceToCamera <= input.maxVisibilityDistance && distanceToCamera >= input.minVisibilityDistance;
        result.isActive = result.isVisible; // Можно изменить логику активности по необходимости
        result.isInOrderDistance = distanceToCamera < input.orderDistance;

        // Вертикальные ограничения
        result.isAboveMaxBottom = input.position.y > input.maxBottomY;
        result.isBelowMaxTop = input.position.y < input.maxTopY;


        result.castShadow = distanceToCamera > input.shadowVisibilityDistance ?  ShadowCastingMode.Off : ShadowCastingMode.On;

        // Расчет LOD
       // result.lodLevel = CalculateLODLevel(distanceToCamera, input.lodDistanceThresholds.ToArray());

        // Расчет угла к камере (если требуется)
        if (input.calculateAngleToCamera)
        {
            Vector3 directionToCamera = (cameraPosition - input.position).normalized;
            result.angleToCamera = Vector3.Angle(directionToCamera, input.rotation * Vector3.forward);
            result.isInViewFrustum = result.angleToCamera <= input.cullAngle;
        }

        this.result[index] = result;
        return;

        // Расчет приоритета и важности
        result.updatePriority = CalculateUpdatePriority(input.basePriority, distanceToCamera);
        result.importanceScore = CalculateImportance(distanceToCamera, input.importanceFactor);

        // Определение необходимости обновления и рендеринга
        result.needsUpdate = result.isActive && result.updatePriority > 0;
        result.needsRender = result.isVisible && result.isInViewFrustum;

        // Настройки теней


        // Дополнительные метрики (пример расчета)
        result.visibilityFactor = CalculateVisibilityFactor(distanceToCamera, input.maxVisibilityDistance);
        result.occlusionFactor = CalculateOcclusionFactor(input.position, cameraPosition); // Потребуется дополнительная логика

        // Оценка производительности (пример)
        result.performanceImpact = CalculatePerformanceImpact(result.lodLevel, result.isVisible);
        result.isBottleneck = result.performanceImpact > 0.8f; // Пороговое значение

        // Диагностика
        result.calculationIterations = 1; // Увеличивайте при сложных вычислениях
       // result.calculationErrorMessage = new NativeArray<char>("test".ToCharArray(),Allocator.TempJob); // Устанавливайте error при ошибках

        
    }

    private int CalculateLODLevel(float distance, float[] thresholds)
    {
        throw new ArgumentNullException();
    }
    private int CalculateUpdatePriority(int basePriority, float distance) {
        throw new ArgumentNullException(); }
    private float CalculateImportance(float distance, float factor) 
    { 
        throw new ArgumentNullException();
        
    }
    private float CalculateVisibilityFactor(float distance, float maxDistance)
    {
        throw new ArgumentNullException();
    }
    private float CalculateOcclusionFactor(Vector3 position, Vector3 cameraPos) 
    {
        throw new ArgumentNullException(); 
    }
    private float CalculatePerformanceImpact(int lodLevel, bool isVisible)
    { 
        throw new ArgumentNullException();
    }

}

public struct ResultsJobEntity
{
    // Базовые флаги
    public bool isVisible;
    public bool isActive;
    public bool isInOrderDistance;
    public bool isAboveMaxBottom;
    public bool isBelowMaxTop;

    // LOD и дистанция
    public int lodLevel;
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

    // Кастомные результаты
    public float customResult1;
    public float customResult2;
    public int customResultFlag;

    // Диагностика
    //public NativeArray<char> calculationErrorMessage;
    public int calculationIterations;
}

public struct InputJobEntityData
{
    // Базовые данные
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;

    // Данные для расчета дистанции и видимости
    public float orderDistance;
    public float maxVisibilityDistance;
    public float minVisibilityDistance;
    public float shadowVisibilityDistance;

    // Данные для вертикальных ограничений
    public float maxTopY;
    public float maxBottomY;

    // Данные для LOD
    // public NativeArray<float> lodDistanceThresholds; // Массив расстояний для каждого уровня LOD

    // Приоритет и важность
    public int basePriority;
    public float importanceFactor;

    // Тип и идентификация
    public TypeEntity entityType;
    public int entityId;

    // Дополнительные параметры для специфических расчетов
    public float cullAngle; // Угол для отсечения по видимости
    public bool isCastingShadow;
    public bool isReceivingShadow;

    // Флаги для опциональных вычислений
    public bool calculateAngleToCamera;
    public bool useCustomLODLogic;

    public InputJobEntityData SetTransform(Transform transform)
    {
        position = transform.position;
        rotation = transform.rotation;
        scale = transform.localScale;
        return this;
    }

}
