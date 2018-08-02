using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace WhatTheHack
{
    [DefOf]
    class WTH_DefOf
    {
        public static HediffDef WTH_TargetingHacked;
        public static HediffDef WTH_ReplacedAI;
        public static HediffDef WTH_VeryLowPower;
        public static HediffDef WTH_NoPower;
        public static HediffDef WTH_TargetingHackedPoorly;
        public static HediffDef WTH_LocomotionHacked;
        public static HediffDef WTH_RegeneratedPart;
        public static HediffDef WTH_RepairModule;

        public static DamageDef WTH_RegeneratedPartDamage;

        public static RecipeDef WTH_HackMechanoid;
        
        public static DutyDef WTH_SearchAndDestroy;
        public static DutyDef WTH_ControlMechanoidDuty;

        public static JobDef WTH_CarryToHackingTable;
        public static JobDef WTH_ClearHackingTable;
        public static JobDef WTH_Mechanoid_Rest;
        public static JobDef WTH_ControlMechanoid;
        public static JobDef WTH_ControlMechanoid_Goto;

        public static NeedDef WTH_Mechanoid_Power;

        public static ThingDef WTH_HackingTable;
        public static ThingDef WTH_MechanoidPlatform;
        public static ThingDef WTH_PortableChargingPlatform;
        public static ThingDef WTH_MechanoidParts;
        public static ThingDef WTH_MechanoidChip;
        public static ThingDef WTH_Mote_Charging;
        public static ThingDef WTH_Mote_HealingCrossGreen;

        public static StatDef WTH_HackingSuccessChance;
    }
}
