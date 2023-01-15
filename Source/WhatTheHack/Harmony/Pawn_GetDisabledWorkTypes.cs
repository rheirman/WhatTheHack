using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(Pawn), "GetDisabledWorkTypes")]
internal class Pawn_GetDisabledWorkTypes
{
    private static bool Prefix(Pawn __instance, ref List<WorkTypeDef> __result)
    {
        if (!__instance.IsHacked())
        {
            return true;
        }

        var shouldForbid = new List<WorkTypeDef>();
        var store = Base.Instance.GetExtendedDataStorage();
        if (store == null)
        {
            return true;
        }

        var pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(__instance);
        foreach (var def in DefDatabase<WorkTypeDef>.AllDefs)
        {
            if (pawnData.workTypes == null || !pawnData.workTypes.Contains(def))
            {
                shouldForbid.Add(def);
            }
        }

        __result = shouldForbid;
        return false;
    }
}