using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using WhatTheHack.Buildings;

namespace WhatTheHack.Needs;

public class Need_Power : Need
{
    public float canStartWorkThreshold = 0.5f;
    private float lastLevel;
    public bool shouldAutoRecharge = true;

    public Need_Power(Pawn pawn) : base(pawn)
    {
    }

    public bool OutOfPower => CurCategory == PowerCategory.NoPower;

    public float PercentageThreshVeryLowPower => 0.1f; //TODO

    public float PercentageThreshLowPower => 0.2f; //TODO


    public PowerCategory CurCategory
    {
        get
        {
            if (CurLevelPercentage <= 0f)
            {
                return PowerCategory.NoPower;
            }

            if (CurLevelPercentage < PercentageThreshVeryLowPower)
            {
                return PowerCategory.VeryLowPower;
            }

            return CurLevelPercentage < PercentageThreshLowPower ? PowerCategory.LowPower : PowerCategory.EnoughPower;
        }
    }

    //Can be negative in case of power production. 
    public float PowerFallPerTick
    {
        get
        {
            var result = -PowerProduction;
            if (CurCategory == PowerCategory.NoPower)
            {
                return result;
            }

            if (DirectlyPowered(pawn))
            {
                return result;
            }

            if (pawn.HasValidCaravanPlatform() && pawn.GetCaravan() != null && pawn.GetCaravan().HasFuel())
            {
                return result;
            }

            result += PowerRate;
            return result;
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

    public override float MaxLevel => pawn.GetStatValue(WTH_DefOf.WTH_BatteryCapacity); //TODO

    private float PowerRate => pawn.GetStatValue(WTH_DefOf.WTH_PowerRate) / GenDate.TicksPerDay;

    private float PowerProduction
    {
        get
        {
            var result = pawn.GetStatValue(WTH_DefOf.WTH_PowerProduction);
            Building_BaseMechanoidPlatform platform = null;
            if (pawn.CurrentBed() is Building_BaseMechanoidPlatform mapPlatform && mapPlatform.HasPowerNow())
            {
                platform = mapPlatform;
            }
            else if (pawn.CaravanPlatform() is { } caravanPlatform &&
                     pawn.GetCaravan().HasFuel())
            {
                platform = caravanPlatform;
            }

            if (platform != null)
            {
                result += platform.GetStatValue(WTH_DefOf.WTH_RechargeRate);
            }

            result /= GenDate.TicksPerDay;
            return result;
        }
    }

    public override void NeedInterval()
    {
        if (!base.IsFrozen)
        {
            lastLevel = CurLevel;
            CurLevel -= PowerFallPerTick * 150f;
            if (ModsConfig.BiotechActive)
            {
                if (pawn.needs.energy == null)
                {
                    pawn.needs.energy = new Need_MechEnergy(pawn);
                }

                pawn.needs.energy.CurLevelPercentage = CurLevelPercentage;
            }
        }

        if (CurLevel > lastLevel)
        {
            FleckMaker.ThrowMetaIcon(pawn.Position, pawn.Map, WTH_DefOf.WTH_Fleck_Charging);
        }

        SetHediffs();
    }

    public void SetHediffs()
    {
        if (CurCategory != PowerCategory.NoPower)
        {
            var noPowerHediff = pawn.health.hediffSet.GetFirstHediffOfDef(WTH_DefOf.WTH_NoPower);
            if (noPowerHediff != null)
            {
                pawn.health.RemoveHediff(noPowerHediff);
            }
        }
        else if (!pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_NoPower))
        {
            var noPowerHediff = HediffMaker.MakeHediff(WTH_DefOf.WTH_NoPower, pawn);
            pawn.health.AddHediff(noPowerHediff);
        }

        if (CurCategory != PowerCategory.VeryLowPower)
        {
            var veryLowPowerHediff = pawn.health.hediffSet.GetFirstHediffOfDef(WTH_DefOf.WTH_VeryLowPower);
            if (veryLowPowerHediff != null)
            {
                pawn.health.RemoveHediff(veryLowPowerHediff);
            }
        }
        else if (!pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_VeryLowPower))
        {
            var veryLowPowerHediff = HediffMaker.MakeHediff(WTH_DefOf.WTH_VeryLowPower, pawn);
            pawn.health.AddHediff(veryLowPowerHediff);
        }
    }

    public bool DirectlyPowered(Pawn pawn)
    {
        return !this.pawn.IsActivated() && this.pawn.OnBaseMechanoidPlatform() &&
               ((Building_BaseMechanoidPlatform)this.pawn.CurrentBed()).HasPowerNow();
    }

    public override void SetInitialLevel()
    {
        CurLevelPercentage = 0;
        lastLevel = 0;
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
        threshPercents.Add(PercentageThreshLowPower);
        threshPercents.Add(PercentageThreshVeryLowPower);
        base.DrawOnGUI(rect, maxThresholdMarkers, customMargin, drawArrows, doTooltip, rectForTooltip, drawLabel);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref lastLevel, "lastLevel");
        Scribe_Values.Look(ref shouldAutoRecharge, "shoulAutoRecharge", true);
        Scribe_Values.Look(ref canStartWorkThreshold, "canStartWorkThreshold", 0.5f);
    }
}