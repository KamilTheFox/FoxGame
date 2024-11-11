using System;
using Unity.Collections;
using UnityEngine.Jobs;
using UnityEngine.ParticleSystemJobs;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

public struct CalculatePropertyRendererJob : IJobParallelFor
{
    public CalculatePropertyRendererJob(NativeArray<InputJobPropertyData> _positionsOrders, Vector3 positionCamera, NativeArray<ResultsJobProperty> _resultBooleans)
    {
        positionsOrders = _positionsOrders;
        cameraPosition = positionCamera;
        result = _resultBooleans;
    }

    private NativeArray<InputJobPropertyData> positionsOrders;

    private Vector3 cameraPosition;

    private NativeArray<ResultsJobProperty> result;

    public void Execute(int index)
    {
        ResultsJobProperty result = new ResultsJobProperty();
        InputJobPropertyData input = positionsOrders[index];

        // Расчет расстояния до камеры
        float distanceToCamera = Vector3.Distance(input.position, cameraPosition);
        result.distanceToCamera = distanceToCamera;

        result.isVisible = distanceToCamera < input.maxVisibilityDistance;

        result.isSpriteVisible = distanceToCamera > input.VisibilitySprite;

        if(result.isSpriteVisible)
        {
            result.rotationSpriteToCamera = Quaternion.LookRotation((cameraPosition - input.position).normalized, Vector3.up);
        }

        // Вертикальные ограничения
        result.isAboveMaxBottom = input.position.y > input.maxBottomY;
        result.isBelowMaxTop = input.position.y < input.maxTopY;

        if (result.isVisible == false)
        {
            this.result[index] = result;
            return;
        }

        // Применить действие на расстоянии
        result.isInActionDistance = distanceToCamera < input.orderDistance;

        result.castShadow = distanceToCamera > input.shadowVisibilityDistance ?  ShadowCastingMode.Off : ShadowCastingMode.On;

        //Расчет LOD
        result.lodLevel = CalculateLODLevel(distanceToCamera, input);

        // Расчет угла к камере (если требуется)
        //if (input.calculateAngleToCamera)
        //{
        //    Vector3 directionToCamera = (cameraPosition - input.position).normalized;
        //    result.angleToCamera = Vector3.Angle(directionToCamera, input.rotation.eulerAngles);
        //    result.isInViewFrustum = result.angleToCamera <= input.cullAngle;
        //}

        //result.isVisible = input.calculateAngleToCamera ? result.isInViewFrustum : result.isVisible;

        this.result[index] = result;
        return; //Недопустимый код сейчас не нужен. Обязательно нужно убать return после реализации ункций ниже
        // Расчет приоритета и важности
        //result.updatePriority = CalculateUpdatePriority(input.basePriority, distanceToCamera);
        //result.importanceScore = CalculateImportance(distanceToCamera, input.importanceFactor);

        //// Определение необходимости обновления и рендеринга
        //result.needsUpdate = result.isActive && result.updatePriority > 0;
        //result.needsRender = result.isVisible && result.isInViewFrustum;

        //// Настройки теней

        //// Дополнительные метрики (пример расчета)
        //result.visibilityFactor = CalculateVisibilityFactor(distanceToCamera, input.maxVisibilityDistance);
        //result.occlusionFactor = CalculateOcclusionFactor(input.position, cameraPosition); // Потребуется дополнительная логика

        //// Оценка производительности (пример)
        //result.performanceImpact = CalculatePerformanceImpact(result.lodLevel, result.isVisible);
        //result.isBottleneck = result.performanceImpact > 0.8f; // Пороговое значение

        //// Диагностика
        //result.calculationIterations = 1; // Увеличивайте при сложных вычислениях

        
    }

    private int CalculateLODLevel(float distance, InputJobPropertyData data)
    {
        return 0;
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



