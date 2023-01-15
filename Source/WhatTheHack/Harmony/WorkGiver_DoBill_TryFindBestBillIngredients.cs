using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using WhatTheHack.Comps;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(WorkGiver_DoBill), "TryFindBestBillIngredients")]
internal class WorkGiver_DoBill_TryFindBestBillIngredients
{
    private static bool Prefix(Bill bill, Pawn pawn, ref List<ThingCount> chosen, ref bool __result)
    {
        Thing thing;
        if (bill.recipe == WTH_DefOf.WTH_Craft_VanometricModule)
        {
            thing = pawn.Map.spawnedThings.FirstOrDefault(
                t => t.GetInnerIfMinified().def == ThingDefOf.VanometricPowerCell &&
                     bill.IsFixedOrAllowedIngredient(t) &&
                     pawn.CanReach(t, PathEndMode.Touch, Danger.Deadly) &&
                     !t.IsForbidden(pawn) &&
                     t is MinifiedThing);
            if (thing != null)
            {
                ThingCountUtility.AddToList(chosen, thing, 1);
                __result = true;
                return false;
            }
        }


        if (!bill.recipe.defName.Contains("WTH_Mount"))
        {
            return true;
        }

        bool IsMounted(Thing t)
        {
            return t.TryGetComp<CompMountable>() is { Active: true };
        }

        thing = pawn.Map.spawnedThings.FirstOrDefault(
            t => t.GetInnerIfMinified() is Building_TurretGun &&
                 bill.IsFixedOrAllowedIngredient(t) &&
                 pawn.CanReach(t, PathEndMode.Touch, Danger.Deadly) &&
                 !t.IsForbidden(pawn) &&
                 !IsMounted(t) &&
                 t is MinifiedThing);
        if (thing == null)
        {
            return true;
        }

        ThingCountUtility.AddToList(chosen, thing, 1);
        __result = true;
        return false;
    }
}