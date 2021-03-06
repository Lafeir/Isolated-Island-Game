﻿using IsolatedIslandGame.Protocol;
using IsolatedIslandGame.Protocol.Communication.OperationCodes;
using IsolatedIslandGame.Protocol.Communication.ResponseParameters.Player;
using System;
using System.Collections.Generic;
namespace IsolatedIslandGame.Library.CommunicationInfrastructure.Responses.Handlers.PlayerResponseHandlers
{
    class SynthesizeMaterialResponseHandler : ResponseHandler<Player, PlayerOperationCode>
    {
        public SynthesizeMaterialResponseHandler(Player subject) : base(subject)
        {
        }

        internal override bool CheckError(Dictionary<byte, object> parameters, ErrorCode returnCode, string debugMessage)
        {
            switch (returnCode)
            {
                case ErrorCode.NoError:
                    {
                        if (parameters.Count != 2)
                        {
                            LogService.ErrorFormat(string.Format("SynthesizeMaterial Parameter Error, Parameter Count: {0}", parameters.Count));
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                case ErrorCode.InvalidOperation:
                    {
                        LogService.ErrorFormat("SynthesizeMaterial Error DebugMessage: {0}", debugMessage);
                        subject.EventManager.ErrorInform("錯誤", "沒有這種藍圖");
                        return false;
                    }
                case ErrorCode.PermissionDeny:
                    {
                        LogService.ErrorFormat("SynthesizeMaterial Error DebugMessage: {0}", debugMessage);
                        subject.EventManager.ErrorInform("錯誤", "素材不足");
                        return false;
                    }
                default:
                    {
                        LogService.ErrorFormat("SynthesizeMaterial Error DebugMessage: {0}", debugMessage);
                        subject.EventManager.ErrorInform("錯誤", "未知的錯誤種類");
                        return false;
                    }
            }
        }
        internal override bool Handle(PlayerOperationCode operationCode, ErrorCode returnCode, string debugMessage, Dictionary<byte, object> parameters)
        {
            if (base.Handle(operationCode, returnCode, debugMessage, parameters))
            {
                try
                {
                    Blueprint.ElementInfo[] requirements = (Blueprint.ElementInfo[])parameters[(byte)SynthesizeMaterialResponseParameterCode.Requirements];
                    Blueprint.ElementInfo[] products = (Blueprint.ElementInfo[])parameters[(byte)SynthesizeMaterialResponseParameterCode.Products];
                    subject.ResponseManager.SynthesizeMaterialResponse(returnCode, requirements, products);
                    return true;
                }
                catch (InvalidCastException ex)
                {
                    LogService.Error("SynthesizeMaterial Parameter Cast Error");
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
