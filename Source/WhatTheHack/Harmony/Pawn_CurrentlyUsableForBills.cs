using HarmonyLib;
using Verse;
using Verse.AI;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(Pawn), "CurrentlyUsableForBills")]
internal static class Pawn_CurrentlyUsableForBills
{
    private static void Postfix(Pawn __instance, ref bool __result)
    {
        var bill = __instance.health.surgeryBills.FirstShouldDoNow;
        if (!__instance.IsMechanoid())
        {
            return;
        }

        if (bill == null || !bill.recipe.HasModExtension<DefModExtension_Recipe>() ||
            !__instance.InteractionCell.IsValid)
        {
            return;
        }

        if (bill.recipe.GetModExtension<DefModExtension_Recipe>().requireBed == false ||
            __instance.OnHackingTable())
        {
            __result = true;
        }
        else
        {
            JobFailReason.Is("WTH_Reason_NotOnTable".Translate());
            __result = false;
        }
    }
}