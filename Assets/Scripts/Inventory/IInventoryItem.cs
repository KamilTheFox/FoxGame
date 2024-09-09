using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Inventory
{
    public interface IInventoryItem
    {
        string Name { get; }
        string Description { get; }
        Sprite Icon { get; }

        int Value { get; }

    }
}
