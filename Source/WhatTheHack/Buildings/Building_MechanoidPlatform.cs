using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.Buildings
{
    class Building_MechanoidPlatform : Building_Bed
    {
        public const int SLOTINDEX = 1;
        CompRefuelable refuelableComp;
        CompPowerTrader powerComp;
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.refuelableComp = base.GetComp<CompRefuelable>();
            this.powerComp = base.GetComp<CompPowerTrader>();
        }
        public bool CanHealNow()
        {
            return this.refuelableComp.HasFuel && this.powerComp != null && this.powerComp.PowerOn; ;
        }
    }
}
