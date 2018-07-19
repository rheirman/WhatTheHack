using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.Buildings
{
    public class Building_PortableChargingPlatform : Building_BaseMechanoidPlatform
    {
        private Pawn caravanPawn = null;
        public new const int SLOTINDEX = 1;

        public Pawn CaravanPawn
        {
            get
            {
                return caravanPawn;
            }
            set
            {
                caravanPawn = value;
            }
        }

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

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref caravanPawn, "caravanPawn");
        }
    }
}
