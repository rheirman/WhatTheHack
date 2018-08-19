using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using WhatTheHack.Comps;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(Thing), "Print")]
    static class Building_DrawAt
    {
        static bool Prefix(Thing __instance)
        {
            if (__instance.TryGetComp<CompMountable>() is CompMountable comp && comp.Active)
            {
                return false;
            }
            return true;
        }
    }


    [HarmonyPatch(typeof(Thing), "get_DrawPos")]
    static class Thing_get_DrawPos
    {
        static bool Prefix(Thing __instance, ref Vector3 __result)
        {
            if(!__instance.Destroyed && __instance.TryGetComp<CompMountable>() is CompMountable comp && comp.Active)
            {
                Vector3 drawPos = comp.mountedTo.DrawPos;
                drawPos.z = comp.mountedTo.DrawPos.z + 0.7f;
                drawPos.y = comp.mountedTo.DrawPos.y + 1;
                __result = drawPos;
                return false;
            }
            return true;
        }
    }
    

    [HarmonyPatch(typeof(Thing), "DrawExtraSelectionOverlays")]
    static class Thing_DrawExtraSelectionOverlays
    {
        static void Postfix(Thing __instance)
        {
            if (__instance is Pawn)
            {
                Pawn pawn = (Pawn)__instance;
                if (pawn.RemoteControlLink() != null)
                {
                    if (pawn.IsHacked())
                    {
                        GenDraw.DrawRadiusRing(pawn.RemoteControlLink().Position, 30f);
                    }
                    else
                    {
                        GenDraw.DrawRadiusRing(pawn.Position, 30f);
                    }
                    GenDraw.DrawLineBetween(pawn.Position.ToVector3Shifted(), pawn.RemoteControlLink().Position.ToVector3Shifted());
                }
            }
        }
    }
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
            System.Random random = new System.Random(DateTime.Now.Millisecond);

            int partsCount = random.Next(GenMath.RoundRandom(pawn.kindDef.combatPower * 0.03f * efficiency), GenMath.RoundRandom(pawn.kindDef.combatPower * 0.06f * efficiency)); //TODO: no magic number
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
