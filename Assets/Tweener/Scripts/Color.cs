using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using UnityEngine;

namespace Tweener
{
    internal class SetColor : Tweener, IExpansionColor
    {
        protected override string NameOperation => "Color";
        protected Dictionary<string,Color> oldColor = new();
        protected Dictionary<string, bool> isChildObject = new();
        protected Dictionary<string, Color> StrivingColor = new();
        protected Dictionary<string, Color> oldStrivingColor = new();
        private bool oldTweenIsReverse;
        protected bool _rewrite;
        readonly Dictionary<string, Material> materials = new();
        List<IgnoreARGB> ignores = new();
        TypeChangeColor typeChangeColor;
        public SetColor(Transform _transform, Color color, float _time) : base(_transform, _time)
        {
            foreach(Renderer renderer in transform.gameObject.GetComponentsInChildren<Renderer>())
            {
                Material mat = renderer.material;
                if(mat.color.a != color.a)
                    mat.ToFadeMode();
                if (mat == null) continue;
                string name = mat.name + mat.GetInstanceID();
                isChildObject.Add(name, transform == renderer.transform.parent || transform == renderer.transform);
                materials.Add(name, mat);
                oldColor.Add(name, mat.color);
                StrivingColor.Add(name, color);
            }
        }
        protected override void Rewrite(ITweenable tweenable)
        {
            _rewrite = true;
            SetColor set = (SetColor)tweenable;
            oldStrivingColor = set.StrivingColor;
            oldTweenIsReverse = set.reverseProgress;
        }
        private Color ConvertColorInIgnore(bool[] ignors,Color strivingColor, Color Default)
        {
            return new Color(
                    ignors[1] ? Default.r : strivingColor.r,
                    ignors[2] ? Default.g : strivingColor.g,
                    ignors[3] ? Default.b : strivingColor.b,
                    ignors[0] ? Default.a : strivingColor.a
                            );
        }
        protected override void OnUpdate(float percentage)
        {
            foreach (KeyValuePair<string, Color> color in oldColor)
            {
                if (typeChangeColor == TypeChangeColor.ObjectAndChilds && !isChildObject[color.Key])
                    continue;
                Material mat = materials[color.Key];
                Color strivingColor = StrivingColor[color.Key];
                Color oldValueColor = color.Value;
                bool[] Ignor = new bool[]
                    {
                        ignores.Contains(IgnoreARGB.A),
                        ignores.Contains(IgnoreARGB.R),
                        ignores.Contains(IgnoreARGB.G),
                        ignores.Contains(IgnoreARGB.B)
                    };
                Color Material = materials[color.Key].color;
                if(reverseProgress)
                    oldValueColor = ConvertColorInIgnore(Ignor, oldValueColor, Material);
                else
                    strivingColor = ConvertColorInIgnore(Ignor, strivingColor, Material);
                mat.color = Color.Lerp(oldValueColor, strivingColor, percentage);
                if (typeChangeColor == TypeChangeColor.CurrentObject)
                    break;
            }

        }

        public new IExpansionColor ChangeEase(Ease type)
        {
            return (IExpansionColor)base.ChangeEase(type);
        }

        public new IExpansionColor ToCompletion(Action action,bool CallWhenDestroy)
        {
            return (IExpansionColor)base.ToCompletion(action, CallWhenDestroy);
        }

        public new IExpansionColor ReverseProgress()
        {
            return (IExpansionColor)base.ReverseProgress();
        }

        public new IExpansionColor ToChanging(Action action)
        {
            return (IExpansionColor)base.ToChanging(action);
        }

        public new IExpansionColor ChangeLoop(TypeLoop loop)
        {
            return (IExpansionColor)base.ChangeLoop(loop);
        }


        public IExpansionColor TypeOfColorChange(TypeChangeColor type)
        {
            typeChangeColor = type;
            return this;
        }
        public IExpansionColor IgnoreAdd(IgnoreARGB ARGB)
        {
            switch(ARGB)
            {
                case IgnoreARGB.RGB:
                    ignores.Add(IgnoreARGB.R);
                    ignores.Add(IgnoreARGB.G);
                    ignores.Add(IgnoreARGB.B);
                    break;
                case IgnoreARGB.RG:
                    ignores.Add(IgnoreARGB.R);
                    ignores.Add(IgnoreARGB.G);
                    break;
                case IgnoreARGB.RB:
                    ignores.Add(IgnoreARGB.R);
                    ignores.Add(IgnoreARGB.B);
                    break;
                case IgnoreARGB.GB:
                    ignores.Add(IgnoreARGB.G);
                    ignores.Add(IgnoreARGB.B);
                    break;
                default:
                    ignores.Add(ARGB);
                    break;
            }
            return this;
        }

        protected override void RewriteReverseValue()
        {
            ReverseValues(ref oldColor, ref StrivingColor);
            if (oldTweenIsReverse)
                StrivingColor = oldStrivingColor;
        }

       
    }
    internal class AddColor : SetColor
    {
        public AddColor(Transform _transform, Color color, float _time) : base(_transform, color, _time)
        {
            StrivingColor.ToList().ForEach((value) =>
            {
                Debug.Log(color);
                Color newColor = new Color(color.r, color.g, color.b, color.a > 1F? 1F : color.a < 0F? 0F : color.a );
                StrivingColor[value.Key] = (_rewrite? oldStrivingColor[value.Key] : oldColor[value.Key]) + newColor;
                Debug.Log(StrivingColor[value.Key]);
            });
        }
    }
    public enum TypeChangeColor : byte
    {
        CurrentObject,
        ObjectAndChilds,
        ObjectAndHierarchy,
    }
    public enum IgnoreARGB : byte
    {
        A,R,G,B,
            RGB,
            RG,
            GB,
            RB
    }
}
