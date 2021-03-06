﻿using IsolatedIslandGame.Protocol.Communication.FetchDataCodes;
using IsolatedIslandGame.Protocol.Communication.FetchDataParameters.System;
using IsolatedIslandGame.Protocol.Communication.FetchDataResponseParameters.System;
using System;
using System.Collections.Generic;

namespace IsolatedIslandGame.Library.CommunicationInfrastructure.Operations.Handlers.SystemOperationHandlers.FetchDataHandlers
{
    class FetchVesselWithOwnerPlayerIDHandler : SystemFetchDataHandler
    {
        public FetchVesselWithOwnerPlayerIDHandler(SystemManager subject) : base(subject, 1)
        {
        }

        public override bool Handle(CommunicationInterface communicationInterface, SystemFetchDataCode fetchCode, Dictionary<byte, object> parameter)
        {
            if (base.Handle(communicationInterface, fetchCode, parameter))
            {
                try
                {
                    int ownerPlayerID = (int)parameter[(byte)FetchVesselWithOwnerPlayerIDParameterCode.OwnerPlayerID];
                    Vessel vessel;
                    if(VesselManager.Instance.FindVesselByOwnerPlayerID(ownerPlayerID, out vessel))
                    {
                        var result = new Dictionary<byte, object>
                        {
                            { (byte)FetchVesselWithOwnerPlayerIDResponseParameterCode.VesselID, vessel.VesselID },
                            { (byte)FetchVesselWithOwnerPlayerIDResponseParameterCode.PlayerID, vessel.PlayerInformation.playerID },
                            { (byte)FetchVesselWithOwnerPlayerIDResponseParameterCode.Nickname, vessel.PlayerInformation.nickname },
                            { (byte)FetchVesselWithOwnerPlayerIDResponseParameterCode.Signature, vessel.PlayerInformation.signature },
                            { (byte)FetchVesselWithOwnerPlayerIDResponseParameterCode.GroupType, (byte)vessel.PlayerInformation.groupType },
                            { (byte)FetchVesselWithOwnerPlayerIDResponseParameterCode.LocationX, vessel.LocationX },
                            { (byte)FetchVesselWithOwnerPlayerIDResponseParameterCode.LocationZ, vessel.LocationZ },
                            { (byte)FetchVesselWithOwnerPlayerIDResponseParameterCode.EulerAngleY, vessel.RotationEulerAngleY }
                        };
                        SendResponse(communicationInterface, fetchCode, result);
                        return true;
                    }
                    else
                    {
                        LogService.ErrorFormat("FetchVesselWithOwnerPlayerID No Vessel OwnerPlayerID: {0}", ownerPlayerID);
                        return false;
                    }
                }
                catch (InvalidCastException ex)
                {
                    LogService.ErrorFormat("FetchVesselWithOwnerPlayerID Invalid Cast!");
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
