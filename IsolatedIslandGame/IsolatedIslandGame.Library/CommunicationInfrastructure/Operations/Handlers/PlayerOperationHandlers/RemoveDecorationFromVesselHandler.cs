﻿using IsolatedIslandGame.Library.Items;
using IsolatedIslandGame.Protocol.Communication.OperationCodes;
using IsolatedIslandGame.Protocol.Communication.OperationParameters.Player;
using System.Collections.Generic;

namespace IsolatedIslandGame.Library.CommunicationInfrastructure.Operations.Handlers.PlayerOperationHandlers
{
    class RemoveDecorationFromVesselHandler : PlayerOperationHandler
    {
        public RemoveDecorationFromVesselHandler(Player subject) : base(subject, 1)
        {
        }

        internal override bool Handle(PlayerOperationCode operationCode, Dictionary<byte, object> parameters)
        {
            if (base.Handle(operationCode, parameters))
            {
                int decorationID = (int)parameters[(byte)RemoveDecorationFromVesselParameterCode.DecorationID];

                lock(subject.Vessel)
                {
                    if (subject.Vessel.ContainsDecoration(decorationID))
                    {
                        Decoration decoration;
                        if(subject.Vessel.FindDecoration(decorationID, out decoration))
                        {
                            Item material = decoration.Material;
                            subject.Inventory.AddItem(material, 1);
                            subject.Vessel.RemoveDecoration(decorationID);
                            DecorationFactory.Instance.DeleteDecoration(decorationID);
                            LogService.InfoFormat("Player: {0}, RemoveDecorationFromVessel, MaterialItemID: {1}", subject.IdentityInformation, material.ItemID);
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        LogService.ErrorFormat("RemoveDecorationFromVessel error Player: {0}, the decoration is not existed DecorationID: {1}", subject.IdentityInformation, decorationID);
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
        }
    }
}
