using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using WhatTheHack.Needs;

namespace WhatTheHack.Buildings
{
    public class Building_MechanoidPlatform : Building_BaseMechanoidPlatform
    {
        public CompPowerTrader powerComp;
        public const float MINFUELREGENERATE = 4.0f;
         
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.refuelableComp = base.GetComp<CompRefuelable>();
            this.powerComp = base.GetComp<CompPowerTrader>();
            LessonAutoActivator.TeachOpportunity(WTH_DefOf.WTH_Platform, OpportunityType.Important);
        }
        public override bool RegenerateActive {
            get
            {
                return regenerateActive;
            }
        }
        public override bool RepairActive
        {
            get
            {
                return repairActive;
            }
        }
        public override bool CanHealNow()
        {
            return this.refuelableComp.HasFuel && this.HasPowerNow();
        }
        public override bool HasPowerNow()
        {
            return this.powerComp != null && this.powerComp.PowerOn; 
        }
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }
            Gizmo regenerateGizmo = new Command_Toggle
            {
                defaultLabel = "WTH_Gizmo_Regenerate_Label".Translate(),
                defaultDesc = "WTH_Gizmo_Regenerate_Description".Translate(new object[] { MINFUELREGENERATE }),
                icon = ContentFinder<Texture2D>.Get(("Things/Mote_HealingCrossGreen"), true),
                isActive = () => regenerateActive,
                toggleAction = () =>
                {
                    regenerateActive = !regenerateActive;
                }
            };
            yield return regenerateGizmo;

            Gizmo repairGizmo = new Command_Toggle
            {
                defaultLabel = "WTH_Gizmo_Repair_Label".Translate(),
                defaultDesc = "WTH_Gizmo_Repair_Description".Translate(),
                icon = ContentFinder<Texture2D>.Get(("Things/Mote_HealingCrossBlue"), true),
                isActive = () => repairActive,
                toggleAction = () =>
                {
                    repairActive = !repairActive;
                }
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
}
