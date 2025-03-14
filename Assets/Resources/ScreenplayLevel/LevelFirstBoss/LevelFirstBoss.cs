﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GroupMenu;
using CameraScripts;
using System.Text;
using UnityEngine.Events;
using UnityEngine.UI;
using PlayerDescription;

namespace ScreenplayLevel
{
    public class LevelFirstBoss : MonoBehaviour, IHootKeys
    {
        [SerializeField] private byte healthPlayer;
        [SerializeField] private GameObject[] ComponentInterface = new GameObject[0];

        [SerializeField] private Texture2D IconEnemy;

        [SerializeField] private CharacterMediator Player;

        private MenuUI<Text> WinLoseText;

        public class ThirdPersonViewTop : IViewedCamera, IInputCharacter
        {
            private CharacterMediator Player;
            private CameraControll _camera;

            private GameObject ThirdObject;

            private float DistanceViewCamera = 13F;

            private Vector3 ForvardRotate;

            private Quaternion SmoothRotate = Quaternion.identity;
            public ThirdPersonViewTop(CameraControll camera, CharacterMediator _Player)
            {
                Player = _Player;
                camera.EntranceBody(Player.gameObject);
                _camera = camera;
            }
            public Vector3 RotateBody()
            {
                Vector3 vector = Player.Motor.Direction;
                if (vector != Vector3.zero)
                {
                    ForvardRotate = SmoothRotate.eulerAngles;
                    SmoothRotate = Quaternion.Lerp(SmoothRotate,Quaternion.LookRotation(new Vector3(vector.x, 0, vector.z)), Time.fixedDeltaTime * 3);
                }
                return ForvardRotate;
            }
            public Vector2 ViewAxisMaxVertical => new Vector2(-85, -45);

            public void ViewAxis(Transform camera, Vector3 euler)
            {
                camera.localEulerAngles = euler;
                float dir = Input.GetAxis("Mouse ScrollWheel");
                if (!ViewInteractEntity.isMoveItem && DistanceViewCamera + dir < 20F && DistanceViewCamera + dir > 7F)
                {
                    DistanceViewCamera += dir;
                }
                Ray ray = new Ray(Player.transform.position, -camera.forward);
                Vector3 position = ray.GetPoint(DistanceViewCamera);
                _camera.Transform.position = position;
                ThirdObject.transform.rotation = Quaternion.LookRotation(new Vector3(camera.forward.x, 0, camera.forward.z));
            }

            public float DistanceView => 3F + DistanceViewCamera;
            public void Dispose()
            {
                _camera.transform.SetParent(Player.Transform);
                Player.Motor.ForwardTransform = null;
                GameObject.Destroy(ThirdObject);
            }
            
            public void Construct()
            {
                ThirdObject = new GameObject("ThirdObject");
                Player.Motor.ForwardTransform = ThirdObject.transform;
                Player.Body.ClearInteractEntity();
                _camera.transform.SetParent(null);
                _camera.Transform.localPosition = Vector3.zero;
                _camera.Transform.localEulerAngles = Vector3.zero;
            }

            public bool IsRun => Input.GetKey(KeyCode.LeftShift);

            public bool IsCrouch => false;

            bool IInputCharacter.Space()
            {
                return Player.Motor.isSwim ? Input.GetKey(KeyCode.Space) : Input.GetKeyDown(KeyCode.Space);
            }

            bool IInputCharacter.Shift()
            {
                return IsRun;
            }

            Vector3 IInputCharacter.Move(Transform source, out bool isMove)
            {
                return MovementMode.WASD(source, 1F, out isMove, true);
            }
        }
        private class HealthPlayer : IDiesing
        {
            private CharacterMediator player;

            UnityAction<int> updateValue;
            public HealthPlayer(CharacterMediator _player, byte _Health, UnityAction<int> _updateValue)
            {
                player = _player;
                Health = _Health;
                updateValue = _updateValue;
            }

            public Transform Transform => player.Transform;

            public bool IsDie => player.Body.IsDie;

            public GameObject gameObject => Transform.gameObject;

            public byte Health;

            public void Death()
            {
                if (IsDie) return;
                player.Body.Fell();
                Health--;
                updateValue.Invoke(Health);
                if (Health <= 0)
                {
                    player.Body.Die();
                }
                else
                {
                    player.Body.StartCoroutine(NotFell());
                }
            }
            private IEnumerator NotFell()
            {
                yield return new WaitForSeconds(1F);
                player.Body.StendUp();
                yield break;
            }
        }

        MenuUI<Image> imageUI;
        public static MenuUI<Image> imageUIEnemy;

        private void ConstructMenuNone()
        {
            None.EnableAim(false);
            None.EnableInfoEntity(false);
            imageUI = MenuUI<Image>.Create("ImageHealthPlayer", Menu.Find("None").transform, LText.None, false, (rect) => { return new Rect(10, 10, 100, 100); });
            MenuUI<Text> texti = MenuUI<Text>.Create("ImageHealthPlayerText", imageUI.Component.transform, LText.None, false, (rect) => { return new Rect(0, 10, 100, 100); });
            texti.Component.alignment = TextAnchor.UpperCenter;
            texti.isUpdateText = false;
            imageUIEnemy = MenuUI<Image>.Create("ImageHealthEnemy", Menu.Find("None").transform, LText.None, false, (rect) => { return new Rect(10, 10, 100, 100); });
            var texty = MenuUI<Text>.Create("ImageHealthEnemyText", imageUIEnemy.Component.transform, LText.None, false, (rect) => { return new Rect(0, 10, 100, 100); });
            texty.isUpdateText = false;
            Text textComponent = texty.Component;
            textComponent.alignment = TextAnchor.UpperCenter;
            imageUIEnemy.Component.rectTransform.anchorMin = Vector2.one;
            imageUIEnemy.Component.rectTransform.anchorMax = Vector2.one;
            imageUIEnemy.Component.rectTransform.pivot = Vector2.one * 0.5F;
            imageUIEnemy.Component.rectTransform.anchoredPosition = new Vector2(-110, -55);

            imageUI.SetImage("FoxIcon");
            imageUI.SetText(healthPlayer.ToString());
            imageUIEnemy.SetImage(IconEnemy);
            imageUIEnemy.SetText(EnemyBoss.Instance.countHealth.ToString());

            None.OnActivate += UIThisLevel;
        }
        public void Start()
        {
            Menu.CurrentPauseMenu = new MenuWinLose();
            CameraControll controll = CameraControll.Instance;
            var inputView = new ThirdPersonViewTop(controll, Player);
            controll.ChangeViewPerson(inputView);
            Player.Motor.SetInputCharacter(inputView);
            Player.Body.UniqueDeathscenario = new HealthPlayer(Player, healthPlayer, (count => { imageUI.SetText(count.ToString()); }));
            Menu.instance.hootKeys = this;
            ConstructMenuNone();

            Player.Body.OnDied += Over;
            Player.Body.OnFell += Player_OnFell;
            EnemyBoss.DeathEnemy += Win;

        }

        private void Player_OnFell()
        {
            EnemyBoss.SetMockery = true;
            foreach (var tnt in FindObjectsOfType<Detonator>())
            {
                tnt.gameObject.SetActive(false);
            }
        }

        private void Win()
        {
            Menu.ActivateMenu<MenuWinLose>();
            MenuWinLose.SetTextTitle("You WIN");
        }
        private void Over()
        {
            EnemyBoss.DeathEnemy -= Win;
            Menu.ActivateMenu<MenuWinLose>();
            MenuWinLose.SetTextTitle("Game Over");
            Menu.PauseEnableGame(false);
        }
        private void UIThisLevel(bool isActivate)
        {
            imageUI.gameObject.SetActive(isActivate);
            imageUIEnemy.gameObject.SetActive(isActivate);
        }
        public void Action()
        {
            
        }
    }
}
