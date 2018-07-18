using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.Buildings
{
    class Building_PortableChargingPlatform : Building_BaseMechanoidPlatform
    {
        public new const int SLOTINDEX = 1;
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.refuelableComp = base.GetComp<CompRefuelable>();
        }

        public override bool CanHealNow()
        {
            return false;
        }

        public override bool HasPowerNow()
        {
            return this.refuelableComp.Fuel > 0;
        }
    }
}
