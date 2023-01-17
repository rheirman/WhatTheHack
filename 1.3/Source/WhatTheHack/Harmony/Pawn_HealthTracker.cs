using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using WhatTheHack.Buildings;
using WhatTheHack.Needs;
using WhatTheHack.Storage;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(Pawn_HealthTracker), "SetDead")]
    static class Pawn_HealthTracker_SetDead
    {
        static void Postfix(Pawn_HealthTracker __instance)
        {
            List<Hediff> removedHediffs = __instance.hediffSet.hediffs.FindAll((Hediff h) => h.def.GetModExtension<DefModextension_Hediff>() is DefModextension_Hediff modExt && Rand.Chance(modExt.destroyOnDeathChance));
            foreach (Hediff hediff in removedHediffs)
            {
                __instance.AddHediff(WTH_DefOf.WTH_DestroyedModule, hediff.Part);
            }
            __instance.hediffSet.hediffs = __instance.hediffSet.hediffs.Except(removedHediffs).ToList();
        }
    }

    [HarmonyPatch(typeof(Pawn_HealthTracker), "HasHediffsNeedingTend")]
    static class Pawn_HealthTracker_HasHediffsNeedingTend
    {
        static bool Prefix(Pawn_HealthTracker __instance, ref Pawn ___pawn)
        {
            if (___pawn.IsMechanoid() && ___pawn.IsHacked())
            {
                return false;
            }
            return true;
        }
    }

    //Make sure mechanoids can be downed like other pawns. 
    [HarmonyPatch(typeof(Pawn_HealthTracker), "CheckForStateChange")]
    [HarmonyPriority(Priority.High)]
    public static class Pawn_HealthTracker_CheckForStateChange
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            bool flag = false;
            var instructionsList = new List<CodeInstruction>(instructions);
            for (var i = 0; i < instructionsList.Count; i++)
            {
                CodeInstruction instruction = instructionsList[i];

                if (instruction.operand as MethodInfo == typeof(RaceProperties).GetMethod("get_IsMechanoid"))
                {
                    flag = true;
                }
                if(flag && instruction.opcode == OpCodes.Ldc_R4)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn_HealthTracker), "pawn"));
                    yield return new CodeInstruction(OpCodes.Call, typeof(Pawn_HealthTracker_CheckForStateChange).GetMethod("GetMechanoidDownChance"));
                    //yield return new CodeInstruction(OpCodes.Ldc_R4, 0.5f);//TODO: no magic number? 
                    flag = false;
                }
                else
                {
                    yield return instruction;
                }
            }
        }
        public static float GetMechanoidDownChance(Pawn pawn)
        {
            
            if(pawn.Faction != Faction.OfPlayer && !pawn.Faction.HostileTo(Faction.OfPlayer))//make sure allied mechs always die to prevent issues with relation penalties when the player hacks their mechs. 
            {
               return 1.0f;
            }
            else
            {
                return Base.deathOnDownedChance / 100f;
            }
        }

    }
    [HarmonyPatch(typeof(Pawn_HealthTracker), "ShouldBeDeadFromLethalDamageThreshold")]
    static class Pawn_HealthTracker_ShouldBeDeadFromLethalDamageThreshold
    {
        static void Postfix(Pawn_HealthTracker __instance, ref bool __result)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (!pawn.IsMechanoid())
            {
                return;
            }
            if(pawn.Faction != Faction.OfPlayer && !pawn.Faction.HostileTo(Faction.OfPlayer))//make sure allied mechs always die to prevent issues with relation penalties when the player hacks their mechs. 
            {
                return;
            }

            if(__result == true)
            {
                if (__instance.hediffSet.HasHediff(WTH_DefOf.WTH_HeavilyDamaged))
                {
                    __result = false;
                    return;
                }
                if (Rand.Chance(Base.downedOnDeathThresholdChance.Value/100f))//Chance mech goes down instead of dying when lethal threshold is achieved. 
                {
                    __instance.AddHediff(WTH_DefOf.WTH_HeavilyDamaged);
                    if (pawn.mindState == null)
                    {
                        pawn.mindState = new Pawn_MindState(pawn);
                    }
                    __result = false;
                }
            }
            else
            {
                if (__instance.hediffSet.HasHediff(WTH_DefOf.WTH_HeavilyDamaged))
                {
                    __instance.RemoveHediff(__instance.hediffSet.GetFirstHediffOfDef(WTH_DefOf.WTH_HeavilyDamaged));
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
            pawn.RemoveAllLinks();
        }
    }
    //Recharge and repair mechanoid when on platform
    //TODO: refactor. Move all needs related stuff to something needs related
    [HarmonyPatch(typeof(Pawn_HealthTracker), "HealthTick")]
    static class Pawn_HealthTracker_HealthTick
    {
        private const int healTickInterval = 200;

        static void Postfix(Pawn_HealthTracker __instance)
        {

            Pawn pawn = __instance.hediffSet.pawn;
            if (!pawn.IsMechanoid())
            {
                return;
            }
            if (pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_SelfDestructed))
            {
                SelfDestruct(pawn);
                return;
            }

            if (pawn.IsHashIntervalTick(healTickInterval))
            {
                var repairHediff = pawn.health.hediffSet.GetFirstHediffOfDef(WTH_DefOf.WTH_Repairing);
                if (repairHediff != null && repairHediff.def.GetModExtension<DefModextension_Hediff>() is DefModextension_Hediff modExt)
                {   
                    TryHealRandomInjury(__instance, pawn, (modExt.repairRate / RimWorld.GenDate.TicksPerDay) * healTickInterval);
                }

                if (!(pawn.CurrentBed() is Building_BaseMechanoidPlatform))
                {
                    return;
                }
                Building_BaseMechanoidPlatform platform = (Building_BaseMechanoidPlatform)pawn.CurrentBed();

                if (platform.RepairActive && __instance.hediffSet.HasNaturallyHealingInjury() && !pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_Repairing) && platform.CanHealNow())
                {

                    TryHealRandomInjury(__instance, pawn, (platform.GetStatValue(WTH_DefOf.WTH_RepairRate) * healTickInterval) / RimWorld.GenDate.TicksPerDay, platform);
                }
                if (!__instance.hediffSet.HasNaturallyHealingInjury() && platform.RegenerateActive && platform.refuelableComp.Fuel >= 4f) //TODO: no magic number
                {
                    TryRegeneratePart(pawn, platform);
                    RegainWeapon(pawn);
                }
            }
           
            
        }

        private static void SelfDestruct(Pawn pawn)
        {
            GenExplosion.DoExplosion(pawn.Position, pawn.Map, 4.5f, DamageDefOf.Bomb, pawn, DamageDefOf.Bomb.defaultDamage, DamageDefOf.Bomb.defaultArmorPenetration, DamageDefOf.Bomb.soundExplosion, null, null, null, null, 0f, 1, false, null, 0f, 1, 0f, false);
            pawn.jobs.startingNewJob = false;
            BodyPartRecord reactorPart = pawn.health.hediffSet.GetNotMissingParts().FirstOrDefault((BodyPartRecord r) => r.def.defName == "Reactor");
            int guard = 0;
            while (!pawn.Dead && guard < 10)
            {
                pawn.TakeDamage(new DamageInfo(DamageDefOf.Bomb, reactorPart.def.GetMaxHealth(pawn), 9999, -1, null, reactorPart));
                guard++;
            }
            if (!pawn.Dead)
            {
                Log.Warning("Pawn " + pawn.Name + " should have died from self destruct but didn't. This should never happen, so please report this to the author of What the Hack!?");
            }
        }

        private static void RegainWeapon(Pawn pawn)
        {
            if(pawn.equipment.Primary == null)
            {
                PawnWeaponGenerator.TryGenerateWeaponFor(pawn, new PawnGenerationRequest(pawn.kindDef));
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
            float partHealth = hediff.Part.def.GetMaxHealth(pawn);
            float fuelNeeded = Math.Min(4f, partHealth / 5f);//body parts with less health need less parts to regenerate, capped at 4. 

            platform.refuelableComp.ConsumeFuel(fuelNeeded);
            //Hediff_Injury injury = new Hediff_Injury();
            DamageWorker_AddInjury addInjury = new DamageWorker_AddInjury();
            addInjury.Apply(new DamageInfo(WTH_DefOf.WTH_RegeneratedPartDamage, hediff.Part.def.GetMaxHealth(pawn) - 1, 0, -1, pawn, hediff.Part), pawn);

         
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

        private static void TryHealRandomInjury(Pawn_HealthTracker __instance, Pawn pawn, float healAmount, Building_BaseMechanoidPlatform platform = null)
        {
            IEnumerable<Hediff_Injury> hediffs = __instance.hediffSet.GetHediffs<Hediff_Injury>().Where((Hediff_Injury i) => HediffUtility.CanHealNaturally(i));
            if (hediffs.Count() == 0)
            {
                return;
            }
            Hediff_Injury hediff_Injury = hediffs.RandomElement();
            hediff_Injury.Heal(healAmount);
            if (pawn.Map != null && !pawn.Position.Fogged(pawn.Map))
            {
                FleckMaker.ThrowMetaIcon(pawn.Position, pawn.Map, WTH_DefOf.WTH_Fleck_HealingCrossGreen);
            }
            if(platform != null)
            {
                platform.refuelableComp.ConsumeFuel((platform.GetStatValue(WTH_DefOf.WTH_PartConsumptionRate) * healTickInterval) /GenDate.TicksPerDay);//TODO no magic number
            }
        }

    }
}
