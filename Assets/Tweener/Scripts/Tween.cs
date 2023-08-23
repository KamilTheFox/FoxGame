using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Tweener
{
    public class Tween : MonoBehaviour
    {
        internal static Tween instance;
        private static bool Launched;

        private void Awake()
        {
            if (instance != null)
            {
                Debug.LogWarning("ErrorInitializeObjects: " + gameObject.name);
                GameObject.Destroy(gameObject);
            }
            instance = this;
        }
        private void OnDestroy()
        {
            Tweener.BetweenObjects.Clear();
            instance = null;
            Launched = false;
        }
        internal bool isPause => Time.timeScale == 0;
        private IEnumerator MainProcess()
        {
            while (Tweener.BetweenObjects.Count != 0)
            {
                if (isPause)
                    yield return new WaitUntil(() => !isPause);
                yield return null;
                List<string> Delete = new();
                foreach (KeyValuePair<string, ITweenable> keyValues in Tweener.BetweenObjects)
                    if (keyValues.Value.IsUsed())
                        Delete.Add(keyValues.Key);
                Delete.ForEach(key =>
                {
                    if (!Tweener.BetweenObjects.ContainsKey(key)) return;
                    ITweenable tweenable = Tweener.BetweenObjects[key];
                    tweenable.OnComplection();
                    if (tweenable.Timer >= 1F)
                        Tweener.BetweenObjects.Remove(key);
                });
            }
            Launched = false;
            yield break;
        }
        #region Color
        public static IExpansionColor SetColor(Transform transform, Color newColor, float time = 1F, TypeComponentChangeColor changeColor = TypeComponentChangeColor.Material)
        {
            return new SetColor(transform, newColor, time, changeColor);
        }
        public static IExpansionColor AddColor(Transform transform, Color newColor, float time = 1F, TypeComponentChangeColor changeColor = TypeComponentChangeColor.Material)
        {
            return new AddColor(transform, newColor, time, changeColor);
        }
        #endregion 
        #region Vector3
        public static IExpansionTween SetScale(Transform Object, Vector3 scale, float time = 1F)
        {
            return new Scale(Object, scale, time, false);
        }
        public static IExpansionTween AddScale(Transform Object, Vector3 scale, float time = 1F)
        {
            return new Scale(Object, scale, time, true);
        }
        public static IExpansionTween SetRotation(Transform Object, Vector3 Euler, float time = 1F)
        {
            return new Rotation(Object, Euler, time, false);
        }
        public static IExpansionTween AddRotation(Transform Object, Vector3 Euler, float time = 1F)
        {
            return new Rotation(Object, Euler, time, true);
        }
        public static IExpansionTween SetPosition(Transform Object, Vector3 position, float time = 1F)
        {
            return new Position(Object, position, time, false);
        }
        public static IExpansionTween AddPosition(Transform Object, Vector3 position, float time = 1F)
        {
            return new Position(Object, position, time, true);
        }

        #endregion 
        #region Bezier

        public static IExpansionBezier GoWay(Transform transform,BezierWay way, float timeOrSpeed, bool isSpeed = false)
        {
            if (way == null)
            {
                Debug.LogWarning("Path not assigned");
                return null;
            }
            return new Bezier(way, transform , timeOrSpeed, isSpeed);
        }

        #endregion 

        private static IExpansionTween ConvertTween(IExpansionTween tween, Action<Tweener> action)
        {
            Tweener tweener = tween as Tweener;
            if (tweener == null && tweener.transform == null) return null;
            action.Invoke(tweener);
            Launch();
            return tween;
        }

        public static IExpansionTween Pause(IExpansionTween tween)
        {
            return ConvertTween(tween, (tweener) =>
            {
                if (Tweener.BetweenObjects.ContainsKey(tweener.NameOperator))
                    Tweener.BetweenObjects.Remove(tweener.NameOperator);
            });
        }
        public static IExpansionTween UnPause(IExpansionTween tween)
        {
            return ConvertTween(tween, (tweener) =>
            {
                if (!Tweener.BetweenObjects.ContainsKey(tweener.NameOperator))
                    Tweener.BetweenObjects.Add(tweener.NameOperator, tweener);
            });
        }
        public static IExpansionTween Stop(IExpansionTween tween, bool OnComplection = false)
        {
            return ConvertTween(tween, (tweener) =>
                {
                    tweener.Restart();
                    if (OnComplection)
                        ((ITweenable)tween).OnComplection();
                    if(Tweener.BetweenObjects.ContainsKey(tweener.NameOperator))
                    Tweener.BetweenObjects.Remove(tweener.NameOperator);
                });
        }
        public static IExpansionTween Start(IExpansionTween tween)
        {
            return ConvertTween(tween, (tweener) =>
            {
                tweener.Restart();
                if (!Tweener.BetweenObjects.ContainsKey(tweener.NameOperator))
                    Tweener.BetweenObjects.Add(tweener.NameOperator, tweener);
            });
        }
        internal static void Launch()
        {
            if (!instance)
            {
                new GameObject("Tweener", typeof(Tween));
            }
            instance.StopAllCoroutines();
            instance.StartCoroutine(instance.MainProcess());
            Launched = true;
        }
    }
}
