using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace GroupMenu
{
    public class MainMenu : MainGroup
    {
        private static GameObject Instance;
        private static Button LoadPoligon, ExitGameButton;
        public override TypeMenu TypeMenu => TypeMenu.MainMenu;

        public override void Activate()
        {
            
        }

        public override void Deactivate()
        {
            
        }

        public override void Start()
        {
            FindUI();
            ExitGameButton.onClick.AddListener(() => Application.Quit());
            LoadPoligon.onClick.AddListener(
                () => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1)
                );
        }
        
        private void FindUI()
        {
            Instance = Menu.FindUIByPath(nameof(MainMenu), instance.transform);
            LoadPoligon = Menu.FindUIByPath<Button>(nameof(LoadPoligon), Instance.transform);
            ExitGameButton = Menu.FindUIByPath<Button>("ExitGame", Instance.transform);
        }
    }
}
