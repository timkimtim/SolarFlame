using UnityEngine;

namespace Items.Scripts
{
    /// <summary>
    /// This is a scriptable object that defines waht an item is in our game.
    /// it could be inherited from to have branched version of items, for example potions and equipment.
    /// </summary>

    [CreateAssetMenu(menuName = "Inventory System/Inventory Item")]
    public class InventoryItemData : ScriptableObject
    {
        public string DisplayName;
        [TextArea(4, 4)]
        public string Description;
        public string Type;
        public Sprite Icon;
        public int MaxStackSize;
    }
}
