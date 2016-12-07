﻿using IsolatedIslandGame.Protocol.Communication.FetchDataCodes;
using IsolatedIslandGame.Protocol.Communication.FetchDataParameters.System;
using IsolatedIslandGame.Protocol.Communication.FetchDataResponseParameters.System;
using IsolatedIslandGame.Library.Items;
using System;
using System.Collections.Generic;

namespace IsolatedIslandGame.Library.CommunicationInfrastructure.Operations.Handlers.SystemOperationHandlers.FetchDataHandlers
{
    class FetchItemHandler : SystemFetchDataHandler
    {
        public FetchItemHandler(SystemManager subject) : base(subject, 1)
        {
        }

        public override bool Handle(SystemFetchDataCode fetchCode, Dictionary<byte, object> parameter)
        {
            if (base.Handle(fetchCode, parameter))
            {
                try
                {
                    int itemID = (int)parameter[(byte)FetchItemParameterCode.ItemID];
                    Item item = ItemManager.Instance.FindItem(itemID);
                    if(item != null)
                    {
                        if (item is Material)
                        {
                            Material material = item as Material;
                            var result = new Dictionary<byte, object>
                            {
                                { (byte)FetchMaterialResponseParameterCode.ItemID, material.ItemID },
                                { (byte)FetchMaterialResponseParameterCode.ItemName, material.ItemName },
                                { (byte)FetchMaterialResponseParameterCode.Description, material.Description },
                                { (byte)FetchMaterialResponseParameterCode.MaterialID, material.MaterialID }
                            };
                            SendResponse(fetchCode, result);
                        }
                        else
                        {
                            var result = new Dictionary<byte, object>
                            {
                                { (byte)FetchItemResponseParameterCode.ItemID, item.ItemID },
                                { (byte)FetchItemResponseParameterCode.ItemName, item.ItemName },
                                { (byte)FetchItemResponseParameterCode.Description, item.Description },
                            };
                            SendResponse(fetchCode, result);
                        }
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (InvalidCastException ex)
                {
                    LogService.ErrorFormat("Fetch System Version Invalid Cast!");
                    LogService.Error(ex.Message);
                    LogService.Error(ex.StackTrace);
                    return false;
                }
                catch (Exception ex)
                {
                    LogService.Error(ex.Message);
                    LogService.Error(ex.StackTrace);
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