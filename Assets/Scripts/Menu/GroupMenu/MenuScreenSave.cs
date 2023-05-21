using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace GroupMenu
{
    public class MenuScreenSave : MainGroup
    {
        public override TypeMenu TypeMenu => TypeMenu.MenuScreenSave;

        private Texture2D textureSave;

        private MenuUI<Image> iconTexture;

        public MenuScreenSave()
        {
            textureSave = null;
            UpdateTexture();
        }

        public MenuScreenSave(Texture2D texture2)
        {
            textureSave = texture2;
            UpdateTexture();
        }
        private void UpdateTexture()
        {
            iconTexture?.SetImage(textureSave);
        }
        protected override void Start()
        {
            //iconTexture = MenuUI<Image>.Find(nameof(iconTexture), GetTransform(), LText.None);
        }
    }
}
