using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;


public static class MenuUIExpansion
{
    public static Button.ButtonClickedEvent OnClick(this MenuUI<Button> button) => button.Component.onClick;
    public static Scrollbar.ScrollEvent OnValueChanged(this MenuUI<Scrollbar> scrollbar) => scrollbar.Component.onValueChanged;
    public static Slider.SliderEvent OnValueChanged(this MenuUI<Slider> slider) => slider.Component.onValueChanged;

    public static void SetMinMax(this MenuUI<Slider> slider, float min, float max) 
        {
        slider.Component.minValue = min;
        slider.Component.maxValue = max;
         }
}
