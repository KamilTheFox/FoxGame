using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

public struct TextUI
{
    public TextUI(LText ltext, Func<object> formader = null)
    {
        Ltext = ltext;
        Formated = formader;
    }
    public TextUI(Func<object> formader)
    {
        Ltext = LText.None;
        Formated = formader;
    }
    public string Text
    {
        get
        {
            string text = Localisation.GetText(Ltext);

            if (((object)Formated) == null)
                Formated = () => null;
                TextFormated(ref text, Formated());
            return text;
        }
    }
    private LText Ltext;
    public Func<object> Formated { private get; set; }
    private void TextFormated(ref string iscomText, object value)
    {

        Regex regex = new Regex("{(\\d*?)}");

        MatchCollection collection = regex.Matches(iscomText);

        if(value is LText check && check == LText.Null)
        {
            foreach (Match match in collection)
                iscomText = iscomText.Replace(match.Value, string.Empty);
        }

        int countToFormat = collection.Count;
        if (value is object[] array)
        {
            List<object> list = array.ToList();
            if (countToFormat == 0)
            list.Add(new TextUI(LText.ErrorNoneObject));
            while (countToFormat > list.Count())
            {
                list.Add(null);
            }
            for (int i = 0; i < list.Count(); i++)
            {
                list[i] = ChangeLTextToTextUI(list[i]);
            }
            value = list.ToArray();
            iscomText = string.Format(iscomText, value as object[]);
            return;
        }
        for (int i = 1; i < collection.Count; i++)
            iscomText = iscomText.Replace(collection[i].Value, string.Empty);
            value = ChangeLTextToTextUI(value);
        if(value == null)
            value = new TextUI(LText.ErrorNoneObject);
            iscomText = string.Format(iscomText, value);
    }
    private object ChangeLTextToTextUI(object value)
    {
        if (value is LText text)
            return new TextUI(text);
        return value;
    }
    public override string ToString()
    {
        return Text;
    }
    public static implicit operator TextUI(string text)
    {
        return text.GetTextUI();
    }
    public static implicit operator TextUI(LText text)
    {
        return text.GetTextUI();
    }
}
