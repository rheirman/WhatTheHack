using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using WhatTheHack.Comps;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(WorkGiver_DoBill), "TryFindBestBillIngredients")]
    class WorkGiver_DoBill_TryFindBestBillIngredients
    {
        static bool Prefix(WorkGiver_DoBill __instance, Bill bill, Pawn pawn, ref List<ThingCount> chosen, ref bool __result)
        {
            if(bill.recipe == WTH_DefOf.WTH_Craft_VanometricModule)
            {
                Thing thing = pawn.Map.spawnedThings.FirstOrDefault(
                    (Thing t) => (t.GetInnerIfMinified().def == ThingDefOf.VanometricPowerCell) &&
                    bill.IsFixedOrAllowedIngredient(t) &&
                    pawn.CanReach(t, Verse.AI.PathEndMode.Touch, Danger.Deadly) &&
                    !t.IsForbidden(pawn) &&
                    t is MinifiedThing);
                if (thing != null)
                {
                    ThingCountUtility.AddToList(chosen, thing, 1);
                    __result = true;
                    return false;
                }
            } 


            if (bill.recipe.defName.Contains("WTH_Mount"))
            {
                Predicate<Thing> isMounted = (Thing t) => t.TryGetComp<CompMountable>() is CompMountable comp && comp.Active;
                Thing thing = pawn.Map.spawnedThings.FirstOrDefault(
                    (Thing t) => (t.GetInnerIfMinified() is Building_TurretGun) &&
                    bill.IsFixedOrAllowedIngredient(t) &&
                    pawn.CanReach(t, Verse.AI.PathEndMode.Touch, Danger.Deadly) &&
                    !t.IsForbidden(pawn) &&
                    !isMounted(t) &&
                    t is MinifiedThing);
                if(thing != null)
                {
                    ThingCountUtility.AddToList(chosen, thing, 1);
                    __result = true;
                    return false;
                }

            }
            return true;
        }
    }

}
