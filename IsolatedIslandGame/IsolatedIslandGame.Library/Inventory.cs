﻿using IsolatedIslandGame.Library.Items;
using IsolatedIslandGame.Protocol;
using System.Collections.Generic;
using System.Linq;
using System;

namespace IsolatedIslandGame.Library
{
    public class Inventory
    {
        public static int DefaultCapacity { get { return 20; } }

        public int InventoryID { get; private set; }
        public int Capacity { get; private set; }
        private Dictionary<int, InventoryItemInfo> itemInfoDictionary;
        private InventoryItemInfo[] itemInfos;
        public int DifferentItemCount { get { return itemInfoDictionary.Count; } }
        

        public IEnumerable<InventoryItemInfo> ItemInfos { get { return itemInfoDictionary.Values.OrderBy(x => x.PositionIndex); } }

        public delegate void InventoryItemInfoChangeEventHandler(DataChangeType changeType, InventoryItemInfo info);
        private event InventoryItemInfoChangeEventHandler onItemInfoChange;
        public event InventoryItemInfoChangeEventHandler OnItemInfoChange { add { onItemInfoChange += value; } remove { onItemInfoChange -= value; } }

        public Inventory(int inventoryID, int capacity)
        {
            InventoryID = inventoryID;
            Capacity = capacity;
            itemInfoDictionary = new Dictionary<int, InventoryItemInfo>();
            itemInfos = new InventoryItemInfo[Capacity];
        }
        public bool ContainsInventoryItemInfo(int inventoryItemInfoID)
        {
            return itemInfoDictionary.ContainsKey(inventoryItemInfoID);
        }
        public bool ContainsItem(int itemID)
        {
            return itemInfoDictionary.Values.Any(x => x.Item.ItemID == itemID);
        }
        public bool FindInventoryItemInfo(int inventoryItemInfoID, out InventoryItemInfo info)
        {
            if (ContainsInventoryItemInfo(inventoryItemInfoID))
            {
                info = itemInfoDictionary[inventoryItemInfoID];
                return true;
            }
            else
            {
                info = null;
                return false;
            }
        }
        public bool FindInventoryItemInfoByItemID(int itemID, out InventoryItemInfo info)
        {
            if (ContainsItem(itemID))
            {
                info = ItemInfos.First(x => x.Item.ItemID == itemID);
                return true;
            }
            else
            {
                info = null;
                return false;
            }
        }
        public int ItemCount(int itemID)
        {
            if(ContainsItem(itemID))
            {
                return ItemInfos.First(x => x.Item.ItemID == itemID).Count;
            }
            else
            {
                return 0;
            }
        }
        public void LoadItemInfo(InventoryItemInfo info)
        {
            if(!ContainsInventoryItemInfo(info.InventoryItemInfoID))
            {
                itemInfoDictionary.Add(info.InventoryItemInfoID, info);
                itemInfos[info.PositionIndex] = info;
                onItemInfoChange?.Invoke(DataChangeType.Add, info);
            }
            else
            {
                InventoryItemInfo existedInfo = itemInfoDictionary[info.InventoryItemInfoID];
                existedInfo.Count = info.Count;
                existedInfo.PositionIndex = info.PositionIndex;
                onItemInfoChange?.Invoke(DataChangeType.Update, existedInfo);
            }
        }
        public void RemoveItemInfo(int itemInfoID)
        {
            if (ContainsInventoryItemInfo(itemInfoID))
            {
                InventoryItemInfo info = itemInfoDictionary[itemInfoID];
                itemInfoDictionary.Remove(itemInfoID);
                itemInfos[info.PositionIndex] = null;
                onItemInfoChange?.Invoke(DataChangeType.Remove, info);
            }
        }
        public bool AddItem(Item item, int count)
        {
            InventoryItemInfo info;
            if (FindInventoryItemInfoByItemID(item.ItemID, out info))
            {
                info.Count += count;
                onItemInfoChange?.Invoke(DataChangeType.Update, info);
                return true;
            }
            else
            {
                int positionIndex = Array.FindIndex(itemInfos, x => x == null);
                if (positionIndex >= 0)
                {
                    if (InventoryItemInfoFactory.Instance != null && InventoryItemInfoFactory.Instance.CreateItemInfo(InventoryID, item.ItemID, count, positionIndex, out info))
                    {
                        itemInfoDictionary.Add(info.InventoryItemInfoID, info);
                        itemInfos[info.PositionIndex] = info;
                        onItemInfoChange?.Invoke(DataChangeType.Add, info);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }
        public bool RemoveItem(int itemID, int count)
        {
            if (ContainsItem(itemID) && ItemCount(itemID) >= count)
            {
                InventoryItemInfo info;
                if(FindInventoryItemInfoByItemID(itemID, out info))
                {
                    info.Count -= count;
                    if (info.Count == 0)
                    {
                        if (itemInfoDictionary.ContainsKey(info.InventoryItemInfoID))
                        {
                            itemInfoDictionary.Remove(info.InventoryItemInfoID);
                        }
                        itemInfos[info.PositionIndex] = null;
                        InventoryItemInfoFactory.Instance?.DeleteItemInfo(info.InventoryItemInfoID);
                        onItemInfoChange?.Invoke(DataChangeType.Remove, info);
                    }
                    else
                    {
                        onItemInfoChange?.Invoke(DataChangeType.Update, info);
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
