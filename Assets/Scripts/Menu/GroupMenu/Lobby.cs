using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using PlayerDescription;
using System.Collections.Generic;

namespace GroupMenu
{
    public class Lobby : MainGroup
    {
        private static MenuUI<Button> Button_Fly;

        private static MenuUI<Text> CountItems;

        private static TypeItem typeItem;

        private static TypePlant typePlant;
        public override TypeMenu TypeMenu => TypeMenu.Lobby;
        protected override void Activate()
        {
            Button_Fly?.UpdateText();
            CountItems?.UpdateText();
        }
        protected override void Start()
        {
            FindBackMainMenu();
            if (GameState.IsCreative)
            {
                Button_Fly = MenuUI<Button>.Create("ButtonFly", GetTransform(), new TextUI(() => new object[] { LText.Fly, ": ", CameraControll.Instance?.CPlayerBody?.CharacterInput.isFly.GetLText() }), true);

                Button_Fly.OnClick().AddListener(() => CameraControll.Instance?.CPlayerBody?.CharacterInput.Fly());

                Transform group = MenuUI<HorizontalLayoutGroup>.Create("Bodyes", GetTransform(), LText.Null, true).gameObject.transform;

                MenuUI<Button> GiveBodyFox = MenuUI<Button>.Create("Fox", group, new TextUI(() => new object[] { LText.Fox, }));

                MenuUI<Button> GiveBodyRedBot = MenuUI<Button>.Create("RedBot", group, new TextUI(() => new object[] { LText.Red, " Bot" }));

                MenuUI<Button> GiveBodyBlackBot = MenuUI<Button>.Create("BlackBot", group, new TextUI(() => new object[] { LText.Black + " Bot"}));

                MenuUI<Button> GiveBodyBDSMFox = MenuUI<Button>.Create("BDSMFox", group, new TextUI(() => new object[] { LText.Fox + " BDSM" }));

                MenuUI<Button> Swim = MenuUI<Button>.Create("FoxSwim", group, new TextUI(() => new object[] { LText.Fox + " Swim" }));

                MenuUI<Button> kein = MenuUI<Button>.Create("Kevin", group, new TextUI(() => new object[] { "Kevin" }));

                Transform group2 = MenuUI<HorizontalLayoutGroup>.Create("Bodyes2", GetTransform(), LText.Null, true).gameObject.transform;

                MenuUI<Button> coyot = MenuUI<Button>.Create("Coyot", group2, new TextUI(() => new object[] { "Coyot" }));
                MenuUI<Button> coyotBiker = MenuUI<Button>.Create("Biker", group2, new TextUI(() => new object[] { "Biker" }));
                MenuUI<Button> wolf = MenuUI<Button>.Create("Wolf", group2, new TextUI(() => new object[] { "Wolf" }));

                coyotBiker.OnClick().AddListener(() =>
                {
                    GiveBody(6);
                    Clothes putOnClothes = CameraControll.Instance.CPlayerBody.GetComponent<Clothes>();
                    putOnClothes.SelectClothes(0);
                    putOnClothes.SelectClothes(1);
                    putOnClothes.SelectClothes(2);
                    putOnClothes.SelectClothes(3);
                });

                coyot.OnClick().AddListener(() =>
                {
                    GiveBody(6);
                });

                wolf.OnClick().AddListener(() =>
                {
                    GiveBody(7);
                });

                GiveBodyFox.OnClick().AddListener(() =>
                {
                    GiveBody();
                });

                GiveBodyRedBot.OnClick().AddListener(() =>
                {
                    GiveBody(1);
                });

                GiveBodyBlackBot.OnClick().AddListener(() =>
                {
                    GiveBody(2);
                });

                kein.OnClick().AddListener(() =>
                {
                    GiveBody(5);
                });
                

                GiveBodyBDSMFox.OnClick().AddListener(() =>
                {
                    GiveBody();
                    Clothes putOnClothes = CameraControll.Instance.CPlayerBody.GetComponent<Clothes>();
                    putOnClothes.SelectClothes(0);
                    putOnClothes.SelectClothes(1);
                    putOnClothes.SelectClothes(2);
                });
                Swim.OnClick().AddListener(() =>
                {
                    GiveBody();
                    Clothes putOnClothes = CameraControll.Instance.CPlayerBody.GetComponent<Clothes>();
                    putOnClothes.SelectClothes(3);
                });

                MenuUI<Button> TabEntity = MenuUI<Button>.Create("TabEntity", GetTransform(), new TextUI(() => new object[] { LText.Create, " ", LText.Entities }), true);

                TabEntity.OnClick(() =>
                {
                    Menu.RebuildMenuInMainGroup<Lobby>(Menu.TypeRebuildMainGroup.Only_Custom, this.TabEntity1);
                });
            }
            FindExtitButton();
        }
        private void GiveBody(int indexBody = 0)
        {
            Menu.PopMenu(true);
            if (CameraControll.Instance.CPlayerBody == null)
                CameraControll.Instance.GiveBody(indexBody);
            else
                MessageBox.Info("Нельзя выдать новое тело находясь в теле");
        }
        private void FindBackMainMenu()
        {
            MenuUI<Button>.Create("BackMainMenu", GetTransform(), LText.MainMenu, true).OnClick(() => GameState.StartGame(GameState.TypeModeGame.MainMenu));
        }
        private void FindExtitButton()
        {
            MenuUI<Button>.Create("Exit", GetTransform(), LText.Exit, true).OnClick(Menu.ExitGame);
        }
        private void TabEntity1(MainGroup group)
        {
            FindBackMainMenu();

            MenuUI<Button> Body = MenuUI<Button>.Create("Boby", GetTransform(), LText.Body, true);
            Body.OnClick().AddListener(() => Menu.RebuildMenuInMainGroup<Lobby>(Menu.TypeRebuildMainGroup.Only_Default));

            Transform GroupAddItems = MenuUI<HorizontalLayoutGroup>.Create("AddItem", GetTransform(), LText.Null, true).gameObject.transform;

            MenuUI<Dropdown> SellectItem = MenuUI<Dropdown>.Create("SellectItem", GroupAddItems, LText.Null, false, MenuUIAutoRect.SetWidth(100F));

            List<string> TypesItem = Enum.GetNames(typeof(TypeItem)).ToList();
            TypesItem.Remove("None");
            TypesItem.Sort();

            SellectItem.Component.AddOptions(TypesItem);
            SellectItem.Component.onValueChanged.AddListener((index) =>
            {
                typeItem = (TypeItem)Enum.Parse(typeof(TypeItem),  TypesItem[index]);
            });
            typeItem = TypeItem.Apple;
            MenuUI<Button>.Create("Add", GroupAddItems, LText.Add).OnClick(() => GiveItem());
            MenuUI<Button>.Create("X20", GroupAddItems, "X20".GetTextUI()).OnClick(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    for (int y = -1; y < 1; y++)
                    {
                        for (int z = -1; z < 1; z++)
                        {
                            var tuple = GiveItem();
                            if (!tuple.Item1)
                                break;
                            tuple.Item2.Transform.position = Vector3.up * (i * 2 + 2) + new Vector3(y,0,z);
                        }
                    }
                }
            });
            MenuUI<Button>.Create("GiveItem", GroupAddItems, LText.Give).OnClick(() =>
            {

                CharacterBody player = CameraControll.Instance?.CPlayerBody;
                if (player)
                {
                    var CheckGive = GiveItem();
                    if (!CheckGive.Item1) return;
                    player.GiveItem(CheckGive.Item2);
                    Menu.PopMenu(true);
                }
                else
                    MessageBox.Info("Предмет можно выдать только телу");
            });
            Transform GroupClearItems = MenuUI<HorizontalLayoutGroup>.Create("ClearItem", GetTransform(), LText.Null, true).gameObject.transform;
            CountItems = MenuUI<Text>.Create("CountItems", GroupClearItems, new TextUI(() => new object[] { LText.Count, ": ", ItemEngine.CountItems }));
            MenuUI<Button>.Create("Clear", GroupClearItems, LText.Clear).OnClick().AddListener(() => { ItemEngine.RemoveItemAll(); CountItems.UpdateText(); });
            
            MenuUI<Button>.Create("KillAll", GetTransform(), "KillAll".GetTextUI(), true).OnClick(() =>
            {
                AnimalEngine.AnimalList.ForEach(animal => animal.Death());
                Menu.PopMenu(true);
            });
            Transform GroupAddPlant = MenuUI<HorizontalLayoutGroup>.Create("AddPlant", GetTransform(), LText.Null, true).gameObject.transform;

            MenuUI<Dropdown> SellectPlant = MenuUI<Dropdown>.Create("SellectPlant", GroupAddPlant, LText.Null, false, MenuUIAutoRect.SetWidth(100F));

            SellectPlant.Component.AddOptions(Enum.GetNames(typeof(TypePlant)).ToList());
            SellectPlant.Component.onValueChanged.AddListener((index) =>
            {
                typePlant = (TypePlant)index;
            });
            MenuUI<Button> Tree = MenuUI<Button>.Create("AddPlant", GroupAddPlant, new TextUI(() => new object[] { LText.Create }));
            Tree.OnClick(() =>
            {
                PositionEntity(typePlant);
            });
            FindExtitButton();
        }
        private void AnimalCreate()
        {
            Transform GroupFox = MenuUI<HorizontalLayoutGroup>.Create("GroupFox", GetTransform(), LText.Null, true).gameObject.transform;
            MenuUI<Text>.Create("Label", GroupFox, LText.Create);
            MenuUI<Button> Fox = MenuUI<Button>.Create("Fox", GroupFox, LText.Fox);
            Fox.OnClick().AddListener(() =>
            {
                PositionEntity(TypeAnimal.Fox);
            });
            MenuUI<Button>.Create("Fox_Red", GroupFox, LText.Red).OnClick(() =>
            {
                PositionEntity(TypeAnimal.Fox_Red);
            });
            MenuUI<Button>.Create("Fox_While", GroupFox, LText.While).OnClick(() =>
            {
                PositionEntity(TypeAnimal.Fox_White);
            });
        }
        private static void PositionEntity(Enum _Enum)
        {
            Vector3 position = CameraControll.Instance.Transform.position + CameraControll.Instance.Transform.forward * 3F;
            Ray ray = new Ray(position, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit Hit, 4F)) // , 1 << LayerMask.NameToLayer("Terrain")
            {
                position = Hit.point + ray.direction.normalized * 0.01F;
                Quaternion rotation = CameraControll.Instance.Transform.rotation;
                rotation = Quaternion.Euler((Vector3.up * 180F + Vector3.up * rotation.eulerAngles.y));
                if (_Enum is TypePlant typePlant)
                    PlantEngine.AddPlant(typePlant, position, Quaternion.AngleAxis(UnityEngine.Random.Range(0F, 360F), Vector3.up));
                else if (_Enum is TypeAnimal typeAnimal)
                    AnimalEngine.AddAnimal(typeAnimal, position, rotation);
                Menu.PopMenu(true);
            }
            else
                MessageBox.Info("Под объектом должна быть поверхность");
        }
        private static Tuple<bool, ItemEngine> GiveItem()
        {
            if (typeItem == TypeItem.None)
            {
                MessageBox.Info("Вещь не выбрана");
                return new Tuple<bool, ItemEngine>(false, null);
            }
            Menu.PopMenu(true);
            ItemEngine item = ItemEngine.AddItem(typeItem, Vector3.zero, Quaternion.identity, false);
            CountItems.UpdateText();
            return new Tuple<bool, ItemEngine>(true, item);
        }
    }
}
