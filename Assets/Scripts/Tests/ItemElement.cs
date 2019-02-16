using Assets.Scripts.PooledScrollList;
using UnityEngine;

namespace Assets.Scripts.Tests
{
    [RequireComponent(typeof(Item))]
    public class ItemElement : PooledElement<ItemData>
    {
        private Item _item;

        public Item Item
        {
            get
            {
                if (_item == null)
                {
                    _item = GetComponent<Item>();
                }

                return _item;
            }
        }

        public override ItemData Data
        {
            get { return Item.Data; }
            set { Item.Setup(value); }
        }
    }
}