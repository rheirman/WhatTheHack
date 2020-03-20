using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;
using Verse.AI;
using WhatTheHack.Buildings;
using WhatTheHack.Comps;

//Used for mechanoids only.  

namespace WhatTheHack.Storage
{
    public class ExtendedPawnData : IExposable
    {

        //MAKE SURE TO UPDATE SHOULDCLEAN WHEN ADDING FIELDS!!!!

        public bool isHacked = false;//obsolete. 
        public bool canWorkNow = false;
        public bool isActive = false;
        public bool shouldExplodeNow = false;//obsolete. Still here for compatibility with old saves.

        public Pawn remoteControlLink = null;
        public Building_RogueAI controllingAI = null;
        public Faction originalFaction = null;
        public List<WorkTypeDef> workTypes = null;
        public CompMountable turretMount = null;
        public Building_PortableChargingPlatform caravanPlatform = null;


        public bool ShouldSave(Pawn pawn)
        {
            if ((pawn.IsColonist || pawn.RaceProps.IsMechanoid) && !(pawn.Dead || pawn.Destroyed))
            {
                return true;
            }
            return false;
        }

        public bool ShouldClean()
        {
            bool foundValue = false;
            foreach(FieldInfo fi in this.GetType().GetFields())
            {
                var fival = fi.GetValue(this);

                if(fival is bool val && val == true)
                {
                    foundValue = true; 
                }
                else if (fival != null && !(fival is int) && !(fival is float) && !(fival is bool))
                {
                    foundValue = true; 
                }
            }
            if (!foundValue)
            {
                return true;
            }
            return false;
        }
        public void ExposeData()
        {
            Scribe_Values.Look(ref isHacked, "isHacked", false);//Obsolete. 
            Scribe_Values.Look(ref canWorkNow, "canWorkNow", false);
            Scribe_Values.Look(ref shouldExplodeNow, "shouldExplodeNow", false);
            Scribe_Values.Look(ref isActive, "isActive", false);
            Scribe_References.Look(ref remoteControlLink, "remoteControlLink");
            Scribe_References.Look(ref controllingAI, "controllingAI");
            Scribe_References.Look(ref caravanPlatform, "caravanPlatform");
            Scribe_References.Look(ref originalFaction, "originalFaction");
            Scribe_Deep.Look(ref turretMount, "mountedTo");
            Scribe_Collections.Look(ref workTypes, "workTypes");
        }


    }
}
