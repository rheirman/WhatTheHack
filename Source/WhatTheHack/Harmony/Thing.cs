using Harmony;
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
            Log.Message("callign butcher products postfix");

            if (__instance is Pawn && ((Pawn)__instance).RaceProps.IsMechanoid)
            {
                Pawn pawn = __instance as Pawn;
                int partsCount = GenMath.RoundRandom(pawn.kindDef.combatPower / 10 * efficiency);
                Log.Message("spawning " + partsCount + "mechanoid parts");
                __result = GenerateMechanoidParts(__result, partsCount);
            }
        }

        private static IEnumerable<Thing> GenerateMechanoidParts(IEnumerable<Thing> things, int partsCount)
        {
            foreach( Thing thing in things){
                yield return thing;
            }
            if (partsCount > 0)
            {
                Thing parts = ThingMaker.MakeThing(WTH_DefOf.MechanoidParts, null);
                parts.stackCount = partsCount;
                yield return parts;
            }
        }
    }
}
