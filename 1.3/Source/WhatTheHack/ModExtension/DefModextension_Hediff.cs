using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack
{
    public class DefModextension_Hediff : DefModExtension
    {
        public ThingDef extraButcherProduct = null;
        public bool hasAbility = false;
        public float armorOffset = 0f;
        public float powerRateOffset = 0f;
        public float powerProduction = 0f;
        public float batteryCapacityOffset = 0f;
        public float firingRateOffset = 0f;
        public float carryingCapacityOffset = 0f;
        public float spawnChance = 0f;
        public float destroyOnDeathChance = 0f;
        public bool canUninstall = false;
        public float repairRate = 0f;
    }
}
