using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using WhatTheHack.Buildings;
using WhatTheHack.Needs;

namespace WhatTheHack.Harmony
{

    /*
    [HarmonyPatch(typeof(Pawn_HealthTracker), "MakeDowned")]
    static class Pawn_HealthTracker_MakeDowned
    {
        static void Prefix(Pawn_HealthTracker __instance)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if(pawn.RaceProps.IsMechanoid && pawn.equipment.Primary != null)
            {
                Log.Message("adding " + pawn.equipment.Primary.def + " to inventory of " + pawn.Name);
                bool success = pawn.equipment.Primary.holdingOwner.TryTransferToContainer(pawn.equipment.Primary, pawn.inventory.GetDirectlyHeldThings());
                if (!success)
                {
                    Log.Message("couldn't add weapon to inventory");
                }
            }
        }
        static void Postfix(Pawn_HealthTracker __instance)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (pawn.RaceProps.IsMechanoid)
            {
                pawn.inventory.UnloadEverything = false;
            }
        }
    }
    */


    [HarmonyPatch(typeof(Pawn_HealthTracker), "HealthTick")]
    static class Pawn_HealthTracker_HealthTick
    {
        static void Postfix(Pawn_HealthTracker __instance)
        {
            
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();

            if(!pawn.RaceProps.IsMechanoid  || !(pawn.CurrentBed() is Building_MechanoidPlatform))
            {
                Log.Message("pawn not in bed :( mie mie ");
                return;
            }

            Building_MechanoidPlatform platform = (Building_MechanoidPlatform)pawn.CurrentBed();

            if (__instance.hediffSet.HasNaturallyHealingInjury() && pawn.OnMechanoidPlatform())
            {
                if (platform.CanHealNow())
                {
                        Hediff_Injury hediff_Injury = __instance.hediffSet.GetHediffs<Hediff_Injury>().Where(new Func<Hediff_Injury, bool>(HediffUtility.CanHealNaturally)).RandomElement<Hediff_Injury>();
                        hediff_Injury.Heal(platform.def.building.bed_healPerDay / RimWorld.GenDate.TicksPerDay);
                        if (pawn.IsHashIntervalTick(100) && !pawn.Position.Fogged(pawn.Map))
                        {
                            MoteMaker.ThrowMetaIcon(pawn.Position, pawn.Map, ThingDefOf.Mote_HealingCross);
                        }
                }           
            }

            if (platform.HasPowerNow())
            {
                Need powerNeed = pawn.needs.TryGetNeed(WTH_DefOf.Mechanoid_Power);
                float powerPerTick = platform.PowerComp.Props.basePowerConsumption / GenDate.TicksPerDay;
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
                int i = 512;

                if (pawn.IsHashIntervalTick(i))
                {
                    MoteMaker.ThrowMetaIcon(pawn.Position, pawn.Map, ThingDefOf.Mote_PowerBeam);
                }

                else if (pawn.IsHashIntervalTick(i / 2))
                {
                    MoteMaker.ThrowMetaIcon(pawn.Position, pawn.Map, ThingDefOf.Mote_ShotHit_Spark);
                }
                else if (pawn.IsHashIntervalTick(i / 4))
                {
                    MoteMaker.ThrowMetaIcon(pawn.Position, pawn.Map, ThingDefOf.Mote_FireGlow);
                }
                else if (pawn.IsHashIntervalTick(i / 8))
                {
                    MoteMaker.ThrowMetaIcon(pawn.Position, pawn.Map, ThingDefOf.Mote_MetaPuff);
                }
                else if (pawn.IsHashIntervalTick(i / 16))
                {
                    MoteMaker.ThrowMetaIcon(pawn.Position, pawn.Map, ThingDefOf.Mote_LightningGlow);
                }
            }

        }
    }
}
