﻿using IsolatedIslandGame.Library.CommunicationInfrastructure.Operations.Handlers.SystemOperationHandlers.FetchDataHandlers;
using IsolatedIslandGame.Protocol;
using IsolatedIslandGame.Protocol.Communication.FetchDataCodes;
using IsolatedIslandGame.Protocol.Communication.FetchDataParameters;
using IsolatedIslandGame.Protocol.Communication.FetchDataParameters.System;
using IsolatedIslandGame.Protocol.Communication.OperationCodes;
using System.Collections.Generic;

namespace IsolatedIslandGame.Library.CommunicationInfrastructure.Operations.Handlers.SystemOperationHandlers
{
    public class SystemFetchDataResolver : SystemOperationHandler
    {
        internal readonly Dictionary<SystemFetchDataCode, SystemFetchDataHandler> fetchTable;

        public SystemFetchDataResolver(SystemManager subject) : base(subject, 2)
        {
            fetchTable = new Dictionary<SystemFetchDataCode, SystemFetchDataHandler>
            {
                { SystemFetchDataCode.Item, new FetchItemHandler(subject) },
                { SystemFetchDataCode.AllVessels, new FetchAllVesselsHandler(subject) },
                { SystemFetchDataCode.Vessel, new FetchVesselHandler(subject) },
                { SystemFetchDataCode.VesselWithOwnerPlayerID, new FetchVesselWithOwnerPlayerIDHandler(subject) },
                { SystemFetchDataCode.VesselDecorations, new FetchVesselDecorationsHandler(subject) }
            };
        }

        internal override bool Handle(CommunicationInterface communicationInterface, SystemOperationCode operationCode, Dictionary<byte, object> parameters)
        {
            if (base.Handle(communicationInterface, operationCode, parameters))
            {
                string debugMessage;
                SystemFetchDataCode fetchCode = (SystemFetchDataCode)parameters[(byte)FetchDataParameterCode.FetchDataCode];
                Dictionary<byte, object> resolvedParameters = (Dictionary<byte, object>)parameters[(byte)FetchDataParameterCode.Parameters];
                if (fetchTable.ContainsKey(fetchCode))
                {
                    return fetchTable[fetchCode].Handle(communicationInterface, fetchCode, resolvedParameters);
                }
                else
                {
                    debugMessage = string.Format("System Fetch Operation Not Exist Fetch Code: {0}", fetchCode);
                    SendError(communicationInterface, operationCode, ErrorCode.InvalidOperation, debugMessage);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        internal void SendOperation(SystemFetchDataCode fetchCode, Dictionary<byte, object> parameters)
        {
            systemManager.OperationManager.SendFetchDataOperation(fetchCode, parameters);
        }

        public void FetchItem(int itemID)
        {
            var parameters = new Dictionary<byte, object>
            {
                { (byte)FetchItemParameterCode.ItemID, itemID }
            };
            SendOperation(SystemFetchDataCode.Item, parameters);
        }
        public void FetchAllVessels()
        {
            SendOperation(SystemFetchDataCode.AllVessels, new Dictionary<byte, object>());
        }
        public void FetchVessel(int vesselID)
        {
            var parameters = new Dictionary<byte, object>
            {
                { (byte)FetchVesselParameterCode.VesselID, vesselID }
            };
            SendOperation(SystemFetchDataCode.Vessel, parameters);
        }
        public void FetchVesselWithOwnerPlayerID(int ownerPlayerID)
        {
            var parameters = new Dictionary<byte, object>
            {
                { (byte)FetchVesselWithOwnerPlayerIDParameterCode.OwnerPlayerID, ownerPlayerID }
            };
            SendOperation(SystemFetchDataCode.VesselWithOwnerPlayerID, parameters);
        }
        public void FetchVesselDecorations(int vesselID)
        {
            var parameters = new Dictionary<byte, object>
            {
                { (byte)FetchVesselDecorationsParameterCode.VesselID, vesselID }
            };
            SendOperation(SystemFetchDataCode.VesselDecorations, parameters);
        }
    }
}
