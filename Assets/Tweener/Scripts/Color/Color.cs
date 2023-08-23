using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tweener
{
    internal class SetColor : Tweener, IExpansionColor
    {
        protected class InfoTweenColor
        {
            public bool isCurrentObject;
            public bool isChildObject;
            public bool isHierarchyObject;
            public Color oldColor;
            public Color StrivingColor;
            public Color oldStrivingColor;
            public InfoTweenColor SetBoolHierarchy(Transform thisTransform,Transform thatTransform)
            {
                isChildObject = thisTransform == thatTransform.parent;
                isCurrentObject = thisTransform == thatTransform;
                isHierarchyObject = thisTransform != thatTransform.parent && thisTransform != thatTransform;
                return this;
            }
        }
        protected class InfoTweenColor<T> : InfoTweenColor
        {
            public T component;
            }
        protected override string NameOperation => "Color";

        protected Dictionary<string, InfoTweenColor> TweenColors = new();
        private Color strivingColor;
        private bool oldTweenIsReverse;
        protected bool _rewrite;
        List<IgnoreARGB> ignores = new();
        TypeChangeColor typeChangeColor;


        public SetColor(Transform _transform, Color color, float _time, TypeComponentChangeColor typeChange = TypeComponentChangeColor.Material) : base(_transform, _time)
        {
            strivingColor = color;
            BuildTweenColor(color, typeChange);
        }
        private void BuildTweenColor(Color color, TypeComponentChangeColor typeChange)
        {
            if (typeChange == TypeComponentChangeColor.Light || typeChange == TypeComponentChangeColor.All)
            {
                FindLight(color);
            }
            if (typeChange == TypeComponentChangeColor.Material || typeChange == TypeComponentChangeColor.All)
            {
                FindMaterial(color);
            }
        }
        private void FindLight(Color color)
        {
            foreach (Light light in transform.gameObject.GetComponentsInChildren<Light>())
            {
                if (light == null)
                    continue;
                string name = light.name + light.GetInstanceID() + " tag_light";

                TweenColors.Add(name, new InfoTweenColor<Light>()
                {
                    component = light,
                    StrivingColor = color,
                    oldColor = light.color,
                }.SetBoolHierarchy(transform, light.transform));
            }
        }
        private void FindMaterial(Color color)
        {
            foreach (Renderer renderer in transform.gameObject.GetComponentsInChildren<Renderer>())
            {
                Material[] materials = renderer.materials;
                if (materials == null)
                    continue;
                materials.ToList().ForEach(material =>
                {
                    if (material.color.a != color.a)
                        material.ToFadeMode();
                    string name = material.name + material.GetInstanceID() + " tag_material";

                    TweenColors.Add(name, new InfoTweenColor<Material>()
                    {
                        component = material,
                        StrivingColor = color,
                        oldColor = material.color,
                    }.SetBoolHierarchy(transform, renderer.transform));
                });
            }
        }
        protected override void Rewrite(ITweenable tweenable)
        {
            _rewrite = true;
            SetColor set = (SetColor)tweenable;
            set.TweenColors.ToList().ForEach(color =>
            {
                TweenColors[color.Key].oldStrivingColor = color.Value.StrivingColor;
            });
            oldTweenIsReverse = set.reverseProgress;
        }
        protected override void RewriteReverseValue()
        {
            TweenColors.Values.ToList().ForEach(x =>
            {
                ReverseValues(ref x.oldColor, ref x.StrivingColor);
                if (oldTweenIsReverse)
                    x.StrivingColor = x.oldStrivingColor;
            });
        }
        private Color ConvertColorInIgnore(bool[] ignors, Color strivingColor, Color Default)
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
            foreach (KeyValuePair<string, InfoTweenColor> tween in TweenColors)
            {

                if (!tween.Value.isCurrentObject && typeChangeColor == TypeChangeColor.CurrentObject)
                    continue;

                if (!tween.Value.isChildObject && typeChangeColor == TypeChangeColor.Childs)
                    continue;

                if (!(tween.Value.isChildObject || tween.Value.isCurrentObject) && typeChangeColor == TypeChangeColor.ObjectAndChilds)
                    continue;

                Color strivingColor = tween.Value.StrivingColor;
                Color oldValueColor = tween.Value.oldColor;
                bool[] Ignor = new bool[]
                    {
                        ignores.Contains(IgnoreARGB.A),
                        ignores.Contains(IgnoreARGB.R),
                        ignores.Contains(IgnoreARGB.G),
                        ignores.Contains(IgnoreARGB.B)
                    };
                Color Material = Color.white;
                if (tween.Key.Contains("tag_material"))
                {
                    Material = ((InfoTweenColor<Material>)tween.Value).component.color;
                }
                else if (tween.Key.Contains("tag_light"))
                {
                    Material = ((InfoTweenColor<Light>)tween.Value).component.color;
                }

                if (reverseProgress)
                    oldValueColor = ConvertColorInIgnore(Ignor, oldValueColor, Material);
                else
                    strivingColor = ConvertColorInIgnore(Ignor, strivingColor, Material);


                Color newColor = Color.Lerp(oldValueColor, strivingColor, percentage);

                if (tween.Key.Contains("tag_material"))
                {
                    ((InfoTweenColor<Material>)tween.Value).component.color = newColor;
                }
                else if (tween.Key.Contains("tag_light"))
                {
                    ((InfoTweenColor<Light>)tween.Value).component.color = newColor;
                }
            }

        }
        #region InterfaceUpCast
        public new IExpansionColor ChangeEase(Ease type)
        {
            return (IExpansionColor)base.ChangeEase(type);
        }

        public new IExpansionColor ToCompletion(Action action, bool CallWhenDestroy)
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
        #endregion

        public IExpansionColor TypeOfColorChange(TypeChangeColor type)
        {
            typeChangeColor = type;
            return this;
        }
        public IExpansionColor IgnoreAdd(IgnoreARGB ARGB)
        {
            switch (ARGB)
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

        public IExpansionColor TypeComponentChange(TypeComponentChangeColor typeComponent)
        {
            Restart();
            if (reverseProgress)
                ReverseProgress();
            OnUpdate(0F);
            TweenColors = new();
            BuildTweenColor(strivingColor, typeComponent);
            return this;
        }
    }
   
    
    
    
}
