using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using WhatTheHack.Buildings;

namespace WhatTheHack.Jobs
{
    class WorkGiver_HaulMechanoid : WorkGiver_Scanner
    {
        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return pawn.Map.mapPawns.AllPawns.Where((Pawn p) => p.RaceProps.IsMechanoid && HealthAIUtility.ShouldHaveSurgeryDoneNow(p)).Cast<Thing>();
        }

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            return PotentialWorkThingsGlobal(pawn).Count() == 0;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Job result = null;
            Pawn mech = t as Pawn;
            if(mech != null && HaulAIUtility.PawnCanAutomaticallyHaulFast(pawn, t, forced))
            {
                Building_HackingTable closestAvailableTable = Utilities.GetAvailableHackingTable(pawn, mech);

                if (!mech.OnHackingTable())
                {
                    result = new Job(WTH_DefOf.WTH_CarryToHackingTable, t, closestAvailableTable) { count = 1};
                }
            }
            return result;
        }
    }
}
