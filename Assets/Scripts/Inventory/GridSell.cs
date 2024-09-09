using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inventory
{
    public interface GridSell
    {
        bool FreeCell(IInventoryItem item);

        void AddItem(IInventoryItem item);


    }
}
