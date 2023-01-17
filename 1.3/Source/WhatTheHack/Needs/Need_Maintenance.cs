using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using WhatTheHack.ThinkTree;

namespace WhatTheHack.Needs
{
    public class Need_Maintenance : Need
    {
        private float lastLevel = 0;
        public float maintenanceThreshold = 0.2f;
        public Need_Maintenance(Pawn pawn) : base(pawn)
        {
            
        }

        public override void SetInitialLevel()
        {
            base.CurLevelPercentage = 1.0f;
            lastLevel = 1.0f;
        }
        
        public int PartsNeededToRestore()
        {
            float totalParts = NeededPartsForFullRecovery;
            float fractionNeeded = MaintenanceWanted/MaxLevel;
            return Mathf.RoundToInt(totalParts * fractionNeeded);
        }
        private int NeededPartsForFullRecovery {
            get
            {
                float combatPowerCapped = pawn.kindDef.combatPower <= 10000 ? pawn.kindDef.combatPower : 300;
                return Mathf.RoundToInt(combatPowerCapped / 30f);
            }
        }
        public void RestoreUsingParts(int partCount)
        {
            float restoreFraction =  (float) partCount / (float)NeededPartsForFullRecovery;
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
                this.lastLevel = CurLevel;
                if (Base.maintenanceDecayEnabled)
                {
                    this.CurLevel -= FallPerTick() * 150f * Base.maintenanceDecayModifier;
                }
            }
            if (this.CurCategory == MaintenanceCategory.VeryLowMaintenance)
            {
                MaybeUnhackMechanoid();
            }
            SetHediffs();
        }

        private void MaybeUnhackMechanoid()
        {
            if(pawn.Map != null && !pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_ReplacedAI))//TODO make unhacking during caravan possible
            {
                System.Random rand = new System.Random(DateTime.Now.Millisecond);
                int rndInt = rand.Next(1, 1000);
                float maxChanceProm = 10;
                float maxVeryLow = PercentageThreshVeryLowMaintenance * MaxLevel;
                float factor = 1f - (CurLevel / maxVeryLow);
                int chanceProm = Mathf.RoundToInt(factor * maxChanceProm);
                if (rndInt <= chanceProm)
                {
                    UnHackMechanoid(pawn);
                }
            }
        }

        public void SetHediffs()
        {
            TrySetHediff(MaintenanceCategory.LowMaintenance, WTH_DefOf.WTH_LowMaintenance);
            TrySetHediff(MaintenanceCategory.VeryLowMaintenance, WTH_DefOf.WTH_VeryLowMaintenance);
        }
        private void TrySetHediff(MaintenanceCategory cat, HediffDef hediffDef)
        {
            if (CurCategory != cat && pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef, false) is Hediff hediff)
            {
                pawn.health.RemoveHediff(hediff);
            }
            if (CurCategory == cat && !pawn.health.hediffSet.HasHediff(hediffDef))
            {
                Hediff addedHeddif = HediffMaker.MakeHediff(hediffDef, pawn);
                pawn.health.AddHediff(addedHeddif);
            }

        }

        public float PercentageThreshVeryLowMaintenance
        {
            get
            {
                return 0.1f;//TODO

            }
        }

        public float PercentageThreshLowMaintenance
        {
            get
            {
                return 0.2f;//TODO
            }
        }

        public MaintenanceCategory CurCategory
        {
            get
            {
                if (base.CurLevelPercentage < this.PercentageThreshVeryLowMaintenance)
                {
                    return MaintenanceCategory.VeryLowMaintenance;
                }
                if (base.CurLevelPercentage < this.PercentageThreshLowMaintenance)
                {
                    return MaintenanceCategory.LowMaintenance;
                }
                return MaintenanceCategory.EnoughMaintenance;
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
                else if (CurLevel < lastLevel)
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
                return 100f;
            }
        }
        public float MaintenanceWanted
        {
            get
            {
                return this.MaxLevel - this.CurLevel;
            }
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
        public override void DrawOnGUI(Rect rect, int maxThresholdMarkers = int.MaxValue, float customMargin = -1, bool drawArrows = true, bool doTooltip = true, Rect? rectForTooltip = null)
        {
            if (this.threshPercents == null)
            {
                this.threshPercents = new List<float>();
            }
            this.threshPercents.Clear();
            this.threshPercents.Add(this.PercentageThreshLowMaintenance);
            this.threshPercents.Add(this.PercentageThreshVeryLowMaintenance);
            base.DrawOnGUI(rect, maxThresholdMarkers, customMargin, drawArrows, doTooltip, rectForTooltip);
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref lastLevel, "lastLevel");
            Scribe_Values.Look<float>(ref maintenanceThreshold, "maintenanceThreshold", 0.2f);
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
                LordMaker.MakeNewLord(Faction.OfMechanoids, new LordJob_AssaultColony(Faction.OfMechanoids, true, true, false, false, true), pawn.Map, new List<Pawn> { pawn});
            }
            Find.LetterStack.ReceiveLetter("WTH_Letter_Mech_Reverted_Label".Translate(), "WTH_Letter_Mech_Reverted_Description".Translate(), LetterDefOf.ThreatBig, pawn);
        }
    }
}
