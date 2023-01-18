using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Random = System.Random;

namespace WhatTheHack.Needs;

public class Need_Maintenance : Need
{
    private float lastLevel;
    public float maintenanceThreshold = 0.2f;

    public Need_Maintenance(Pawn pawn) : base(pawn)
    {
    }

    private int NeededPartsForFullRecovery
    {
        get
        {
            var combatPowerCapped = pawn.kindDef.combatPower <= 10000 ? pawn.kindDef.combatPower : 300;
            return Mathf.RoundToInt(combatPowerCapped / 30f);
        }
    }

    public float PercentageThreshVeryLowMaintenance => 0.1f; //TODO

    public float PercentageThreshLowMaintenance => 0.2f; //TODO

    public MaintenanceCategory CurCategory
    {
        get
        {
            if (CurLevelPercentage < PercentageThreshVeryLowMaintenance)
            {
                return MaintenanceCategory.VeryLowMaintenance;
            }

            return CurLevelPercentage < PercentageThreshLowMaintenance
                ? MaintenanceCategory.LowMaintenance
                : MaintenanceCategory.EnoughMaintenance;
        }
    }

    public override int GUIChangeArrow
    {
        get
        {
            if (CurLevel > lastLevel)
            {
                return 1;
            }

            if (CurLevel < lastLevel)
            {
                return -1;
            }

            return 0;
        }
    }

    public override float MaxLevel => 100f;

    public float MaintenanceWanted => MaxLevel - CurLevel;

    public override void SetInitialLevel()
    {
        CurLevelPercentage = 1.0f;
        lastLevel = 1.0f;
    }

    public int PartsNeededToRestore()
    {
        float totalParts = NeededPartsForFullRecovery;
        var fractionNeeded = MaintenanceWanted / MaxLevel;
        return Mathf.RoundToInt(totalParts * fractionNeeded);
    }

    public void RestoreUsingParts(int partCount)
    {
        var restoreFraction = partCount / (float)NeededPartsForFullRecovery;
        CurLevel += MaxLevel * restoreFraction;
    }

    private float FallPerTick()
    {
        if (pawn.health != null && pawn.health.hediffSet.HasNaturallyHealingInjury()) //damaged pawns detoriate quicker
        {
            return MaxLevel / (GenDate.TicksPerDay * 8f);
        }

        return MaxLevel / (GenDate.TicksPerDay * 12f);
    }

    public override void NeedInterval()
    {
        if (!base.IsFrozen)
        {
            lastLevel = CurLevel;
            if (Base.maintenanceDecayEnabled)
            {
                CurLevel -= FallPerTick() * 150f * Base.maintenanceDecayModifier;
            }
        }

        if (CurCategory == MaintenanceCategory.VeryLowMaintenance)
        {
            MaybeUnhackMechanoid();
        }

        SetHediffs();
    }

    private void MaybeUnhackMechanoid()
    {
        if (pawn.Map == null ||
            pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_ReplacedAI)) //TODO make unhacking during caravan possible
        {
            return;
        }

        var rand = new Random(DateTime.Now.Millisecond);
        var rndInt = rand.Next(1, 1000);
        float maxChanceProm = 10;
        var maxVeryLow = PercentageThreshVeryLowMaintenance * MaxLevel;
        var factor = 1f - (CurLevel / maxVeryLow);
        var chanceProm = Mathf.RoundToInt(factor * maxChanceProm);
        if (rndInt <= chanceProm)
        {
            UnHackMechanoid(pawn);
        }
    }

    public void SetHediffs()
    {
        TrySetHediff(MaintenanceCategory.LowMaintenance, WTH_DefOf.WTH_LowMaintenance);
        TrySetHediff(MaintenanceCategory.VeryLowMaintenance, WTH_DefOf.WTH_VeryLowMaintenance);
    }

    private void TrySetHediff(MaintenanceCategory cat, HediffDef hediffDef)
    {
        if (CurCategory != cat && pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef) is { } hediff)
        {
            pawn.health.RemoveHediff(hediff);
        }

        if (CurCategory != cat || pawn.health.hediffSet.HasHediff(hediffDef))
        {
            return;
        }

        var addedHeddif = HediffMaker.MakeHediff(hediffDef, pawn);
        pawn.health.AddHediff(addedHeddif);
    }


    public override string GetTipString()
    {
        return string.Concat(LabelCap, ": ", CurLevelPercentage.ToStringPercent(), " (", CurLevel.ToString("0.##"),
            " / ", MaxLevel.ToString("0.##"), ")\n", def.description);
    }

    public override void DrawOnGUI(Rect rect, int maxThresholdMarkers = int.MaxValue, float customMargin = -1,
        bool drawArrows = true, bool doTooltip = true, Rect? rectForTooltip = null, bool drawLabel = true)
    {
        if (threshPercents == null)
        {
            threshPercents = new List<float>();
        }

        threshPercents.Clear();
        threshPercents.Add(PercentageThreshLowMaintenance);
        threshPercents.Add(PercentageThreshVeryLowMaintenance);
        base.DrawOnGUI(rect, maxThresholdMarkers, customMargin, drawArrows, doTooltip, rectForTooltip, drawLabel);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref lastLevel, "lastLevel");
        Scribe_Values.Look(ref maintenanceThreshold, "maintenanceThreshold", 0.2f);
    }

    private static void UnHackMechanoid(Pawn pawn)
    {
        if (pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_TargetingHackedPoorly))
        {
            pawn.health.RemoveHediff(pawn.health.hediffSet.GetFirstHediffOfDef(WTH_DefOf.WTH_TargetingHackedPoorly));
        }

        if (pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_TargetingHacked))
        {
            pawn.health.RemoveHediff(pawn.health.hediffSet.GetFirstHediffOfDef(WTH_DefOf.WTH_TargetingHacked));
        }

        if (pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_BackupBattery))
        {
            pawn.health.RemoveHediff(pawn.health.hediffSet.GetFirstHediffOfDef(WTH_DefOf.WTH_BackupBattery));
        }

        if (pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_VeryLowPower))
        {
            pawn.health.RemoveHediff(pawn.health.hediffSet.GetFirstHediffOfDef(WTH_DefOf.WTH_VeryLowPower));
        }

        if (pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_NoPower))
        {
            pawn.health.RemoveHediff(pawn.health.hediffSet.GetFirstHediffOfDef(WTH_DefOf.WTH_NoPower));
        }

        pawn.SetFaction(Faction.OfMechanoids);
        pawn.story = null;
        if (pawn.GetLord() == null || pawn.GetLord().LordJob == null)
        {
            LordMaker.MakeNewLord(Faction.OfMechanoids, new LordJob_AssaultColony(Faction.OfMechanoids), pawn.Map,
                new List<Pawn> { pawn });
        }

        Find.LetterStack.ReceiveLetter("WTH_Letter_Mech_Reverted_Label".Translate(),
            "WTH_Letter_Mech_Reverted_Description".Translate(), LetterDefOf.ThreatBig, pawn);
    }
}