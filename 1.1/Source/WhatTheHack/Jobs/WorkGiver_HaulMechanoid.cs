using HarmonyLib;
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
            if (pawn == mech)
            {
                return null;
            }
            if(mech != null && PawnCanAutomaticallyHaulFast(pawn, t, forced))
            {
                Building_HackingTable closestAvailableTable = Utilities.GetAvailableHackingTable(pawn, mech);

                if (closestAvailableTable != null && !mech.OnHackingTable())
                {
                    result = new Job(WTH_DefOf.WTH_CarryToHackingTable, t, closestAvailableTable) { count = 1};
                }
            }
            return result;
        }

        //Copied from vanilla to prevent it from being broken by other mods. HaulExplicitly for instance would break this otherwise. 
        private static bool PawnCanAutomaticallyHaulFast(Pawn p, Thing t, bool forced)
        {
            UnfinishedThing unfinishedThing = t as UnfinishedThing;
            if (unfinishedThing != null && unfinishedThing.BoundBill != null)
            {
                return false;
            }
            if (!p.CanReach(t, PathEndMode.ClosestTouch, p.NormalMaxDanger(), false, TraverseMode.ByPawn))
            {
                return false;
            }
            LocalTargetInfo target = t;
            if (!p.CanReserve(target, 1, -1, null, forced))
            {
                return false;
            }
            if (t.def.IsNutritionGivingIngestible && t.def.ingestible.HumanEdible && !t.IsSociallyProper(p, false, true))
            {
                JobFailReason.Is(Traverse.Create(typeof(HaulAIUtility)).Field("ReservedForPrisonersTrans").GetValue<string>(), null);
                return false;
            }
            if (t.IsBurning())
            {
                JobFailReason.Is(Traverse.Create(typeof(HaulAIUtility)).Field("BurningLowerTrans").GetValue<string>(), null);
                return false;
            }
            return true;
        }
    }
}
