using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tweener
{
    internal abstract class Tweener : ITweenable, IExpansionTween
    {
        public Tweener(Transform _transform, float _time)
        {
            transform = _transform;
            timeScale = _time;
            percentage = 0f;
            ChangeEase(Ease.Linear);
            if (BetweenObjects.TryGetValue(NameOperator, out ITweenable tweenable))
            {
                Rewrite(tweenable);
                tweenable.OnChange();
                ((Tweener)tweenable).time = 0f;
                BetweenObjects.Remove(NameOperator);
            }
            BetweenObjects.Add(NameOperator, this);
            Tween.Launch();
        }
        public string NameOperator => $"{transform.GetInstanceID()}:{transform.name} [{NameOperation}]";

        public static Dictionary<string, ITweenable> BetweenObjects = new();

        private Func<float, float> Progress;
        public Transform transform { get; private set; }
        protected abstract string NameOperation { get; }

        public readonly float timeScale;
        private float time = 0f;

        public float Timer => time;

        public TypeLoop typeLoop;
        public Ease typeEase;
        public bool reverseProgress { get; private set; }
        private float percentage;

        private event Action toCompletion;
        private Action toChanging;

        protected event Action eventRestart;
        protected event Action eventStop;
        protected event Action EventDestroyObject;

        private static readonly Dictionary<Ease, Ease> EaseReverse = new()
        {
            [Ease.CubicRoot] = Ease.CubicDegree,
            [Ease.CubicDegree] = Ease.CubicRoot,
            [Ease.SquareRoot] = Ease.SquareDegree,
            [Ease.SquareDegree] = Ease.SquareRoot,
        };
        private float GetTimeSkale
        {
            get
            {
                float time = Time.deltaTime / timeScale;
                if (time == 0F)
                    return 1F;
                if (time < 0F)
                {
                    time *= -1F;
                    if (!reverseProgress)
                        reverseProgress = true;
                }
                return time;
            }
        }
        internal void Restart()
        {
            eventRestart?.Invoke();
            time = 0F;
            percentage = 0F;
        }
        internal void Stop()
        {
            eventStop?.Invoke();
            time = 1F;
            percentage = 1F;
        }
        protected virtual void Rewrite(ITweenable tweenable) { }

        protected abstract void RewriteReverseValue();
        public static void ReverseValues<T>(ref T value1, ref T value2)
        {
            T value = value1;
            value1 = value2;
            value2 = value;
        }
        private float GetProgress()
        {
            time += GetTimeSkale;
            percentage = Progress(time);
            return percentage;
        }
        protected abstract void OnUpdate(float percentage);
        bool ITweenable.IsUsed()
        {
            if (transform == null)
            {
                EventDestroyObject.Invoke();
                return true;
            }

            OnUpdate(GetProgress());
            bool IsCompletion = time >= 1F;
            return IsCompletion;
        }
        void ITweenable.OnComplection()
        {
            toCompletion?.Invoke();
            if (typeLoop == TypeLoop.None)
                toCompletion = null;
        }
        public IExpansionTween ChangeEase(Ease type)
        {
            typeEase = type;
            Progress = type switch
            {
                Ease.Linear => (time) => time,
                Ease.CubicRoot => (time) => Mathf.Pow(time, 0.33F),
                Ease.SquareRoot => (time) => Mathf.Pow(time, 0.5F),
                Ease.SquareDegree => (time) => Mathf.Pow(time, 2F),
                Ease.CubicDegree => (time) => Mathf.Pow(time, 3F),
                Ease.FourthDegree => (time) => Mathf.Pow(time, 4F),
                Ease.FiveDegree => (time) => Mathf.Pow(time, 5F),

                Ease.InJumps => (time) =>
                {
                      float value = time;

                      return value;
                },
                _ => (time) => { return 1F; },

            };
            return this;
        }
        public IExpansionTween ToCompletion(Action action, bool CallWhenDestroy = false)
        {
            toCompletion += action;
            if (!CallWhenDestroy)
                EventDestroyObject += () =>
                {
                    Debug.LogWarning("The object did not finish the tween, To completion was not called. To call it, activate the CallWhenDestroy parameter");
                    toCompletion = null;

                };
            return this;
        }

        public IExpansionTween ReverseProgress()
        {
            reverseProgress = !reverseProgress;
            RewriteReverseValue();
            return this;
        }

        public void OnChange()
        {
            toChanging?.Invoke();
        }

        public IExpansionTween ToChanging(Action action)
        {
            toChanging += action;
            return this;
        }
        private void Loop()
        {
            Restart();
        }
        private void PingPong()
        {
            Restart();
            ReverseProgress();
        }
        private void PingPongReverseEase()
        {
            Restart();
            ReverseProgress();
            if (EaseReverse.TryGetValue(typeEase, out Ease ease))
                typeEase = ease;
            ChangeEase(typeEase);
        }
        public IExpansionTween ChangeLoop(TypeLoop loop)
        {
            typeLoop = loop;
            switch(loop)
            {
                case TypeLoop.None:
                    toCompletion -= Loop;
                    toCompletion -= PingPong;
                    toCompletion -= PingPongReverseEase;
                    break;
                case TypeLoop.PingPong:
                    toCompletion -= Loop;
                    toCompletion += PingPong;
                    toCompletion -= PingPongReverseEase;
                    break;
                case TypeLoop.Loop:
                    toCompletion += Loop;
                    toCompletion -= PingPongReverseEase;
                    toCompletion -= PingPong;
                    break;
                case TypeLoop.PingPongReverseEase:
                    toCompletion -= Loop;
                    toCompletion -= PingPong;
                    toCompletion += PingPongReverseEase;
                    break;

            }
            return this;
        }
    }

}
