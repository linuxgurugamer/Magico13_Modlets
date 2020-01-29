using System;
using System.Collections.Generic;
using UnityEngine;

namespace NotInMyBackYard
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class NIMBYEvent : MonoBehaviour
    {
        public static EventData<Vessel> OnVesselsTagged;
        public static NIMBYEvent Instance;
        public List<Guid> taggedVessels = new List<Guid>();
        private void Awake()
        {
            DontDestroyOnLoad(this);
            OnVesselsTagged = new EventData<Vessel>("OnVesselsTagged");
            Instance = this;
        }

        public void TagVessel(Vessel v)
        {
            if (taggedVessels.Contains(v.id)) return;
            taggedVessels.Add(v.id);
            ScreenMessages.PostScreenMessage(v.vesselName + " marked for recovery");
            Debug.Log("[NIMBY]: "+v.vesselName+" tagged for recovery");
        }
    }
}