using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using WhatTheHack.Buildings;
using WhatTheHack.Needs;
using WhatTheHack.Storage;

namespace WhatTheHack.Harmony
{


    [HarmonyPatch(typeof(Pawn_HealthTracker), "MakeDowned")]
    static class Pawn_HealthTracker_MakeDowned
    {
        static void Postfix(Pawn_HealthTracker __instance)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            ExtendedPawnData pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(pawn);
            pawnData.isActive = false;
        }
    }
  


    [HarmonyPatch(typeof(Pawn_HealthTracker), "HealthTick")]
    static class Pawn_HealthTracker_HealthTick
    {
        static void Postfix(Pawn_HealthTracker __instance)
        {
            
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if(!pawn.RaceProps.IsMechanoid)
            {
                return;
            }
            if(!(pawn.CurrentBed() is Building_MechanoidPlatform))
            {
                return;
            }

            Building_MechanoidPlatform platform = (Building_MechanoidPlatform)pawn.CurrentBed();

            if (__instance.hediffSet.HasNaturallyHealingInjury() && pawn.OnMechanoidPlatform())
            {
                if (pawn.IsHashIntervalTick(10) && platform.CanHealNow())
                {
                    TryHealRandomInjury(__instance, pawn, platform);
                    if (platform.RegenerateActive)
                    {
                        TryRegeneratePart(__instance, pawn, platform);
                    }
                }
            }
            if (pawn.IsHashIntervalTick(1000))
            {
                Random rand = new Random(DateTime.Now.Millisecond);
                foreach (Hediff_MissingPart hediff in from x 
                                                      in pawn.health.hediffSet.GetHediffs<Hediff_MissingPart>()
                                                      select x)
                {
                    int randInt = rand.Next(0, 100);
                    Log.Message("randInt: " + randInt);
                    Log.Message("fixing lost parts, damaging part with: " + (int)hediff.Part.def.GetMaxHealth(pawn) + " damage");
                    pawn.health.RemoveHediff(hediff);
                    platform.refuelableComp.ConsumeFuel(5f);
                    //Hediff_Injury injury = new Hediff_Injury();
                    DamageWorker_AddInjury addInjury = new DamageWorker_AddInjury();
                    addInjury.Apply(new DamageInfo(WTH_DefOf.WTH_RegeneratedPartDamage, (int)hediff.Part.def.GetMaxHealth(pawn) - 1, -1, null, hediff.Part), pawn);
                    //pawn.health.AddHediff(WTH_DefOf.WTH_RegeneratedPart);

                }

            }


            if (platform.HasPowerNow())
            {
                Need powerNeed = pawn.needs.TryGetNeed(WTH_DefOf.Mechanoid_Power);
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

        private static void TryHealRandomInjury(Pawn_HealthTracker __instance, Pawn pawn, Building_MechanoidPlatform platform)
        {
            IEnumerable<Hediff_Injury> hediffs = __instance.hediffSet.GetHediffs<Hediff_Injury>().Where((Hediff_Injury i) => HediffUtility.CanHealNaturally(i) && i.def != WTH_DefOf.WTH_RegeneratedPart);
            if (hediffs.Count() == 0)
            {
                return;
            }
            Hediff_Injury hediff_Injury = hediffs.RandomElement();
            hediff_Injury.Heal(platform.def.building.bed_healPerDay / RimWorld.GenDate.TicksPerDay);
            if (pawn.IsHashIntervalTick(50) && !pawn.IsHashIntervalTick(100) && !pawn.Position.Fogged(pawn.Map))
            {
                MoteMaker.ThrowMetaIcon(pawn.Position, pawn.Map, ThingDefOf.Mote_HealingCross);
            }
            platform.refuelableComp.ConsumeFuel(0.002f);//TODO no magic number
        }
        private static void TryRegeneratePart(Pawn_HealthTracker __instance, Pawn pawn, Building_MechanoidPlatform platform)
        {
            IEnumerable<Hediff_Injury> hediffs = __instance.hediffSet.GetHediffs<Hediff_Injury>().Where((Hediff_Injury i) => HediffUtility.CanHealNaturally(i) && i.def == WTH_DefOf.WTH_RegeneratedPart);
            if(hediffs.Count() == 0)
            {
                return;
            }
            Hediff_Injury hediff_Injury = hediffs.RandomElement();
            hediff_Injury.Heal(platform.def.building.bed_healPerDay / RimWorld.GenDate.TicksPerDay);
            if (pawn.IsHashIntervalTick(70) && !pawn.IsHashIntervalTick(100) && !pawn.IsHashIntervalTick(50) && !pawn.Position.Fogged(pawn.Map))
            {
                MoteMaker.ThrowMetaIcon(pawn.Position, pawn.Map, WTH_DefOf.WTH_Mote_HealingCrossGreen);
            }
            platform.refuelableComp.ConsumeFuel(0.01f);//TODO no magic number
        }
    }
}
