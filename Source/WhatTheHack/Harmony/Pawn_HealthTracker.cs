using Harmony;
using RimWorld;
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
            ExtendedPawnData pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(pawn);
            pawnData.isActive = false;
            pawn.RemoveRemoteControlLink();

        }
    }
    //Recharge and repair mechanoid when on platform
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
            if (!(pawn.CurrentBed() is Building_MechanoidPlatform))
            {
                return;
            }

            Building_MechanoidPlatform platform = (Building_MechanoidPlatform)pawn.CurrentBed();

            if (__instance.hediffSet.HasNaturallyHealingInjury() && pawn.OnMechanoidPlatform())
            {
                if (pawn.IsHashIntervalTick(10) && platform.CanHealNow())
                {
                    TryHealRandomInjury(__instance, pawn, platform);
                    if (platform.RegenerateActive && pawn.IsHashIntervalTick(1000))
                    {
                        TryRegeneratePart(pawn, platform);
                    }

                }
            }

            if (platform.HasPowerNow())
            {
                Need powerNeed = pawn.needs.TryGetNeed(WTH_DefOf.WTH_Mechanoid_Power);
                float powerPerTick = 0.75f * platform.PowerComp.Props.basePowerConsumption / GenDate.TicksPerDay; //TODO: no magic number
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
        }

        private static void TryRegeneratePart(Pawn pawn, Building_MechanoidPlatform platform)
        {
            Random rand = new Random(DateTime.Now.Millisecond);
            foreach (Hediff_MissingPart hediff in from x
                                                    in pawn.health.hediffSet.GetHediffs<Hediff_MissingPart>()
                                                    select x)
            {
                int randInt = rand.Next(0, 100);

                if(randInt <= 2)
                {
                    pawn.health.RemoveHediff(hediff);
                    platform.refuelableComp.ConsumeFuel(5f);
                    //Hediff_Injury injury = new Hediff_Injury();
                    DamageWorker_AddInjury addInjury = new DamageWorker_AddInjury();
                    addInjury.Apply(new DamageInfo(WTH_DefOf.WTH_RegeneratedPartDamage, hediff.Part.def.GetMaxHealth(pawn) - 1), pawn);
                    MoteMaker.ThrowMetaIcon(pawn.Position, pawn.Map, WTH_DefOf.WTH_Mote_HealingCrossGreen);
                }

                //pawn.health.AddHediff(WTH_DefOf.WTH_RegeneratedPart);

            }
        }

        private static void TryHealRandomInjury(Pawn_HealthTracker __instance, Pawn pawn, Building_MechanoidPlatform platform)
        {
            IEnumerable<Hediff_Injury> hediffs = __instance.hediffSet.GetHediffs<Hediff_Injury>().Where((Hediff_Injury i) => HediffUtility.CanHealNaturally(i) && i.def != WTH_DefOf.WTH_RegeneratedPart);
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
