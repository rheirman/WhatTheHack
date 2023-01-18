using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace WhatTheHack.Buildings;

public class Building_MechanoidPlatform : Building_BaseMechanoidPlatform
{
    public const float MINFUELREGENERATE = 4.0f;
    public CompPowerTrader powerComp;

    public override bool RegenerateActive => regenerateActive;

    public override bool RepairActive => repairActive;

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        refuelableComp = GetComp<CompRefuelable>();
        powerComp = GetComp<CompPowerTrader>();
        LessonAutoActivator.TeachOpportunity(WTH_DefOf.WTH_Platform, OpportunityType.Important);
    }

    public override bool CanHealNow()
    {
        return refuelableComp.HasFuel && HasPowerNow();
    }

    public override bool HasPowerNow()
    {
        return powerComp is { PowerOn: true };
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var g in base.GetGizmos())
        {
            yield return g;
        }

        Gizmo regenerateGizmo = new Command_Toggle
        {
            defaultLabel = "WTH_Gizmo_Regenerate_Label".Translate(),
            defaultDesc = "WTH_Gizmo_Regenerate_Description".Translate(MINFUELREGENERATE),
            icon = ContentFinder<Texture2D>.Get("Things/Mote_HealingCrossGreen"),
            isActive = () => regenerateActive,
            toggleAction = () => { regenerateActive = !regenerateActive; }
        };
        yield return regenerateGizmo;

        Gizmo repairGizmo = new Command_Toggle
        {
            defaultLabel = "WTH_Gizmo_Repair_Label".Translate(),
            defaultDesc = "WTH_Gizmo_Repair_Description".Translate(),
            icon = ContentFinder<Texture2D>.Get("Things/Mote_HealingCrossBlue"),
            isActive = () => repairActive,
            toggleAction = () => { repairActive = !repairActive; }
        };
        yield return repairGizmo;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref regenerateActive, "regenerateActive", true);
        Scribe_Values.Look(ref repairActive, "repairActive", true);
    }
}