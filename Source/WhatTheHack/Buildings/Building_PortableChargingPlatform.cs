using RimWorld;
using Verse;

namespace WhatTheHack.Buildings;

public class Building_PortableChargingPlatform : Building_BaseMechanoidPlatform
{
    private Pawn caravanPawn;

    public Pawn CaravanPawn
    {
        get => caravanPawn;
        set => caravanPawn = value;
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        refuelableComp = GetComp<CompRefuelable>();
        LessonAutoActivator.TeachOpportunity(WTH_DefOf.WTH_Caravanning, OpportunityType.Important);
    }

    public override bool CanHealNow()
    {
        return false;
    }

    public override bool HasPowerNow()
    {
        return refuelableComp.Fuel > 0;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref caravanPawn, "caravanPawn");
    }
}