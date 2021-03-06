﻿using IsolatedIslandGame.Database.Repositories;
using IsolatedIslandGame.Library;
using IsolatedIslandGame.Protocol;
using MySql.Data.MySqlClient;
using System;
using System.Net;

namespace IsolatedIslandGame.Database.MySQL.Repositories
{
    class MySQL_PlayerRepository : PlayerRepository
    {
        public override bool Register(ulong facebookID)
        {
            int playerID;
            if (Contains(facebookID, out playerID))
            {
                return false;
            }
            else
            {
                string sqlString = @"INSERT INTO PlayerCollection 
                    (FacebookID, RegisterDate) VALUES (@facebookID, @registerDate) ;
                    SELECT LAST_INSERT_ID();";
                using (MySqlCommand command = new MySqlCommand(sqlString, DatabaseService.ConnectionList.PlayerDataConnection.Connection as MySqlConnection))
                {
                    command.Parameters.AddWithValue("@facebookID", facebookID);
                    command.Parameters.AddWithValue("@registerDate", DateTime.Now);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            playerID = reader.GetInt32(0);
                        }
                        else
                        {
                            LogService.ErrorFormat("MySQLPlayerRepository Register Player no affected row from FacebookID: {0}", facebookID);
                            return false;
                        }
                    }
                }
            }
            Inventory inventory;
            return DatabaseService.RepositoryList.InventoryRepository.Create(playerID, Inventory.DefaultCapacity, out inventory);
        }

        public override bool Contains(ulong facebookID, out int playerID)
        {
            using (MySqlCommand command = new MySqlCommand("SELECT PlayerID FROM PlayerCollection WHERE FacebookID = @facebookID;", DatabaseService.ConnectionList.PlayerDataConnection.Connection as MySqlConnection))
            {
                command.Parameters.AddWithValue("@facebookID", facebookID);
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        playerID = reader.GetInt32(0);
                        return true;
                    }
                    else
                    {
                        playerID = -1;
                        return false;
                    }
                }
            }
        }

        public override bool Read(int playerID, out Player player)
        {
            string sqlString = @"SELECT  
                FacebookID, Nickname, Signature, GroupType, LastConnectedIPAddress
                from PlayerCollection WHERE PlayerID = @playerID;";
            using (MySqlCommand command = new MySqlCommand(sqlString, DatabaseService.ConnectionList.PlayerDataConnection.Connection as MySqlConnection))
            {
                command.Parameters.AddWithValue("@playerID", playerID);
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        ulong facebookID = reader.GetUInt64(0);
                        string nickname = reader.IsDBNull(1) ? null : reader.GetString(1);
                        string signature = reader.IsDBNull(2) ? null : reader.GetString(2);
                        GroupType groupType = (GroupType)reader.GetByte(3);
                        IPAddress lastConnectedIPAddress = reader.IsDBNull(4) ? IPAddress.None : IPAddress.Parse(reader.GetString(4));
                        player = new Player(playerID, facebookID, nickname, signature, groupType, lastConnectedIPAddress);
                        return true;
                    }
                    else
                    {
                        player = null;
                        return false;
                    }
                }
            }
        }

        public override void Update(Player player)
        {
            string sqlString = @"UPDATE PlayerCollection SET 
                Nickname = @nickname,
                Signature = @signature,
                GroupType = @groupType,
                LastConnectedIPAddress = @lastConnectedIPAddress
                WHERE PlayerID = @playerID;";
            using (MySqlCommand command = new MySqlCommand(sqlString, DatabaseService.ConnectionList.PlayerDataConnection.Connection as MySqlConnection))
            {
                command.Parameters.AddWithValue("@nickname", player.Nickname ?? "");
                command.Parameters.AddWithValue("@signature", player.Signature ?? "");
                command.Parameters.AddWithValue("@groupType", (byte)player.GroupType);
                command.Parameters.AddWithValue("@lastConnectedIPAddress", player.LastConnectedIPAddress.ToString());
                command.Parameters.AddWithValue("@playerID", player.PlayerID);
                if (command.ExecuteNonQuery() <= 0)
                {
                    LogService.ErrorFormat("MySQLPlayerRepository Save Player no affected row from PlayerID:{0}, IPAddress:{1}", player.PlayerID, player.LastConnectedIPAddress);
                }
            }
            if(player.Inventory != null)
            {
                DatabaseService.RepositoryList.InventoryRepository.Update(player.Inventory);
            }
            if (player.Vessel != null)
            {
                DatabaseService.RepositoryList.VesselRepository.Update(player.Vessel);
            }
            var knownBlueprints = DatabaseService.RepositoryList.PlayerKnownBlueprintRepository.ListOfPlayer(player.PlayerID);
            foreach (var blueprint in knownBlueprints)
            {
                player.GetBlueprint(blueprint);
            }
        }
    }
}
