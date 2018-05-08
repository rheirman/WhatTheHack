using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using WhatTheHack.Buildings;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(Pawn_HealthTracker), "HealthTick")]
    static class Pawn_HealthTracker_HealthTick
    {
        static void Postfix(Pawn_HealthTracker __instance)
        {
            
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (pawn.RaceProps.IsMechanoid && __instance.hediffSet.HasNaturallyHealingInjury() && pawn.OnMechanoidPlatform())

            {
                Building_MechanoidPlatform platform = (Building_MechanoidPlatform)pawn.CurrentBed();
                if (!platform.CanHealNow())
                {
                    Log.Message("cannot heal now");
                    return;
                }

                float num = 8f;
                if (pawn.GetPosture() != PawnPosture.Standing)
                {
                    num += 4f;
                    Building_Bed building_Bed = pawn.CurrentBed();
                    if (building_Bed != null)
                    {
                        num += building_Bed.def.building.bed_healPerDay;
                    }
                }
                Hediff_Injury hediff_Injury = __instance.hediffSet.GetHediffs<Hediff_Injury>().Where(new Func<Hediff_Injury, bool>(HediffUtility.CanHealNaturally)).RandomElement<Hediff_Injury>();
                hediff_Injury.Heal(num * pawn.HealthScale * 0.01f);
                if (pawn.IsHashIntervalTick(100) && !pawn.Position.Fogged(pawn.Map))
                {
                    MoteMaker.ThrowMetaIcon(pawn.Position, pawn.Map, ThingDefOf.Mote_HealingCross);
                }
            }
            
        }
    }
}
