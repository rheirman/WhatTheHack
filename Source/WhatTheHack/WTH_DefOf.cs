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
    public class WTH_DefOf
    {
        public static HediffDef WTH_TargetingHacked;
        public static HediffDef WTH_BackupBattery;
        public static HediffDef WTH_ReplacedAI;
        public static HediffDef WTH_VeryLowPower;
        public static HediffDef WTH_NoPower;
        public static HediffDef WTH_LowMaintenance;
        public static HediffDef WTH_VeryLowMaintenance;
        public static HediffDef WTH_TargetingHackedPoorly;
        public static HediffDef WTH_RegeneratedPart;
        public static HediffDef WTH_RepairModule;
        public static HediffDef WTH_Repairing;
        public static HediffDef WTH_RepairArm;
        public static HediffDef WTH_SelfDestruct;
        public static HediffDef WTH_TurretModule;
        public static HediffDef WTH_MountedTurret;
        public static HediffDef WTH_BatteryExpansionModule;
        public static HediffDef WTH_VanometricModule;
        public static HediffDef WTH_BeltModule;

        public static DamageDef WTH_RegeneratedPartDamage;

        public static RecipeDef WTH_HackMechanoid;
        public static RecipeDef WTH_InduceEmergencySignal;
        public static RecipeDef WTH_Craft_VanometricModule;

        public static DutyDef WTH_SearchAndDestroy;
        public static DutyDef WTH_ControlMechanoidDuty;

        public static JobDef WTH_CarryToHackingTable;
        public static JobDef WTH_ClearHackingTable;
        public static JobDef WTH_Mechanoid_Rest;
        public static JobDef WTH_ControlMechanoid;
        public static JobDef WTH_ControlMechanoid_Goto;
        public static JobDef WTH_Ability;
        public static JobDef WTH_PerformMaintenance;
        public static JobDef WTH_HackRogueAI;

        public static NeedDef WTH_Mechanoid_Power;
        public static NeedDef WTH_Mechanoid_Maintenance;

        public static ThingDef WTH_TableMechanoidWorkshop;
        public static ThingDef WTH_HackingTable;
        public static ThingDef WTH_MechanoidBeacon;
        public static ThingDef WTH_MechanoidPlatform;
        public static ThingDef WTH_PortableChargingPlatform;
        public static ThingDef WTH_MechanoidParts;
        public static ThingDef WTH_MechanoidChip;
        public static ThingDef WTH_Mote_Charging;
        public static ThingDef WTH_Mote_HealingCrossGreen;
        public static ThingDef WTH_Apparel_MechControllerBelt;
        public static ThingDef WTH_MechanoidData;
        public static ThingDef WTH_RogueAI;
        
        public static StatDef WTH_HackingSuccessChance;
        public static StatDef WTH_HackingMaintenanceSpeed;
        public static StatDef WTH_ControllerBeltRadius;

        public static ResearchProjectDef WTH_TurretModule_GunTurrets;
        public static ResearchProjectDef WTH_TurretModule_Mortars;
        public static ResearchProjectDef WTH_LRMSTuning;

        public static WorkTypeDef WTH_Hack;

        public static ConceptDef WTH_Hacking;
        public static ConceptDef WTH_Modification;
        public static ConceptDef WTH_Caravanning;
        public static ConceptDef WTH_Platform;
        public static ConceptDef WTH_Maintenance;
        public static ConceptDef WTH_Power;
        public static ConceptDef WTH_Concept_MechanoidParts;

        public static SiteCoreDef WTH_RoamingMechanoidsCore;
        public static SiteCoreDef WTH_MechanoidTempleCore;

        public static SitePartDef WTH_RoamingMechanoidsPart;
        public static SitePartDef WTH_MechanoidTemplePart;
        public static ThingSetMakerDef WTH_MapGen_MechanoidTempleContents;

        //vanilla
        public static IncidentDef ShortCircuit;
        public static BodyPartDef Reactor;
        public static BodyPartGroupDef Waist;

        public static DifficultyDef Peaceful;
        public static DifficultyDef Easy;
        public static DifficultyDef Medium;
        public static DifficultyDef Rough;
        public static DifficultyDef Hard;
        public static DifficultyDef Extreme;

    }
}
