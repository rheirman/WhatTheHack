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
        public static HediffDef TargetingHacked;
        public static HediffDef ReplacedAI;
        public static HediffDef VeryLowPower;
        public static HediffDef NoPower;
        public static HediffDef TargetingHackedPoorly;
        public static RecipeDef HackMechanoid;
        

        public static DutyDef SearchAndDestroy;

        public static JobDef CarryToHackingTable;
        public static JobDef ClearHackingTable;
        public static JobDef Mechanoid_Rest;

        public static NeedDef Mechanoid_Power;

        public static ThingDef HackingTable;
        public static ThingDef MechanoidPlatform;
        public static ThingDef MechanoidParts;
        public static ThingDef MechanoidChip;
        public static ThingDef Mote_Charging;
    }
}
