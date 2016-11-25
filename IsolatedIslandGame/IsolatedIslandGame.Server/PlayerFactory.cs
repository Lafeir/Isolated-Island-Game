﻿using IsolatedIslandGame.Database;
using IsolatedIslandGame.Library;
using IsolatedIslandGame.Protocol;
using System.Collections.Generic;
using IsolatedIslandGame.Database.DatabaseFormatData;

namespace IsolatedIslandGame.Server
{
    public class PlayerFactory
    {
        private static PlayerFactory instance;
        public static PlayerFactory Instance { get { return instance; } }

        public static void InitialFactory()
        {
            instance = new PlayerFactory();
        }

        private Dictionary<int, Player> players;

        private PlayerFactory()
        {
            players = new Dictionary<int, Player>();
        }
        public bool PlayerLogin(ServerUser user, ulong facebookID, string accessToken, out string debugMessage, out ErrorCode errorCode)
        {
            if(FacebookService.LoginCheck(facebookID, accessToken))
            {
                debugMessage = null;
                errorCode = ErrorCode.NoError;
                int playerID;
                PlayerData playerData;
                if (!DatabaseService.RepositoryList.PlayerRepository.Contains(facebookID, out playerID))
                {
                    if(!DatabaseService.RepositoryList.PlayerRepository.Register(facebookID))
                    {
                        debugMessage = "register fail";
                        errorCode = ErrorCode.Fail;
                        return false;
                    }
                    if (DatabaseService.RepositoryList.PlayerRepository.Contains(facebookID, out playerID))
                    {
                        playerData = DatabaseService.RepositoryList.PlayerRepository.Find(playerID);
                    }
                    else
                    {
                        debugMessage = string.Format("facebookID: {0} Register Fail Identity: {1}", facebookID, user.IdentityInformation);
                        errorCode = ErrorCode.Fail;
                        return false;
                    }
                }
                else
                {
                    playerData = DatabaseService.RepositoryList.PlayerRepository.Find(playerID);
                }
                Player player = new Player(user, playerData.playerID, playerData.facebookID, playerData.nickname, playerData.lastConnectedIPAddress);
                if (PlayerOnline(player))
                {
                    return true;
                }
                else
                {
                    debugMessage = string.Format("PlayerID: {0} already Logined from IP: {1}", player.PlayerID, player.LastConnectedIPAddress?.ToString() ?? "");
                    errorCode = ErrorCode.AlreadyExisted;
                    return false;
                }
            }
            else
            {
                debugMessage = "facebook login fail";
                errorCode = ErrorCode.Fail;
                return false;
            }
        }
        public void PlayerLogout(Player player)
        {
            if (players.ContainsKey(player.PlayerID))
            {
                UserFactory.Instance.UserDisconnect(player.User as ServerUser);
            }
        }

        public bool PlayerOnline(Player player)
        {
            if (players.ContainsKey(player.PlayerID))
            {
                return false;
            }
            else
            {
                players.Add(player.PlayerID, player);
                player.User.PlayerOnline(player);
                LogService.InfoFormat("PlayerID: {0} Online from: {1}", player.PlayerID, player.LastConnectedIPAddress);
                return true;
            }
        }
        public void PlayerOffline(Player player)
        {
            if (players.ContainsKey(player.PlayerID))
            {
                DatabaseService.RepositoryList.PlayerRepository.Save(player);
                players.Remove(player.PlayerID);
            }
            LogService.InfoFormat("PlayerID: {0} Offline", player.PlayerID);
            player.User.PlayerOffline();
        }
    }
}
