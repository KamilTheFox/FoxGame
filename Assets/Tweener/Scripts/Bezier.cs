using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tweener
{
   
    internal class Bezier : Tweener , IExpansionBezier
    {
        public Bezier(BezierWay way, Transform _transform, float _timeOrSpeed, bool _isSpeed) : base(_transform, _timeOrSpeed)
        {
            Way = way;
            isSpeed = _isSpeed;
            eventRestart += RestartWay;
        }
        private bool isSpeed;
        private void RestartWay()
        {
            ProgressLine = 0F;
            ProgressWay = 0F;
        }
        private float ProgressLine = 0F;
        private float ProgressWay = 0F;

        public BezierWay Way;
        public int CountLines => Way.Count;
        protected override string NameOperation => "Bezie";

        public Vector3 CurrentPosition => throw new NotImplementedException();

        public Vector3 CurrentRotation => throw new NotImplementedException();

        protected override void RewriteReverseValue()
        {
            Way.ReverseWay();
        }
        private void DependenceOnSpeed()
        {

        }
        private void DependenceOnTime(float percentage)
        {
            ProgressWay = percentage;
            float progressPrevious = 0F;
            int CurrentLine = Way.GetSegment(percentage);
            for (int i = 1; i < CurrentLine; i++)
                progressPrevious += Way.GetSegmentPercentage(i);
            ProgressLine = (ProgressWay - progressPrevious) / Way.GetSegmentPercentage(CurrentLine);
            transform.position = GetPoint(Way[CurrentLine - 1].Point, Way[CurrentLine - 1].Exit, Way[CurrentLine].Entrance, Way[CurrentLine].Point, ProgressLine);
            transform.rotation = Quaternion.LookRotation(GetDirection(Way[CurrentLine - 1].Point, Way[CurrentLine - 1].Exit, Way[CurrentLine].Entrance, Way[CurrentLine].Point, ProgressLine));
        }
        protected override void OnUpdate(float percentage)
        {
            if (isSpeed)
            {
                Restart();
                DependenceOnSpeed();
            }
            else
                DependenceOnTime(percentage);
        }

        IExpansionBezier IExpansionTween<IExpansionBezier>.ChangeEase(Ease type)
        {
            return (IExpansionBezier)base.ChangeEase(type);
        }

        public new IExpansionBezier ToCompletion(Action action, bool CallWhenDestroy = false)
        {
            return (IExpansionBezier)base.ToCompletion(action, CallWhenDestroy);
        }

        IExpansionBezier IExpansionTween<IExpansionBezier>.ToChanging(Action action)
        {
            return (IExpansionBezier)base.ToChanging(action);
        }

        IExpansionBezier IExpansionTween<IExpansionBezier>.ReverseProgress()
        {
            return (IExpansionBezier)base.ReverseProgress();
        }

        IExpansionBezier IExpansionTween<IExpansionBezier>.ChangeLoop(TypeLoop loop)
        {
            return (IExpansionBezier)base.ChangeLoop(loop);
        }

        public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            t = Mathf.Clamp01(t);
            float oneMinusT = 1f - t;
            return
                oneMinusT * oneMinusT * oneMinusT * p0 +
                3f * oneMinusT * oneMinusT * t * p1 +
                3f * oneMinusT * t * t * p2 +
                t * t * t * p3;
        }

        public static Vector3 GetDirection(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            t = Mathf.Clamp01(t);
            float oneMinusT = 1f - t;
            return
                3f * oneMinusT * oneMinusT * (p1 - p0) +
                6f * oneMinusT * t * (p2 - p1) +
                3f * t * t * (p3 - p2);
        }
    }
}
