using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace GroupMenu
{
    public class Lobby : MainGroup
    {
        private static MenuUI<Button> ExitGameButton, Back_MainMenu, Button_Fly;

        private static MenuUI<Text> CountItems;

        private static TypeItem typeItem;

        public override TypeMenu TypeMenu => TypeMenu.Lobby;
        protected override void Activate()
        {
            Button_Fly.UpdateText();
            CountItems.UpdateText();
        }
        protected override void Start()
        {
            Back_MainMenu = MenuUI<Button>.Find("BackMainMenu", GetTransform(), LText.MainMenu, true);

            Back_MainMenu.OnClick().AddListener(() => SceneMenu.LoadLevel(0));

            Button_Fly = MenuUI<Button>.Find("ButtonFly", GetTransform(), new TextUI(() => new object[] { LText.Fly, ": " , CameraControll.instance?.PlayerControll?.isFly.GetLText() }), true);

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

            Transform GroupAddItems = MenuUI<HorizontalLayoutGroup>.Create("AddItem", GetTransform(), LText.Null, true).gameObject.transform;
           
            MenuUI<Dropdown> SellectItem = MenuUI<Dropdown>.Create("SellectItem", GroupAddItems, LText.Null, false, MenuUIAutoRect.SetWidth(100F));
            SellectItem.Component.AddOptions(Enum.GetNames(typeof(TypeItem)).ToList());
            SellectItem.Component.onValueChanged.AddListener((index) =>
            {
                typeItem = (TypeItem)index;
            });

            MenuUI<Button>.Create("Add", GroupAddItems, LText.Add).OnClick().AddListener(() => GiveItem());
            MenuUI<Button>.Create("X20", GroupAddItems, "X20".GetTextUI()).OnClick().AddListener(() =>
            {
                for (int i = 0; i < 20; i++)
                {
                    var tuple = GiveItem();
                    if (!tuple.Item1)
                        break;
                    tuple.Item2.Transform.position = Vector3.up * i * 2;
                }
            });
            MenuUI<Button>  GiveTNT = MenuUI<Button>.Create("GiveItem", GroupAddItems, LText.Give);

            GiveTNT.OnClick().AddListener(() =>
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
            CountItems = MenuUI<Text>.Create("CountItems", GroupClearItems, new TextUI(() => new object[] { LText.Count, ": " , ItemEngine.CountItems }));
            MenuUI<Button>.Create("Clear", GroupClearItems, LText.Clear).OnClick().AddListener(() => { ItemEngine.RemoveItemAll(); CountItems.UpdateText(); });
            Transform GroupFox = MenuUI<HorizontalLayoutGroup>.Create("GroupFox", GetTransform(), LText.Null, true).gameObject.transform;
            MenuUI<Text>.Create("Label", GroupFox, LText.Create);
            MenuUI<Button> Fox = MenuUI<Button>.Create("Fox", GroupFox,  LText.Fox );
            Fox.OnClick().AddListener(() =>
            {
                PositionEntity(TypeAnimal.Fox);
            });
            MenuUI<Button> Fox_Red = MenuUI<Button>.Create("Fox_Red", GroupFox, "Red".GetTextUI() );
            Fox_Red.OnClick().AddListener(() =>
            {
                PositionEntity(TypeAnimal.Fox_Red);
            });
            MenuUI<Button> Fox_While = MenuUI<Button>.Create("Fox_While", GroupFox, "While".GetTextUI() );
            Fox_While.OnClick().AddListener(() =>
            {
                PositionEntity(TypeAnimal.Fox_White);
            });
            MenuUI<Button> Tree = MenuUI<Button>.Create("Tree", GetTransform(),  new TextUI(() => new object[] { LText.Create , " Tree" }), true );
            Tree.OnClick().AddListener(() =>
            {
                PositionEntity(TypePlant.Tree_1);
            });


            ExitGameButton = MenuUI<Button>.Find("Exit", GetTransform(), LText.Exit, true);
            ExitGameButton.OnClick().AddListener(() => Menu.ExitGame());
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
                    PlantEngine.AddPlant(typePlant, position, rotation);
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
