using Campofinale.Packets.Sc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campofinale.Game.Inventory
{
    public class InventoryList
    {
        public List<Item> items = new();
        public Dictionary<int, Item> bag = new();
        public int maxBagSize = 30;

        public Player player;
        public InventoryList(Player player)
        {
            this.player = player;
        }
        public enum FindType
        {
            Items,
            FactoryDepots,
            Bag
        }
        public void UpdateInventoryPacket()
        {

        }
        public void UpdateBagInventoryPacket()
        {
            player.Send(new PacketScItemBagScopeSync(this.player,Resource.ItemValuableDepotType.Invalid));
            
        }
        private void AddToBagAvailableSlot(Item item)
        {
            for (int i = 0; i < maxBagSize; i++)
            {
                if (!bag.ContainsKey(i))
                {
                    bag.Add(i, item);
                    return;
                }
            }
        }
        ///
        ///<summary>Add a item directly to the bag if there is enough space or increment current stack value</summary>
        ///
        public bool AddToBag(Item item)
        {
            Item existOne = Find(i=>i.id == item.id && i.amount < item.GetItemTable().maxStackCount,FindType.Bag);
            if (existOne != null)
            {
                
                if(existOne.amount+item.amount > item.GetItemTable().maxStackCount)
                {
                    int max = existOne.GetItemTable().maxStackCount;
                    int toAddInNewSlotAmount = existOne.amount + item.amount - max;
                    item.amount = toAddInNewSlotAmount;
                    if (SlotAvailableInBag())
                    {
                        existOne.amount = max;
                        AddToBagAvailableSlot(item);
                        UpdateBagInventoryPacket();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    existOne.amount += item.amount;
                    UpdateBagInventoryPacket();
                    return true;
                }
            }
            else
            {
                if(bag.Count < maxBagSize)
                {
                    AddToBagAvailableSlot(item);
                    UpdateBagInventoryPacket();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public bool SlotAvailableInBag()
        {
            bool availableSlot = false;
            for (int i = 0; i < maxBagSize; i++)
            {
                if (!bag.ContainsKey(i))
                {
                    return true;
                }
            }
            return availableSlot;
        }
        public Item FindInAll(Predicate<Item> match)
        {
            var item = items.Find(match);
            if (item != null)
            {
                return item;
            }
            var itemB = bag.Values.ToList().Find(match);
            if (itemB != null)
            {
                return itemB;
            }
            return null;
        }
        public Item Find(Predicate<Item> match,FindType findType = FindType.Items)
        {
            switch (findType)
            {
                case FindType.Items:
                    var item = items.Find(match);
                    if (item != null)
                    {
                        return item;
                    }
                    break;
                case FindType.FactoryDepots:
                    //TODO
                    break;
                case FindType.Bag:
                    var itemB = bag.Values.ToList().Find(match);
                    if (itemB != null)
                    {
                        return itemB;
                    }
                    break;
            }
            
            return null;
        }

        ///<summary>
        ///Add an item to the inventory (or increment it's amount if it's not an instance type, else add a new one or add to bag if it's a Factory item
        ///</summary>
        public Item Add(Item item)
        {
            if (item.StorageSpace() == Resource.ItemStorageSpace.BagAndFactoryDepot)
            {
                AddToBag(item);
                return null;
            }
            if (item.InstanceType())
            {
                items.Add(item);
                return item;
            }
            else
            {
                Item exist=Find(i=>i.id==item.id);
                if (exist != null)
                {
                    exist.amount += item.amount;
                    return exist;
                }
                else
                {
                    items.Add(item);
                    return item;
                }
            }

        }

       
    }
}
