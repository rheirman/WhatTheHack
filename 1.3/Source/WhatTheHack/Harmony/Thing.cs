using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using WhatTheHack.Buildings;
using WhatTheHack.Comps;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(Thing), "Print")]
    static class Thing_Print
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
                drawPos.z = comp.mountedTo.DrawPos.z + comp.drawOffset;
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
                        GenDraw.DrawRadiusRing(pawn.RemoteControlLink().Position, Utilities.GetRemoteControlRadius(pawn.RemoteControlLink()));
                    }
                    else
                    {
                        GenDraw.DrawRadiusRing(pawn.Position, Utilities.GetRemoteControlRadius(pawn));
                    }
                    GenDraw.DrawLineBetween(pawn.Position.ToVector3Shifted(), pawn.RemoteControlLink().Position.ToVector3Shifted());
                }
                if(pawn.ControllingAI() is Building_RogueAI controller)
                {
                    if (controller.controlledMechs.Contains(pawn))
                    {
                        GenDraw.DrawLineBetween(pawn.Position.ToVector3Shifted(), pawn.ControllingAI().Position.ToVector3Shifted(), SimpleColor.Blue);
                    }
                    else
                    {
                        GenDraw.DrawLineBetween(pawn.Position.ToVector3Shifted(), pawn.ControllingAI().Position.ToVector3Shifted(), SimpleColor.Red);
                    }
                }
            }
            
            if (__instance is Building_RogueAI rogueAI)
            {
                foreach (Pawn pawn in rogueAI.controlledMechs)
                {
                    GenDraw.DrawLineBetween(rogueAI.Position.ToVector3Shifted(), pawn.Position.ToVector3Shifted(), SimpleColor.Blue);
                }
                foreach (Pawn pawn in rogueAI.hackedMechs)
                {
                    GenDraw.DrawLineBetween(rogueAI.Position.ToVector3Shifted(), pawn.Position.ToVector3Shifted(), SimpleColor.Red);
                }
                foreach (Building_TurretGun t in rogueAI.controlledTurrets)
                {
                    GenDraw.DrawLineBetween(rogueAI.Position.ToVector3Shifted(), t.Position.ToVector3Shifted(), SimpleColor.Green);
                }
            }


        }
    }
    [HarmonyPatch(typeof(Thing), "ButcherProducts")]
    static class Thing_ButcherProducts
    {
        static void Postfix(Thing __instance, ref IEnumerable<Thing> __result, float efficiency)
        {
            if (__instance is Pawn && ((Pawn)__instance).IsMechanoid())
            {
                Pawn pawn = __instance as Pawn;
                __result = GenerateExtraButcherProducts(__result, pawn, efficiency);
            }
        }

        private static IEnumerable<Thing> GenerateExtraButcherProducts(IEnumerable<Thing> things, Pawn pawn, float efficiency)
        {
            foreach (Thing thing in things)
            {
                yield return thing;
            }
            System.Random random = new System.Random(DateTime.Now.Millisecond);

            float combatpowerCapped = pawn.kindDef.combatPower <= 10000 ? pawn.kindDef.combatPower : 300;
            float baseSpawnRateParts = combatpowerCapped * GetDifficultyFactor() * Base.partDropRateModifier;
            float baseSpawnRateChips = combatpowerCapped * GetDifficultyFactor() * Base.chipDropRateModifier;

            int partsCount = random.Next(GenMath.RoundRandom(baseSpawnRateParts * 0.04f * efficiency), GenMath.RoundRandom(baseSpawnRateParts * 0.065f * efficiency)); //TODO: no magic number
            if (partsCount > 0)
            {
                Thing parts = ThingMaker.MakeThing(WTH_DefOf.WTH_MechanoidParts, null);
                parts.stackCount = partsCount;
                yield return parts;
            }
            int chipCount = random.Next(0, GenMath.RoundRandom(baseSpawnRateChips * 0.012f * efficiency));//TODO: no magic number
            if (chipCount > 0)
            {
                Thing chips = ThingMaker.MakeThing(WTH_DefOf.WTH_MechanoidChip, null);
                chips.stackCount = chipCount;
                yield return chips;
            }
            foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
            {
                if (hediff.def.GetModExtension<DefModextension_Hediff>() is DefModextension_Hediff ext)
                {
                    if (ext.extraButcherProduct != null)
                    {
                        yield return ThingMaker.MakeThing(ext.extraButcherProduct);
                    }
                }
            }
        }

        static private float GetDifficultyFactor()
        {
            float difficultyFactor = 1.0f;
            if (Find.Storyteller.difficultyDef == WTH_DefOf.Peaceful)
            {
                difficultyFactor = 2.0f;
            }
            else if (Find.Storyteller.difficultyDef == WTH_DefOf.Easy)
            {
                difficultyFactor = 1.5f;
            }
            else if (Find.Storyteller.difficultyDef == WTH_DefOf.Medium)
            {
                difficultyFactor = 1.35f;
            }
            else if (Find.Storyteller.difficultyDef == WTH_DefOf.Rough)
            {
                difficultyFactor = 1.2f;
            }
            else if (Find.Storyteller.difficultyDef == WTH_DefOf.Hard)
            {
                difficultyFactor = 1.1f;
            }
            return difficultyFactor;
        }
    }
}
