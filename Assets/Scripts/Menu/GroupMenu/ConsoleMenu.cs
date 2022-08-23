using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace GroupMenu
{
    public class ConsoleMenu : MainGroup
    {
        public override TypeMenu TypeMenu => TypeMenu.ConsoleMenu;
        static MenuUI<Text> stackTrace;
        public static void SetStackTrace(string title ,string _stackTrace)
        {
            stackTrace.SetText((title + "\n"  + _stackTrace + "\n" + stackTrace.Text).GetTextUI());
        }
        protected override void Start()
        {
            MenuUI<Button>.Create(GetTransform(), LText.Clear, true).OnClick().AddListener(() => stackTrace.SetText(LText.Null.GetTextUI()));
            stackTrace = MenuUI<Text>.Create(GetTransform(), LText.Null, true, MenuUIAutoRect.SetHeigth(1000));
            stackTrace.Component.alignment = UnityEngine.TextAnchor.UpperLeft;
        }
    }
}
