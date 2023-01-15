using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using WhatTheHack.Recipes;

namespace WhatTheHack.Buildings;

public class Building_HackingTable : Building_Bed
{
    public const int SLOTINDEX = 2;

    private Func<string> getInspectStringFunc = null;

    //private Pawn occupiedByInt = null;
    //public Pawn OccupiedBy { get => occupiedByInt;}
    public CompPowerTrader powerComp;

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        powerComp = GetComp<CompPowerTrader>();
        LessonAutoActivator.TeachOpportunity(WTH_DefOf.WTH_Hacking, OpportunityType.Important);
    }

    public bool TryAddPawnForModification(Pawn pawn, RecipeDef recipeDef)
    {
        var bill = new Bill_Medical(recipeDef, null);
        var bodyparts = RecipeUtility.GetPartsToApplyOn(pawn, bill.recipe);

        if (pawn.health.surgeryBills.FirstShouldDoNow == null ||
            pawn.health.surgeryBills.FirstShouldDoNow.recipe != WTH_DefOf.WTH_HackMechanoid &&
            pawn.health.surgeryBills.FirstShouldDoNow.recipe != WTH_DefOf.WTH_InduceEmergencySignal)
        {
            pawn.health.surgeryBills.AddBill(bill);
            bill.Part = bodyparts.FirstOrDefault();
        }

        /*
        Need_Power powerNeed = pawn.needs.TryGetNeed<Need_Power>();
        if (powerNeed != null)
        {
            //discharge mech battery so pawns can work safely. 
            powerNeed.CurLevel = 0;
        }
        */
        pawn.jobs.StartJob(new Job(WTH_DefOf.WTH_Mechanoid_Rest, this), JobCondition.InterruptForced);
        if (pawn.jobs.curDriver != null)
        {
            pawn.jobs.posture = PawnPosture.LayingInBed;
        }

        return true;
    }

    public bool HasPowerNow()
    {
        return powerComp is { PowerOn: true };
    }
    //Compatibility with animals logic. Calls ThingWithComps.GetInspectString() instead of BuildingBed.GetInspectString (which is targeted by Animals Logic). Store the function for performance.
    //public override string GetInspectString()
    //{
    //    if(getInspectStringFunc == null)
    //    {
    //        var ptr = typeof(ThingWithComps).GetMethod("GetInspectString").MethodHandle.GetFunctionPointer();
    //        getInspectStringFunc = (Func<string>)Activator.CreateInstance(typeof(Func<string>), this, ptr);
    //    }
    //    return getInspectStringFunc();
    //}

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var gizmo in base.GetGizmos())
        {
            if (!(gizmo is Command_Toggle toggleCommand &&
                  (toggleCommand.icon.name == "AsMedical" || toggleCommand.icon.name == "AssignOwner")))
            {
                yield return gizmo;
            }
        }
    }
}