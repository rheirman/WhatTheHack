using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace WhatTheHack.Needs
{
    public class Need_Power : Need
    {
        private const float BasePowerFallPerTick = 2.66666666E-05f;
        private float lastLevel = 0;

        //private const float BaseMalnutritionSeverityPerDay = 0.17f;

        //private const float BaseMalnutritionSeverityPerInterval = 0.00113333331f;

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
                return 0.25f;//TODO
            }
        }

        public float NutritionBetweenHungryAndFed
        {
            get
            {
                return (1f - this.PercentageThreshLowPower) * this.MaxLevel;//TODO
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
                Hediff noPowerHediff = pawn.health.hediffSet.GetFirstHediffOfDef(WTH_DefOf.NoPower, false);
                if (noPowerHediff != null)
                {
                    pawn.health.RemoveHediff(noPowerHediff);
                }
            }
            else if(!pawn.health.hediffSet.HasHediff(WTH_DefOf.NoPower))
            {
                Hediff noPowerHediff = HediffMaker.MakeHediff(WTH_DefOf.NoPower, pawn);
                pawn.health.AddHediff(noPowerHediff);
            }
            if (CurCategory != PowerCategory.VeryLowPower)
            {
                Hediff veryLowPowerHediff = pawn.health.hediffSet.GetFirstHediffOfDef(WTH_DefOf.VeryLowPower, false);
                if (veryLowPowerHediff != null)
                {
                    pawn.health.RemoveHediff(veryLowPowerHediff);
                }
            }
            else if (!pawn.health.hediffSet.HasHediff(WTH_DefOf.VeryLowPower))
            {
                Hediff veryLowPowerHediff = HediffMaker.MakeHediff(WTH_DefOf.VeryLowPower, pawn);
                pawn.health.AddHediff(veryLowPowerHediff);
            }
            
        }

        public float PowerFallPerTick
        {
            get
            {
                return this.PowerFallPerTickAssumingCategory(this.CurCategory);
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
                return 300.0f;//TODO
            }
        }

        public float NutritionWanted
        {
            get
            {
                return this.MaxLevel - this.CurLevel;
            }
        }

        private float PowerRate
        {
            get
            {
                return 10.0f;//TODO - no magic number
            }
        }

        public Need_Power(Pawn pawn) : base(pawn)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref lastLevel, "lastLevel");
        }

        private float PowerFallPerTickAssumingCategory(PowerCategory cat)
        {
            if(cat == PowerCategory.NoPower || !base.pawn.IsActivated())
            {
                return 0;
            }
            return 2.66666666E-05f * this.PowerRate;
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

        public override void SetInitialLevel()
        {
            base.CurLevelPercentage = 0f;
            lastLevel = 0f;
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
    }
}
