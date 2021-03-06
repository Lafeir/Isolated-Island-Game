﻿using IsolatedIslandGame.Protocol;
using System.Collections.Generic;

namespace IsolatedIslandGame.Library
{
    public abstract class VesselManager
    {
        public static VesselManager Instance { get; private set; }

        public static void Initial(VesselManager vesselManager)
        {
            Instance = vesselManager;
        }

        protected Dictionary<int, Vessel> vesselDictionary;
        protected Dictionary<int, Vessel> vesselDictionaryByOwnerPlayerID;
        public IEnumerable<Vessel> Vessels { get { return vesselDictionary.Values; } }
        public int VesselCount { get { return vesselDictionary.Count; } }

        public delegate void VesselChangeEventHandler(DataChangeType changeType, Vessel vessel);
        public abstract event VesselChangeEventHandler OnVesselChange;

        public abstract event Vessel.VesselTransformUpdatedEventHandler OnVesselTransformUpdated;
        public abstract event Vessel.DecorationChangeEventHandler OnVesselDecorationChange;

        protected VesselManager()
        {
            vesselDictionary = new Dictionary<int, Vessel>();
            vesselDictionaryByOwnerPlayerID = new Dictionary<int, Vessel>();
        }

        public bool ContainsVessel(int vesselID)
        {
            return vesselDictionary.ContainsKey(vesselID);
        }
        public bool ContainsVesselWithOwnerPlayerID(int ownerPlayerID)
        {
            return vesselDictionaryByOwnerPlayerID.ContainsKey(ownerPlayerID);
        }
        public abstract bool FindVessel(int vesselID, out Vessel vessel);
        public abstract bool FindVesselByOwnerPlayerID(int ownerPlayerID, out Vessel vessel);
        public abstract void AddVessel(Vessel vessel);
        public abstract bool RemoveVessel(int vesselID);
    }
}
