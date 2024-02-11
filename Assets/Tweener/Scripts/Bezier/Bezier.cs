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
            Way = GameObject.Instantiate(way);
            Way.transform.SetParent(Tween.instance.transform);
            Way.name = way.name + " (Clone for " + NameOperation + ")";
#if UNITY_EDITOR
#pragma warning disable 0618
            Way.Parent = way;
            Way.Visible = false;
            foreach(BezierPoint bezier in Way)
                bezier.Visible = false;
#endif

            isSpeed = _isSpeed;
            if (isSpeed)
            {
                speed = _timeOrSpeed;
                DependenceOnSpeed();
            }
            eventRestart += RestartWay;
        }
        private float speed;

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
        protected override string NameOperation => "Bezier";

        public Vector3 CurrentPosition { get; private set; }

        public Vector3 CurrentRotation { get; private set; }

        protected override void RewriteReverseValue()
        {
            Way.ReverseWay();
        }
        private void DependenceOnSpeed()
        {
                timeScale = Way.DistanceWay / speed;
        }
        private void DependenceOnTime(float percentage)
        {
            ProgressWay = percentage;
            var value = BezierWay.GetPositionInProgress(Way, percentage);

            CurrentPosition = value.Item1;
            CurrentRotation = value.Item2;

            transform.SetPositionAndRotation(CurrentPosition, Quaternion.LookRotation(CurrentRotation));
        }

        protected override void OnUpdate(float percentage)
        {
            if (isSpeed) DependenceOnSpeed();
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
