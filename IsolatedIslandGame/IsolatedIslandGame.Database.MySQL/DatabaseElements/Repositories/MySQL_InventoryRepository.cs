﻿using IsolatedIslandGame.Database.DatabaseElements.Repositories;
using IsolatedIslandGame.Library;
using MySql.Data.MySqlClient;

namespace IsolatedIslandGame.Database.MySQL.DatabaseElements.Repositories
{
    class MySQL_InventoryRepository : InventoryRepository
    {
        public override Inventory Create(int playerID, int capacity)
        {
            string sqlString = @"INSERT INTO InventoryCollection 
                (PlayerID,Capacity) VALUES (@playerID,@capacity) ;
                SELECT LAST_INSERT_ID();";
            using (MySqlCommand command = new MySqlCommand(sqlString, DatabaseService.ConnectionList.PlayerDataConnection.Connection as MySqlConnection))
            {
                command.Parameters.AddWithValue("playerID", playerID);
                command.Parameters.AddWithValue("capacity", capacity);
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int inventoryID = reader.GetInt32(0);
                        return new Inventory(inventoryID, capacity);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }
        public override Inventory Read(int inventoryID)
        {
            Inventory inventory = null;
            string sqlString = @"SELECT  
                Capacity
                from InventoryCollection WHERE InventoryID = @inventoryID;";
            using (MySqlCommand command = new MySqlCommand(sqlString, DatabaseService.ConnectionList.PlayerDataConnection.Connection as MySqlConnection))
            {
                command.Parameters.AddWithValue("@inventoryID", inventoryID);
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int capacity = reader.GetInt32(0);
                        inventory = new Inventory(inventoryID, capacity);
                    }
                }
            }
            if(inventory != null)
            {
                var itemInfos = DatabaseService.RepositoryList.InventoryItemInfoRepository.ListOfInventory(inventory.InventoryID);
                foreach(var itemInfo in itemInfos)
                {
                    inventory.LoadItemInfo(itemInfo);
                }
            }
            return inventory;
        }
        public override Inventory ReadByPlayerID(int playerID)
        {
            Inventory inventory = null;
            string sqlString = @"SELECT  
                InventoryID, Capacity
                from InventoryCollection WHERE PlayerID = @playerID;";
            using (MySqlCommand command = new MySqlCommand(sqlString, DatabaseService.ConnectionList.PlayerDataConnection.Connection as MySqlConnection))
            {
                command.Parameters.AddWithValue("@playerID", playerID);
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int inventoryID = reader.GetInt32(0);
                        int capacity = reader.GetInt32(1);
                        inventory = new Inventory(inventoryID, capacity);
                    }
                }
            }
            if (inventory != null)
            {
                var itemInfos = DatabaseService.RepositoryList.InventoryItemInfoRepository.ListOfInventory(inventory.InventoryID);
                foreach (var itemInfo in itemInfos)
                {
                    inventory.LoadItemInfo(itemInfo);
                }
            }
            return inventory;
        }
        public override void Update(Inventory inventory)
        {
            string sqlString = @"UPDATE InventoryCollection SET 
                Capacity = @capacity
                WHERE InventoryID = @inventoryID;";
            using (MySqlCommand command = new MySqlCommand(sqlString, DatabaseService.ConnectionList.ItemDataConnection.Connection as MySqlConnection))
            {
                command.Parameters.AddWithValue("@capacity", inventory.Capacity);
                command.Parameters.AddWithValue("@inventoryID", inventory.InventoryID);
                if (command.ExecuteNonQuery() <= 0)
                {
                    LogService.ErrorFormat("MySQL_InventoryRepository Save Inventory Error InventoryID: {0}", inventory.InventoryID);
                }
            }
            foreach(var itemInfo in inventory.ItemInfos)
            {
                DatabaseService.RepositoryList.InventoryItemInfoRepository.Update(itemInfo);
            }
        }
        public override void Delete(int inventoryID)
        {
            string sqlString = @"DELETE FROM InventoryCollection 
                WHERE InventoryID = @inventoryID;";
            using (MySqlCommand command = new MySqlCommand(sqlString, DatabaseService.ConnectionList.PlayerDataConnection.Connection as MySqlConnection))
            {
                command.Parameters.AddWithValue("@inventoryID", inventoryID);
                if (command.ExecuteNonQuery() <= 0)
                {
                    LogService.ErrorFormat("MySQL_InventoryRepository Delete Inventory Error InventoryID: {0}", inventoryID);
                }
            }
        }
    }
}
