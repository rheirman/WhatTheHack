using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace WhatTheHack.Jobs;

internal class WorkGiver_HaulMechanoid : WorkGiver_Scanner
{
    public override Danger MaxPathDanger(Pawn pawn)
    {
        return Danger.Deadly;
    }

    public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
    {
        return pawn.Map.mapPawns.AllPawns.Where(p => p.IsMechanoid() && HealthAIUtility.ShouldHaveSurgeryDoneNow(p));
    }

    public override bool ShouldSkip(Pawn pawn, bool forced = false)
    {
        return !PotentialWorkThingsGlobal(pawn).Any();
    }

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        Job result = null;
        var mech = t as Pawn;
        if (pawn == mech)
        {
            return null;
        }

        if (mech == null || !PawnCanAutomaticallyHaulFast(pawn, t, forced))
        {
            return null;
        }

        var closestAvailableTable = Utilities.GetAvailableHackingTable(pawn, mech);

        if (closestAvailableTable != null && !mech.OnHackingTable())
        {
            result = new Job(WTH_DefOf.WTH_CarryToHackingTable, t, closestAvailableTable) { count = 1 };
        }

        return result;
    }

    //Copied from vanilla to prevent it from being broken by other mods. HaulExplicitly for instance would break this otherwise. 
    private static bool PawnCanAutomaticallyHaulFast(Pawn p, Thing t, bool forced)
    {
        if (t is UnfinishedThing { BoundBill: { } })
        {
            return false;
        }

        if (!p.CanReach(t, PathEndMode.ClosestTouch, p.NormalMaxDanger()))
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
            JobFailReason.Is(HaulAIUtility.ReservedForPrisonersTrans);
            //JobFailReason.Is(Traverse.Create(typeof(HaulAIUtility)).Field("ReservedForPrisonersTrans").GetValue<string>());
            return false;
        }

        if (!t.IsBurning())
        {
            return true;
        }

        JobFailReason.Is(HaulAIUtility.BurningLowerTrans);
        //JobFailReason.Is(Traverse.Create(typeof(HaulAIUtility)).Field("BurningLowerTrans").GetValue<string>());
        return false;
    }
}