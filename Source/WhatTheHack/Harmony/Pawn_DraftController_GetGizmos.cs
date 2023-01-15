using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(Pawn_DraftController), "GetGizmos")]
internal static class Pawn_DraftController_GetGizmos
{
    private static void Postfix(Pawn_DraftController __instance, ref IEnumerable<Gizmo> __result)
    {
        __result = PatchGetGizmos(__instance, __result);
    }

    private static IEnumerable<Gizmo> PatchGetGizmos(Pawn_DraftController __instance, IEnumerable<Gizmo> __result)
    {
        foreach (var gizmo in __result)
        {
            if (gizmo is Command_Toggle toggleCommand)
            {
                if (toggleCommand.defaultDesc == "CommandToggleDraftDesc".Translate())
                {
                    DisableCommandIfMechanoidPowerLow(__instance, toggleCommand);
                    DisableCommandIfNotActivated(__instance, toggleCommand);
                    yield return toggleCommand;
                    continue;
                }
            }

            yield return gizmo;
        }
    }

    private static void DisableCommandIfMechanoidPowerLow(Pawn_DraftController __instance, Command_Toggle toggleCommand)
    {
        var store = Base.Instance.GetExtendedDataStorage();
        if (store == null)
        {
            return;
        }

        if (__instance.pawn.ShouldRecharge())
        {
            toggleCommand.Disable("WTH_Reason_PowerLow".Translate());
        }

        if (__instance.pawn.ShouldBeMaintained())
        {
            toggleCommand.Disable("WTH_Reason_MaintenanceLow".Translate());
        }
    }

    private static void DisableCommandIfNotActivated(Pawn_DraftController __instance, Command_Toggle toggleCommand)
    {
        if (toggleCommand.isActive() && !__instance.pawn.IsActivated())
        {
            __instance.Drafted = false;
        }
    }
}