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
                    Hediff_Injury hediff_Injury = __instance.hediffSet.GetHediffs<Hediff_Injury>().Where(new Func<Hediff_Injury, bool>(HediffUtility.CanHealNaturally)).RandomElement<Hediff_Injury>();
                    hediff_Injury.Heal(10 * platform.def.building.bed_healPerDay / RimWorld.GenDate.TicksPerDay);
                    if (pawn.IsHashIntervalTick(100) && !pawn.Position.Fogged(pawn.Map))
                    {
                        MoteMaker.ThrowMetaIcon(pawn.Position, pawn.Map, ThingDefOf.Mote_HealingCross);
                    }
                    platform.refuelableComp.ConsumeFuel(0.004f);//TODO no magic number

                }           
            }

            if (platform.HasPowerNow())
            {
                Need powerNeed = pawn.needs.TryGetNeed(WTH_DefOf.Mechanoid_Power);
                float powerPerTick = 0.75f * platform.PowerComp.Props.basePowerConsumption / GenDate.TicksPerDay; //TODO no magic number
                if (powerNeed.CurLevel + powerPerTick < powerNeed.MaxLevel)
                {
                    powerNeed.CurLevel += powerPerTick;
                }
                else if (powerNeed.CurLevel < powerNeed.MaxLevel)
                {
                    powerNeed.CurLevel = powerNeed.MaxLevel;
                }
                if (pawn.IsHashIntervalTick(100) && !pawn.Position.Fogged(pawn.Map))
                {
                    //MoteMaker.ThrowMetaIcon(pawn.Position, pawn.Map, ThingDefOf.Mote_PowerBeam);
                }
                int i = 100;

                if (pawn.IsHashIntervalTick(i))
                {
                    MoteMaker.ThrowMetaIcon(pawn.Position, pawn.Map, ThingDefOf.Mote_LightningGlow);
                }
            }

        }
    }
}
