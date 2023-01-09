using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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
                Button_Fly = MenuUI<Button>.Find("ButtonFly", GetTransform(), new TextUI(() => new object[] { LText.Fly, ": ", CameraControll.instance?.PlayerControll?.isFly.GetLText() }), true);

                Button_Fly.OnClick().AddListener(() => CameraControll.instance?.PlayerControll?.Fly());


                MenuUI<Button> GiveBody = MenuUI<Button>.Find("GiveBody", GetTransform(), new TextUI(() => new object[] { LText.Give, " ", LText.Body }), true);

                GiveBody.OnClick().AddListener(() =>
                {
                    Menu.PopMenu(true);
                    if (CameraControll.instance.PlayerControll == null)
                        CameraControll.instance.GiveBody();
                    else
                        MessageBox.Info("Нельзя выдать новое тело находясь в теле");
                });
                MenuUI<Button> TabEntity = MenuUI<Button>.Create("TabEntity", GetTransform(), new TextUI(() => new object[] { LText.Create, " ", LText.Entities }), true);

                TabEntity.OnClick(() =>
                {
                    Menu.RebuildMenuInMainGroup<Lobby>(Menu.TypeRebuildMainGroup.Only_Custom, this.TabEntity1);
                });
            }
            FindExtitButton();
        }
        private void FindBackMainMenu()
        {
            MenuUI<Button>.Find("BackMainMenu", GetTransform(), LText.MainMenu, true).OnClick(() => GameState.StartGame(GameState.TypeModeGame.MainMenu));
        }
        private void FindExtitButton()
        {
            MenuUI<Button>.Find("Exit", GetTransform(), LText.Exit, true).OnClick(Menu.ExitGame);
        }
        private void TabEntity1(MainGroup group)
        {
            FindBackMainMenu();

            MenuUI<Button> Body = MenuUI<Button>.Create("Boby", GetTransform(), LText.Body, true);
            Body.OnClick().AddListener(() => Menu.RebuildMenuInMainGroup<Lobby>(Menu.TypeRebuildMainGroup.Only_Default));

            Transform GroupAddItems = MenuUI<HorizontalLayoutGroup>.Create("AddItem", GetTransform(), LText.Null, true).gameObject.transform;

            MenuUI<Dropdown> SellectItem = MenuUI<Dropdown>.Create("SellectItem", GroupAddItems, LText.Null, false, MenuUIAutoRect.SetWidth(100F));
            SellectItem.Component.AddOptions(Enum.GetNames(typeof(TypeItem)).ToList());
            SellectItem.Component.onValueChanged.AddListener((index) =>
            {
                typeItem = (TypeItem)index;
            });

            MenuUI<Button>.Create("Add", GroupAddItems, LText.Add).OnClick(() => GiveItem());
            MenuUI<Button>.Create("X20", GroupAddItems, "X20".GetTextUI()).OnClick(() =>
            {
                for (int i = 0; i < 20; i++)
                {
                    var tuple = GiveItem();
                    if (!tuple.Item1)
                        break;
                    tuple.Item2.Transform.position = Vector3.up * (i * 2 + 2);
                }
            });
            MenuUI<Button>.Create("GiveItem", GroupAddItems, LText.Give).OnClick(() =>
            {

                PlayerControll player = CameraControll.instance?.PlayerControll;
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
            MenuUI<Button>.Create("KillAll", GetTransform(), "KillAll".GetTextUI(), true).OnClick(() =>
            {
                AnimalEngine.AnimalList.ForEach(animal => animal.Dead());
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
        private static void PositionEntity(Enum _Enum)
        {
            Vector3 position = CameraControll.instance.Transform.position + CameraControll.instance.Transform.forward * 3F;
            Ray ray = new Ray(position, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit Hit, 4F)) // , 1 << LayerMask.NameToLayer("Terrain")
            {
                position = Hit.point + ray.direction.normalized * 0.01F;
                Quaternion rotation = CameraControll.instance.Transform.rotation;
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
