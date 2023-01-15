using HarmonyLib;
using RimWorld;

namespace WhatTheHack.Harmony;

//Make sure mechanoids are activated automatically when drafted. 
[HarmonyPatch(typeof(Pawn_DraftController), "set_Drafted")]
internal static class Pawn_Draftcontroller_set_Drafted
{
    private static void Postfix(Pawn_DraftController __instance)
    {
        var store = Base.Instance.GetExtendedDataStorage();
        if (store == null)
        {
            return;
        }

        var pawnData = store.GetExtendedDataFor(__instance.pawn);
        if (__instance.Drafted)
        {
            pawnData.isActive = true;
        }
    }
}