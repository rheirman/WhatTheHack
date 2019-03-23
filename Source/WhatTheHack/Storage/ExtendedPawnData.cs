using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using WhatTheHack.Buildings;
using WhatTheHack.Comps;

//Note: Currently this class contains information specific for other mods (caravanMount, caravanRider, etc), which is of course not ideal for a core framework. Ideally it should be completely generic. However I have yet to come up with an
// way to do this properly without introducing a lot of extra work. So for now I'll just keep it as it is. 

namespace WhatTheHack.Storage
{
    public class ExtendedPawnData : IExposable
    {

        public bool isHacked = false;
        public bool canWorkNow = false;
        public bool isActive = false;
        public bool shouldExplodeNow = false;//obsolete. Still here for compatibility with old saves.

        public Pawn remoteControlLink = null;
        public Building_RogueAI controllingAI = null;
        public Faction originalFaction = null;
        public List<WorkTypeDef> workTypes = null;
        public CompMountable turretMount = null;


        public Building_PortableChargingPlatform caravanPlatform = null;

        public void ExposeData()
        {
            Scribe_Values.Look(ref isHacked, "isHacked", false);
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
