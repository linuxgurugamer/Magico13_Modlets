using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NotInMyBackYard
{
    /// <summary>
    /// This module represents a Beacon that is on a part. It has a limited range (only works when actively loaded), doesn't work on its own vessel,
    /// requires a lab and antenna, and requires an Engineer Kerbal to be present
    /// </summary>
    public class ModuleMobileRecoveryBeacon : PartModule, IBeacon
    {
        //(severedsolo) actually show the player that they have a beacon.
        [KSPField(isPersistant = false, guiActive = true, guiUnits = "m", guiName = "Mobile Beacon Range")]
        public int range = (int)NotInMyBackYard.MobileBeaconRange;

        [KSPField(isPersistant = false, guiActive = true, guiName ="Tagged for Recovery:")]
        public bool tagged = false;

        private void Start()
        {
            if (!HighLogic.LoadedSceneIsFlight) return;
            NIMBYEvent.OnVesselsTagged.Add(VesselTagged);
            tagged = NIMBYEvent.Instance.taggedVessels.Contains(vessel.id);
        }

        private void OnDisable()
        {
            if(NIMBYEvent.OnVesselsTagged != null) NIMBYEvent.OnVesselsTagged.Remove(VesselTagged);
        }

        //(severedsolo) Tag vessel if the event fires.
        private void VesselTagged(Vessel taggingVessel)
        {
            //Don't tag ourselves
            if (vessel == taggingVessel) return;
            //If not in range don't tag
            if (!StrictRequirementsMet) return;
            NIMBYEvent.Instance.TagVessel(vessel);
            tagged = true;
        }

        [KSPEvent(active = true, guiActive = true, guiActiveUnfocused = false, externalToEVAOnly = false, guiName = "Tag Vessels For Recovery")]
        public void TagVessels()
        {
            //Declare to all vessels with loaded PartModules (ie within physics range) that they are trying to be tagged.
            NIMBYEvent.OnVesselsTagged.Fire(vessel);
        }
        
        public bool StrictRequirementsMet
        {
            get
            {
                //make sure the vessel is loaded
                //if (!vessel.loaded || vessel.packed)
                //{
                //    return false;
                //}
                if (vessel.isActiveVessel) //can't be the active vessel
                {
                    return false;
                }

                return Active;
            }
        }

        public string Name
        {
            get
            {
                return vessel.GetDisplayName();
            }
            set { }
        }

        public double Latitude
        {
            get
            {
                return vessel.latitude;
            }
            set { }
        }

        public double Longitude
        {
            get
            {
                return vessel.longitude;
            }
            set { }
        }

        public double Range
        {
            get
            {
                return NotInMyBackYard.MobileBeaconRange;
            }
            set { }
        }

        public bool Active
        {
            get
            {
                return NotInMyBackYard.MobileBeaconRequirementsMet(vessel);
            }
            set { }
        }

        public bool CanRecoverVessel(Vessel vessel)
        {
            return StrictRequirementsMet && GreatCircleDistance(vessel.mainBody.Radius, vessel.latitude, vessel.longitude) < Range;
        }

        public double GreatCircleDistance(double radius, double latitude, double longitude)
        {
            return NotInMyBackYard.DefaultGreatCircleDistance(radius, Latitude, Longitude, latitude, longitude);
        }

        public double GreatCircleDistance(Vessel vessel)
        {
            return GreatCircleDistance(vessel.mainBody.Radius, vessel.latitude, vessel.longitude);
        }
    }
}
