using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

static class GameState
    {
    public static TypeModeGame TypeGame { get; private set; } = TypeModeGame.Creative;

    public static event Action LoadNewScene;
    public enum TypeModeGame
    {
        MainMenu,
        Adventure,
        Creative
    }
    public static bool IsCreative => TypeGame == TypeModeGame.Creative;
    public static bool IsAdventure => TypeGame == TypeModeGame.Adventure;

    public static void StartGame(TypeModeGame modeGame,  int IDScene = 0)
    {
        LoadNewScene?.Invoke();
        LoadNewScene = null;
        TypeGame = modeGame;
        if (TypeModeGame.MainMenu == modeGame)
            IDScene = 0;
        SceneManager.LoadScene(IDScene);
        
    }
}
