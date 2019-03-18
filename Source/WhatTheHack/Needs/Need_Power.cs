using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using WhatTheHack.Buildings;
using WhatTheHack.Storage;

namespace WhatTheHack.Needs
{
    public class Need_Power : Need
    {
        private float lastLevel = 0;
        public bool shouldAutoRecharge = true;
        public float canStartWorkThreshold = 0;

        //private const float BaseMalnutritionSeverityPerDay = 0.17f;

        //private const float BaseMalnutritionSeverityPerInterval = 0.00113333331f;

        public Need_Power(Pawn pawn) : base(pawn)
        {
        }

        public override void NeedInterval()
        {
            if (!base.IsFrozen)
            {
                this.lastLevel = CurLevel;
                this.CurLevel -= this.PowerFallPerTick * 150f;
            }
            SetHediffs();
        }

        public bool OutOfPower
        {
            get
            {
                return this.CurCategory == PowerCategory.NoPower;
            }
        }

        public float PercentageThreshVeryLowPower
        {
            get
            {
                return 0.1f;//TODO

            }
        }

        public float PercentageThreshLowPower
        {
            get
            {
                return 0.2f;//TODO
            }
        }


        public PowerCategory CurCategory
        {
            get
            {
                if (base.CurLevelPercentage <= 0f)
                {
                    return PowerCategory.NoPower;
                }
                if (base.CurLevelPercentage < this.PercentageThreshVeryLowPower)
                {
                    return PowerCategory.VeryLowPower;
                }
                if (base.CurLevelPercentage < this.PercentageThreshLowPower)
                {
                    return PowerCategory.LowPower;
                }
                return PowerCategory.EnoughPower;
            }
        }
        public void SetHediffs(){
            if(CurCategory != PowerCategory.NoPower)
            {
                Hediff noPowerHediff = pawn.health.hediffSet.GetFirstHediffOfDef(WTH_DefOf.WTH_NoPower, false);
                if (noPowerHediff != null)
                {
                    pawn.health.RemoveHediff(noPowerHediff);
                }
            }
            else if(!pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_NoPower))
            {
                Hediff noPowerHediff = HediffMaker.MakeHediff(WTH_DefOf.WTH_NoPower, pawn);
                pawn.health.AddHediff(noPowerHediff);
            }
            if (CurCategory != PowerCategory.VeryLowPower)
            {
                Hediff veryLowPowerHediff = pawn.health.hediffSet.GetFirstHediffOfDef(WTH_DefOf.WTH_VeryLowPower, false);
                if (veryLowPowerHediff != null)
                {
                    pawn.health.RemoveHediff(veryLowPowerHediff);
                }
            }
            else if (!pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_VeryLowPower))
            {
                Hediff veryLowPowerHediff = HediffMaker.MakeHediff(WTH_DefOf.WTH_VeryLowPower, pawn);
                pawn.health.AddHediff(veryLowPowerHediff);
            }
            
        }

        public float PowerFallPerTick
        {
            get
            {

                if (pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_VanometricModule))
                {
                    return 0f;
                }
                else
                {
                    return this.PowerFallPerTickAssumingCategory(this.CurCategory);
                }
            }
        }

        public override int GUIChangeArrow
        {
            get
            {
                if(CurLevel > lastLevel)
                {
                    return 1;
                }
                else if(CurLevel < lastLevel)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
        }

        public override float MaxLevel
        {
            get
            {
                float result = 100 + pawn.BodySize * 100;
                float factor = 1;

                foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
                {
                    if (hediff.def.GetModExtension<DefModextension_Hediff>() is DefModextension_Hediff modExt)
                    {
                        if(modExt.batteryCapacityOffset > 0)
                        {
                            factor += modExt.batteryCapacityOffset;
                        }
                    }
                }
                result *= factor;
                return result;//TODO
            }
        }

        private float PowerRate
        {
            get
            {
                float result = 150 + pawn.BodySize * 150 * Base.powerFallModifier;//TODO - no magic number
                float factor = 1;

                foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
                {
                    if (hediff.def.GetModExtension<DefModextension_Hediff>() is DefModextension_Hediff modExt)
                    {
                        if (modExt.powerRateOffset > 0)
                        {
                            factor += modExt.powerRateOffset;
                        }
                    }
                }
                result *= factor;

                return result;
            }
        }

        private float PowerFallPerTickAssumingCategory(PowerCategory cat)
        {
            if(cat == PowerCategory.NoPower)
            {
                return 0;
            }
            if(DirectlyPowered(pawn))
            {
                return 0;
            }
            if (pawn.HasValidCaravanPlatform() && pawn.GetCaravan() != null && pawn.GetCaravan().HasFuel())
            {
                return 0;
            }
            return 2E-05f * this.PowerRate; //TODO no magic number;
        }
        public bool DirectlyPowered(Pawn pawn)
        {
            return !base.pawn.IsActivated() && base.pawn.OnBaseMechanoidPlatform() && ((Building_BaseMechanoidPlatform)base.pawn.CurrentBed()).HasPowerNow();
        }

        public override void SetInitialLevel()
        {
            base.CurLevelPercentage = 0;
            lastLevel = 0;
        }

        public override string GetTipString()
        {
            return string.Concat(new string[]
            {
                base.LabelCap,
                ": ",
                base.CurLevelPercentage.ToStringPercent(),
                " (",
                this.CurLevel.ToString("0.##"),
                " / ",
                this.MaxLevel.ToString("0.##"),
                ")\n",
                this.def.description
            });
        }

        public override void DrawOnGUI(Rect rect, int maxThresholdMarkers = 2147483647, float customMargin = -1f, bool drawArrows = true, bool doTooltip = true)
        {
            if (this.threshPercents == null)
            {
                this.threshPercents = new List<float>();
            }
            this.threshPercents.Clear();
            this.threshPercents.Add(this.PercentageThreshLowPower);
            this.threshPercents.Add(this.PercentageThreshVeryLowPower);
            base.DrawOnGUI(rect, maxThresholdMarkers, customMargin, drawArrows, doTooltip);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref lastLevel, "lastLevel");
            Scribe_Values.Look<bool>(ref shouldAutoRecharge, "shoulAutoRecharge", true);
            Scribe_Values.Look<float>(ref canStartWorkThreshold, "canStartWorkThreshold", 0.4f);
        }

    }
}
