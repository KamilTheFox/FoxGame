using System.Linq;
using UnityEngine;

namespace Tweener
{
    internal class AddColor : SetColor
    {
        public AddColor(Transform _transform, Color color, float _time, TypeComponentChangeColor changeColor = TypeComponentChangeColor.Material) : base(_transform, color, _time, changeColor)
        {
            TweenColors.ToList().ForEach((value) =>
            {
                InfoTweenColor color1 = value.Value;
                Debug.Log(color);
                Color newColor = new Color(color.r, color.g, color.b, color.a > 1F ? 1F : color.a < 0F ? 0F : color.a);
                color1.StrivingColor = (_rewrite ? color1.oldStrivingColor : color1.oldColor) + newColor;
            });
        }
    }
}
