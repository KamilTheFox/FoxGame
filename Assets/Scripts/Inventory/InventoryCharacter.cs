using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory
{
    public class InventoryCharacter : MonoBehaviour
    {
        [SerializeField] private DefaultGridCell defaultGrid;
        private List<GridSell> grids;

        private class DefaultGridCell : GridSell
        {
            [SerializeField] private int capacity;
            private List<IInventoryItem> inventoryItems;
            public void AddItem(IInventoryItem item)
            {
                if (inventoryItems.Count + item.Value > capacity)
                {
                    return;
                }
                inventoryItems.Add(item);
            }

            public bool FreeCell(IInventoryItem item)
            {
                return inventoryItems.Count + item.Value <= capacity;
            }
        }
        public void Start()
        {
            grids.Add(defaultGrid);
        }

        public void AddItem(IInventoryItem item)
        {
            foreach (GridSell s in grids)
            {
                if(s.FreeCell(item))
                {
                    grids.Add(s);
                    break;
                }
            }
            Menu.Info("Нет места!");
        }

        public void AddGridSell(GridSell grid)
        {
            grids.Add(grid);
        }



    }
}
