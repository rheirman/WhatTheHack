using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WhatTheHack.Buildings
{
    public abstract class Building_BaseMechanoidPlatform : Building_Bed
    {
        private bool regenerateActive = true;
        private bool repairActive = true;
        public CompRefuelable refuelableComp;
        public const int SLOTINDEX = 0;

        public virtual bool RegenerateActive { get => regenerateActive; }
        public virtual bool RepairActive { get => repairActive;}

        public abstract bool CanHealNow();
        public abstract bool HasPowerNow();

    }
}
