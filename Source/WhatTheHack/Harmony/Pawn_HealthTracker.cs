using Harmony;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using Verse;
using WhatTheHack.Buildings;
using WhatTheHack.Needs;
using WhatTheHack.Storage;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(Pawn_HealthTracker), "HasHediffsNeedingTend")]
    static class Pawn_HealthTracker_HasHediffsNeedingTend
    {
        static bool Prefix(Pawn_HealthTracker __instance)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (pawn.RaceProps.IsMechanoid && pawn.IsHacked())
            {
                return false;
            }
            return true;
        }
    }

    //Make sure mechanoids can be downed like other pawns. 
    [HarmonyPatch(typeof(Pawn_HealthTracker), "CheckForStateChange")]
    static class Pawn_HealthTracker_CheckForStateChange
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            bool flag = false;
            var instructionsList = new List<CodeInstruction>(instructions);
            for (var i = 0; i < instructionsList.Count; i++)
            {
                CodeInstruction instruction = instructionsList[i];

                if (instruction.operand == typeof(RaceProperties).GetMethod("get_IsMechanoid"))
                {
                    flag = true;
                }
                if(flag && instruction.opcode == OpCodes.Ldc_R4)
                {
                    //yield return new CodeInstruction(OpCodes.Call, typeof(Pawn_HealthTracker_CheckForStateChange).GetMethod(""))
                    yield return new CodeInstruction(OpCodes.Ldc_R4,0.5f);//TODO: no magic number? 
                    flag = false;
                }
                else
                {
                    yield return instruction;
                }
            }
        }
    }
    //Deactivates mechanoid when downed
    [HarmonyPatch(typeof(Pawn_HealthTracker), "MakeDowned")]
    static class Pawn_HealthTracker_MakeDowned
    {
        static void Postfix(Pawn_HealthTracker __instance)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            ExtendedDataStorage store = Base.Instance.GetExtendedDataStorage();
            if(store == null)
            {
                return;
            }
            ExtendedPawnData pawnData = store.GetExtendedDataFor(pawn);
            pawnData.isActive = false;
            pawn.RemoveRemoteControlLink();
        }
    }
    //Recharge and repair mechanoid when on platform
    //TODO: refactor. Move all needs related stuff to something needs related
    [HarmonyPatch(typeof(Pawn_HealthTracker), "HealthTick")]
    static class Pawn_HealthTracker_HealthTick
    {
        static void Postfix(Pawn_HealthTracker __instance)
        {

            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (!pawn.RaceProps.IsMechanoid)
            {
                return;
            }
            if (pawn.HasValidCaravanPlatform() && pawn.GetCaravan() != null && pawn.GetCaravan().HasFuel())
            {
                float powerPerTick = 0.5f * WTH_DefOf.WTH_PortableChargingPlatform.GetCompProperties<CompProperties_Refuelable>().fuelConsumptionRate * 15 / GenDate.TicksPerDay; //TODO: no magic number
                RechargeMechanoid(pawn, pawn.needs.TryGetNeed(WTH_DefOf.WTH_Mechanoid_Power), powerPerTick);
            }
            if (!(pawn.CurrentBed() is Building_BaseMechanoidPlatform))
            {
                return;
            }

            Building_BaseMechanoidPlatform platform = (Building_BaseMechanoidPlatform)pawn.CurrentBed();

            if (platform.RepairActive && __instance.hediffSet.HasNaturallyHealingInjury())
            {
                if (pawn.IsHashIntervalTick(10) && platform.CanHealNow())
                {
                    TryHealRandomInjury(__instance, pawn, platform);
                }
            }
            if (platform.RegenerateActive && pawn.IsHashIntervalTick(100) && platform.refuelableComp.Fuel > 4f) //TODO: no magic number
            {
                TryRegeneratePart(pawn, platform);
                RegainWeapon(pawn);

            }

            if (platform.HasPowerNow())
            {
                Need powerNeed = pawn.needs.TryGetNeed(WTH_DefOf.WTH_Mechanoid_Power);
                float powerPerTick = 0;
                if (platform.PowerComp != null)
                {
                    powerPerTick = 0.75f * platform.PowerComp.Props.basePowerConsumption / GenDate.TicksPerDay; //TODO: no magic number
                }
                else
                {
                    platform.refuelableComp.ConsumeFuel(platform.refuelableComp.Props.fuelConsumptionRate / GenDate.TicksPerDay);
                    powerPerTick = 0.75f * platform.refuelableComp.Props.fuelConsumptionRate * 15 / GenDate.TicksPerDay; //TODO: no magic number
                }

                RechargeMechanoid(pawn, powerNeed, powerPerTick);
            }
        }

        private static void RegainWeapon(Pawn pawn)
        {
            if(pawn.equipment.Primary == null)
            {
                PawnWeaponGenerator.TryGenerateWeaponFor(pawn);
            }
        }

        private static void RechargeMechanoid(Pawn pawn, Need powerNeed, float powerPerTick)
        {
            if (powerNeed.CurLevel + powerPerTick < powerNeed.MaxLevel)
            {
                if (pawn.IsHashIntervalTick(100))
                {
                    MoteMaker.ThrowMetaIcon(pawn.Position, pawn.Map, WTH_DefOf.WTH_Mote_Charging);
                }
                powerNeed.CurLevel += powerPerTick;
            }
            else if (powerNeed.CurLevel < powerNeed.MaxLevel)
            {
                powerNeed.CurLevel = powerNeed.MaxLevel;
            }
        }

        private static void TryRegeneratePart(Pawn pawn, Building_BaseMechanoidPlatform platform)
        {
            Hediff_MissingPart hediff = FindBiggestMissingBodyPart(pawn);
            if(hediff == null || pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_RegeneratedPart))
            {
                return;
            }
            
            pawn.health.RemoveHediff(hediff);
            platform.refuelableComp.ConsumeFuel(4f);
            //Hediff_Injury injury = new Hediff_Injury();
            DamageWorker_AddInjury addInjury = new DamageWorker_AddInjury();
            addInjury.Apply(new DamageInfo(WTH_DefOf.WTH_RegeneratedPartDamage, hediff.Part.def.GetMaxHealth(pawn) - 1, 0, -1, pawn, hediff.Part), pawn);

                //pawn.health.AddHediff(WTH_DefOf.WTH_RegeneratedPart);

            
        }

        //almost literal copy vanilla CompUseEffect_FixWorstHealthCondition.FindBiggestMissingBodyPart, only returns the hediff instead. 
        private static Hediff_MissingPart FindBiggestMissingBodyPart(Pawn pawn, float minCoverage = 0f)
        {
            Hediff_MissingPart hediff = null;
            foreach (Hediff_MissingPart current in pawn.health.hediffSet.GetMissingPartsCommonAncestors())
            {
                if (current.Part.coverageAbsWithChildren >= minCoverage)
                {
                    if (!pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(current.Part))
                    {
                        if (hediff == null || current.Part.coverageAbsWithChildren > hediff.Part.coverageAbsWithChildren)
                        {
                            hediff = current;
                        }
                    }
                }
            }
            return hediff;
        }

        private static void TryHealRandomInjury(Pawn_HealthTracker __instance, Pawn pawn, Building_BaseMechanoidPlatform platform)
        {
            IEnumerable<Hediff_Injury> hediffs = __instance.hediffSet.GetHediffs<Hediff_Injury>().Where((Hediff_Injury i) => HediffUtility.CanHealNaturally(i));
            if (hediffs.Count() == 0)
            {
                return;
            }
            Hediff_Injury hediff_Injury = hediffs.RandomElement();
            float healAmount = (platform.def.building.bed_healPerDay / RimWorld.GenDate.TicksPerDay) * pawn.HealthScale;
            hediff_Injury.Heal(healAmount);
            if (pawn.IsHashIntervalTick(50) && !pawn.IsHashIntervalTick(100) && !pawn.Position.Fogged(pawn.Map))
            {
                MoteMaker.ThrowMetaIcon(pawn.Position, pawn.Map, ThingDefOf.Mote_HealingCross);
            }
            platform.refuelableComp.ConsumeFuel(0.002f);//TODO no magic number
        }

    }
}
