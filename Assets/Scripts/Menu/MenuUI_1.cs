using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;


public static class MenuUI
{
    public const string PrefixCreate = " (Create)";
    public static Button.ButtonClickedEvent OnClick(this MenuUI<Button> button, Action ActionCleck = null)
    {
        Button.ButtonClickedEvent clickedEvent = button.Component.onClick;
        if (ActionCleck != null)
            clickedEvent.AddListener(ActionCleck.Invoke);
        return clickedEvent;
    }
    public static Scrollbar.ScrollEvent OnValueChanged(this MenuUI<Scrollbar> scrollbar) => scrollbar.Component.onValueChanged;
    public static Slider.SliderEvent OnValueChanged(this MenuUI<Slider> slider) => slider.Component.onValueChanged;

    public static void SetMinMax(this MenuUI<Slider> slider, float min, float max) 
        {
        slider.Component.minValue = min;
        slider.Component.maxValue = max;
         }
    public static Toggle.ToggleEvent OnValueChanged(this MenuUI<Toggle> toggle, Action<bool> ChangeValue = null)
    {
        Toggle.ToggleEvent onValueChanged = toggle.Component.onValueChanged;
        if (ChangeValue != null)
            onValueChanged.AddListener(ChangeValue.Invoke);
        return onValueChanged;
    }
}
