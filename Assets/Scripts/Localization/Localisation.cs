using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public partial class Localisation
{
    private const string textNone = "{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}";
    public static Language Language { get; set; }
     public static string GetText(LText text)
    {
        return GetTextDictionary(text, Language);
    }
    private static string GetTextDictionary( LText text, Language language)
    {
        if (text == LText.Null)
            return string.Empty;

        object Text = ParseLTextRu[text];

        if (Text is string newText)
            if (language == Language.En)
                return text.ToString().Replace("_", " ");
            else return newText;

        return (ParseLTextRu[text] as string[])[(int)language];
    }
}

public enum Language
{
    En,
    Ru
}
public static class LTextExpansion
{
    public static LText GetLText(this string Name)
    {
        if(Enum.TryParse(typeof(LText), Name, out object lText))
            return (LText)lText;
        else return LText.None;
    }
    public static TextUI SetFormat(this LText lText, Func<object> Formated)
    {
        return new TextUI(lText, Formated);
    }
    public static TextUI GetTextUI(this string lText)
    {
        return new TextUI(() => lText);
    }
    public static TextUI GetTextUI(this LText lText)
    {
        return new TextUI(lText);
    }
    public static TextUI FormatDelete(this LText lText)
    {
        return new TextUI(lText, () => LText.Null);
    }
    public static LText GetLText(this bool Booleon)
    {
        return Booleon.ToString().GetLText();
    }
}
