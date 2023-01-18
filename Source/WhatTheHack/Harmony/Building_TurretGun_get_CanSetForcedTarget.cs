using HarmonyLib;
using RimWorld;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(Building_TurretGun), "get_CanSetForcedTarget")]
internal class Building_TurretGun_get_CanSetForcedTarget
{
    private static void Postfix(Building_TurretGun __instance, ref bool __result)
    {
        if (Base.Instance?.GetExtendedDataStorage()?.GetExtendedDataFor(__instance?.Map)?.rogueAI is not { } rogueAI)
        {
            return;
        }

        if (rogueAI.controlledTurrets?.Contains(__instance) == true)
        {
            __result = true;
        }
    }
}