using System;
using System.Collections;
using UnityEngine;

namespace Inventory
{
    [CreateAssetMenu(menuName = "CreateItemInfo", fileName = "ItemInfo")]
    public class ItemInfo : ScriptableObject, IInventoryItem
    {
        [field: SerializeField] public string Name { get; private set; }

        [field: SerializeField] public string Description { get; private set; }

        [field: SerializeField] public Sprite Icon { get; private set; }

        [field: SerializeField] public int Value { get; private set; }

        [field: SerializeField] public TypeItem TypeItem { get; private set; }
    }
}
