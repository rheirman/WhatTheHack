using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WhatTheHack.Buildings
{
    abstract class Building_BaseMechanoidPlatform : Building_Bed
    {
        private bool regenerateActive = false;
        private bool repairActive = false;
        public CompRefuelable refuelableComp;
        public const int SLOTINDEX = 0;

        public virtual bool RegenerateActive { get => regenerateActive; }
        public virtual bool RepairActive { get => repairActive;}

        public abstract bool CanHealNow();
        public abstract bool HasPowerNow();

    }
}
