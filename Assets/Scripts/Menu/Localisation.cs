using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Localisation
{
    private const string textNone = "{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}{12}{13}{14}{15}{16}{17}{18}{19}{20}";
    public static readonly Dictionary<LText, object> ParseLTextRu = new Dictionary<LText, object>()
    {
        [LText.None] = new string[] { textNone, textNone },
        [LText.ErrorNoneObject] = new string[] { " [Error formaded] ", " [Ошибка форматирования] " },
        [LText.Language] = new string[] { "Language: English ({0})", "Язык: Русский ({0})" },
        [LText.Settings] = "Настройки",
        [LText.True] = new string[] { "On", "Вкл" },
        [LText.False] = new string[] { "Off", "Выкл" },
        [LText.Statistics] = "Статистика",
        [LText.MainMenu] = new string[] { "Main Menu", "Главное меню" },
        [LText.MediaPlayer] = new string[] { "Media Player", "Музыкальный Плеер" },
        [LText.Lobby] = "Холл",
        [LText.Exit] = "Выход",
        [LText.Play] = "Играть",
        [LText.Back] = "Назад",
        [LText.Start_Game] = "Начать игру",
        [LText.Volume] = new string[] { "Volume {0}: {1}", "Громкость {0}: {1}" },
        [LText.Sound] = "Звуков",
        [LText.Music] = "Музыки",
        [LText.Sensitive] = "Чувствительность",
        [LText.Fly] = "Полет",
        [LText.Save] = "Сохранить",
        [LText.Reset] = "Сбросить",
        [LText.Ok] = "Окей",
        [LText.Canсel] = "Отмена",
        [LText.Warning] = "Предупреждение",
        [LText.Error] = "Ошибка",
        [LText.Information] = "Информация",
        [LText.Values] = "Значения",
        [LText.Create] = "Создать",
        [LText.Fox] = "Лиса",
        [LText.Add] = "Добавить",
        [LText.Give] = "Выдать",
        [LText.Item] = "Предмет",
        [LText.Body] = "Тело",
        [LText.Count] = "Количество",
        [LText.Clear] = "Очистить",
        [LText.DebugAnimation] = "ОтладчикАнимации",
        [LText.Leave] = "Оставить",
        [LText.Drop] = "Бросить",
        [LText.Interactive] = "Взаимодействовать",
        [LText.Take] = "Взять",
        [LText.KeyCodeE] = new string[] { "E", "У" },
        [LText.KeyCodeF] = new string[] { "F", "А" },
        [LText.KeyCodeMouse0] = new string[] { "LMB", "ЛКМ" },
        [LText.KeyCodeMouse1] = new string[] { "RMB", "ПКМ" },
    };
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
public enum LText
{
    /// <summary>
    /// имеет 21 местo под объекты
    /// </summary>
    None,
    Null,
    True,
    False,
    ErrorNoneObject,
    Start_Game,
    Language,
    Back,
    Play,
    Settings,
    MainMenu,
    DebugAnimation,
    Lobby,
    Exit,
    Statistics,
    /// <summary>
    /// имеет 1 место под объекты
    /// </summary>
    Volume,
    Count,
    Music,
    Sound,
    Sensitive,
    Fly,
    Save,
    Reset,
    Ok,
    Canсel,
    Information,
    Error,
    Warning,
    MediaPlayer,
    Values,
    Create,
    Fox,
    Item,
    Add,
    Give,
    Body,
    Clear,
    KeyCodeE,
    KeyCodeF,
    KeyCodeMouse0,
    KeyCodeMouse1,
    Drop,
    Leave,
    Take,
    Interactive

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
