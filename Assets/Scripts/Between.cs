using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System;

public class Between : MonoBehaviour 
{
    private interface IDisposable
    {
        object Value1 { get; }
        object Value2 { get; }
        public Transform transform { get; }
        bool IsDisposed();
    }
    private enum TypeBetween
    {
        SmoothPosition,
        SmoothRotation,
        SmoothScale,
        LerpPosition,
        LerpRotation,
        LerpScale,
        LerpColor
    }
    private class Parameters : IDisposable 
    {
        public Parameters(Transform _transform, object _OldValue, object _NewValue, float _time, TypeBetween typeBetween)
        {
            transform = _transform;
            OldValue = _OldValue;
            NewValue = _NewValue;
            timeScale = _time;
            percentage = 0f;
            switch (typeBetween)
            {
                case TypeBetween.SmoothPosition:
                    Dispose = SmoothPosition; break;
                case TypeBetween.SmoothRotation:
                    Dispose = SmoothRotation; break;
                case TypeBetween.LerpColor:
                    Dispose = LerpColor; break;
                case TypeBetween.LerpScale:
                    Dispose = LerpScale; break;
                default:
                    Dispose = () => true; break;
            }
        }
        public Transform transform { get; private set; }
        public object Value1 => OldValue;
        public object Value2 => NewValue;

        public object OldValue;
        public object NewValue;
        private readonly float timeScale;
        private float time = 0f;
        private float percentage;
        Func<bool> Dispose;
        private bool LerpColor()
        {
            Color oldColor = (Color)((object[])OldValue)[1];
            Color newColor = (Color)NewValue;

            ((Material)((object[])OldValue)[0]).color = Color.Lerp(oldColor, newColor, GetLerpPercentage());

            return percentage >= 1F;
        }
        private bool LerpScale()
        {
            Vector3 oldScale = (Vector3)OldValue;
            Vector3 newScale = (Vector3)NewValue;

            transform.localScale = Vector3.Lerp(oldScale, newScale, GetLerpPercentage());

            return percentage >= 1F;
        }
        private bool SmoothPosition()
        {
            Vector3 vector1 = (Vector3)OldValue;
            Vector3 vector2 = (Vector3)NewValue;

            transform.position = Vector3.Lerp(vector1, vector2, GetPowDCRPercentage());

            return percentage >= 1F;
        }
        private bool SmoothRotation()
        {
            Vector3 vector1 = (Vector3)OldValue;
            Vector3 vector2 = (Vector3)NewValue;

            transform.rotation = Quaternion.Lerp(Quaternion.Euler(vector1) , Quaternion.Euler(vector2) , GetPowDCRPercentage());

            return percentage >= 1F;
        }
        private float GetLerpPercentage()
        {
            time += Time.deltaTime / timeScale;
            percentage = time;
            return percentage;
        }
        private float GetPowDCRPercentage()
        {
            time += Time.deltaTime / timeScale;
            percentage = MathF.Pow(time, 0.33F);
            return percentage;
        }
        public bool IsDisposed()
        {
            return Dispose.Invoke();
        }
    }


    private static Between instance;
    private static bool Launched;
    private static Dictionary<string, IDisposable> BetweenObjects;
    private void Awake()
    {
        BetweenObjects = new();
        if (instance != null)
        {
            Debug.LogWarning(new TextUI(LText.ErrorInitializeObjects, gameObject.ToString).ToString());
            GameObject.Destroy(gameObject);
        }
        instance = this;
    }
    private IEnumerator Smooth()
    {
        while(BetweenObjects.Count != 0)
        {
            yield return null;
            List<KeyValuePair<string, IDisposable>> disposed = new();
            foreach (KeyValuePair<string, IDisposable> keyValues in BetweenObjects)

                if(keyValues.Value.IsDisposed())

                    disposed.Add(keyValues);

            disposed.ForEach(delete => BetweenObjects.Remove(delete.Key));
        }
        Launched = false;
        yield break;
    }

    private static void StartBetween(Action action)
    {
        if (!instance)
        {
            new GameObject("BetweenObjectMove", typeof(Between));
        }
        action.Invoke();
    }
    public static void SetColorLerp(Transform transform, Color newColor, float time = 1F)
    {
        StartBetween(() =>
        {
            string nameOperator = transform.GetInstanceID() + transform.name + " Color";
            if (BetweenObjects.ContainsKey(nameOperator))
                BetweenObjects.Remove(nameOperator);
            Material material = transform.gameObject.GetComponent<Renderer>().material;
            BetweenObjects.Add(nameOperator, new Parameters(transform,new object[] { material, material.color }, newColor, time, TypeBetween.LerpColor));
            Launch();
        });
    }
    public static void SetPosition(Transform Object, Vector3 newPosition, float  time = 1F)
    {
        StartBetween(() =>
        {
            string nameOperator = Object.GetInstanceID() + Object.name + " Position";
            if (BetweenObjects.ContainsKey(nameOperator))
                BetweenObjects.Remove(nameOperator);
            BetweenObjects.Add(nameOperator, new Parameters(Object, Object.position, newPosition, time, TypeBetween.SmoothPosition));
            Launch();
        });
    }
    public static void SetScaleLerp(Transform Object, Vector3 newScale, float time = 1F)
    {
        StartBetween(() =>
        {
            string nameOperator = Object.GetInstanceID() + Object.name + " Scale";
            if (BetweenObjects.ContainsKey(nameOperator))
                BetweenObjects.Remove(nameOperator);
            BetweenObjects.Add(nameOperator, new Parameters(Object, Object.localScale, newScale, time, TypeBetween.LerpScale));
            Launch();
        });
    }
    public static void SetRotation(Transform Object, Vector3 Euler, float time = 1F)
    {
        StartBetween(() =>
        {
            string nameOperator = Object.GetInstanceID() + Object.name + " Rotation";
            if (BetweenObjects.ContainsKey(nameOperator))
                BetweenObjects.Remove(nameOperator);
            BetweenObjects.Add(nameOperator, new Parameters(Object, Object.rotation.eulerAngles, Euler, time, TypeBetween.SmoothRotation));
            Launch();
        });
    }
    public static void AddRotation(Transform Object, Vector3 Euler, float time = 1F)
    {
        StartBetween(() =>
        {
            string nameOperator = Object.GetInstanceID() + Object.name + " Rotation";
            Vector3 vectorEuler = Euler + Object.rotation.eulerAngles;
            if (BetweenObjects.ContainsKey(nameOperator))
            {
                vectorEuler = Euler + (Vector3)BetweenObjects[nameOperator].Value2;
                BetweenObjects.Remove(nameOperator);
            }
            BetweenObjects.Add(nameOperator, new Parameters(Object, Object.rotation.eulerAngles, vectorEuler, time, TypeBetween.SmoothRotation));
            Launch();
        });
    }
    private static void Launch()
    {
        if (Launched) return;
        instance.StartCoroutine(instance.Smooth());
        Launched = true;
    }
}
