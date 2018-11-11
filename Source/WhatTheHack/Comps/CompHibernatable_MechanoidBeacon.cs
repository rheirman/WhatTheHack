using Harmony;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using WhatTheHack.Buildings;

namespace WhatTheHack.Comps
{
    class CompHibernatable_MechanoidBeacon : CompHibernatable
    {
        public override void CompTick()
        {
            if (this.State == HibernatableStateDefOf.Starting && Find.TickManager.TicksGame > Traverse.Create(this).Field("endStartupTick").GetValue<int>() )
            {
                this.State = HibernatableStateDefOf.Running;
                Traverse.Create(this).Field("endStartupTick").SetValue(0);
                if(this.parent.Map.listerBuildings.allBuildingsColonist.FirstOrDefault((Building b) => b is Building_RogueAI) is Building_RogueAI rogueAI)
                {
                    Find.LetterStack.ReceiveLetter("WTH_MechanoidBeaconComplete_Label".Translate(), "WTH_MechanoidBeaconComplete_Description".Translate(), LetterDefOf.PositiveEvent, new GlobalTargetInfo(this.parent), null, null);
                    rogueAI.IsConscious = true;
                }
            }
        }
    }
}
