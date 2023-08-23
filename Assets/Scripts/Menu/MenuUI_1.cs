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

    private static Dictionary<string, Sprite> keyValueImage;
    public static void SetImageMenu(Image image, string Name)
    {
        if (!image) return;

        if(keyValueImage == null)
        {
            keyValueImage = new ();
            List<Sprite> images = Resources.LoadAll<Sprite>($"Menu\\{Settings.NameSkinMenu}").ToList();
            images.ForEach(image =>
            {
                foreach(string localName in image.name.Split('.'))
                {
                    keyValueImage.Add(localName, image);
                }
            });
        }
        if(keyValueImage.TryGetValue(Name, out Sprite sprite))
        {
            image.sprite = sprite;
        }
    }
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
