using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.Harmony
{

    [HarmonyPatch(typeof(Thing), "ButcherProducts")]
    static class Thing_ButcherProducts
    {
        static void Postfix(Thing __instance, ref IEnumerable<Thing> __result, float efficiency)
        {
            if (__instance is Pawn && ((Pawn)__instance).RaceProps.IsMechanoid)
            {
                Pawn pawn = __instance as Pawn;
                __result = GenerateExtraButcherProducts(__result, pawn, efficiency);
            }
        }

        private static IEnumerable<Thing> GenerateExtraButcherProducts(IEnumerable<Thing> things, Pawn pawn, float efficiency)
        {
            foreach( Thing thing in things){
                yield return thing;
            }
            Random random = new Random(DateTime.Now.Millisecond);

            int partsCount = random.Next(GenMath.RoundRandom(pawn.kindDef.combatPower * 0.05f * efficiency), GenMath.RoundRandom(pawn.kindDef.combatPower * 0.1f * efficiency)); //TODO: no magic number
            if (partsCount > 0)
            {
                Thing parts = ThingMaker.MakeThing(WTH_DefOf.WTH_MechanoidParts, null);
                parts.stackCount = partsCount;
                yield return parts;
            }
            int chipCount = random.Next(0, GenMath.RoundRandom(pawn.kindDef.combatPower * 0.012f * efficiency));//TODO: no magic number
            if (chipCount > 0) 
            {
                Thing chips = ThingMaker.MakeThing(WTH_DefOf.WTH_MechanoidChip, null);
                chips.stackCount = chipCount;
                yield return chips;
            }
            if (pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_ReplacedAI))
            {
                Thing AICore = ThingMaker.MakeThing(ThingDefOf.AIPersonaCore);
                yield return AICore;
            }
        }
    }
}
