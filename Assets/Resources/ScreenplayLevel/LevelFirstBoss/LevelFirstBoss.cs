﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GroupMenu;
using CameraScripts;
using System.Text;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScreenplayLevel
{
    public class LevelFirstBoss : MonoBehaviour, IHootKeys
    {
        [SerializeField] private byte healthPlayer;
        [SerializeField] private GameObject[] ComponentInterface = new GameObject[0];

        [SerializeField] private Texture2D IconEnemy;

        private MenuUI<Text> WinLoseText;

        public class ThirdPersonViewTop : IViewedCamera
        {
            private PlayerBody Player;
            private CameraControll _camera;

            private GameObject ThirdObject;

            private float DistanceViewCamera = 13F;

            private Vector3 ForvardRotate;

            private Quaternion SmoothRotate = Quaternion.identity;
            public ThirdPersonViewTop(CameraControll camera, PlayerBody _Player)
            {
                Player = _Player;
                _camera = camera;
            }
            public Vector3 RotateBody()
            {
                Vector3 vector = Player.PlayerInput.Velosity;
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
                Player.PlayerInput.ForwardTransform = null;
                GameObject.Destroy(ThirdObject);
            }
            
            public void Construct()
            {
                ThirdObject = new GameObject("ThirdObject");
                Player.PlayerInput.ForwardTransform = ThirdObject.transform;
                _camera.transform.SetParent(null);
                _camera.Transform.localPosition = Vector3.zero;
                _camera.Transform.localEulerAngles = Vector3.zero;
            }
        }
        private class HealthPlayer : IDiesing
        {
            private PlayerBody body;

            UnityAction<int> updateValue;
            public HealthPlayer(PlayerBody player, byte _Health, UnityAction<int> _updateValue)
            {
                body = player;
                Health = _Health;
                updateValue = _updateValue;
            }

            public Transform Transform => body.Transform;

            public bool IsDie => body.IsDie;

            public GameObject gameObject => Transform.gameObject;

            public byte Health;

            public void Death()
            {
                if (IsDie) return;

                Health--;
                updateValue.Invoke(Health);
                if (Health <= 0)
                {
                    body.Die();
                }
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
            CameraControll controll = CameraControll.instance;
            controll.ChangeViewPerson(new ThirdPersonViewTop(controll, controll.CPlayerBody));
            controll.CPlayerBody.UniqueDeathscenario = new HealthPlayer(controll.CPlayerBody, healthPlayer, (count => { imageUI.SetText(count.ToString()); }));
            Menu.instance.hootKeys = this;
            ConstructMenuNone();

            PlayerBody.OnDied += Over;
            EnemyBoss.DeathEnemy += Win;

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