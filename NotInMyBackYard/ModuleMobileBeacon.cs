using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NotInMyBackYard
{
    /// <summary>
    /// This module represents a Beacon that is on a part. It has a limited range (only works when actively loaded), doesn't work on its own vessel,
    /// requires a lab and antenna, and requires an Engineer Kerbal to be present
    /// </summary>
    public class ModuleMobileRecoveryBeacon : PartModule, IBeacon
    {
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
