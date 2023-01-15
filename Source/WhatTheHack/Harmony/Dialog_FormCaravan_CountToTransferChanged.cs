using HarmonyLib;
using RimWorld;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(Dialog_FormCaravan), "Notify_TransferablesChanged")]
internal class DialogFormCaravanNotifyTransferablesChanged
{
    private static void Postfix(Dialog_FormCaravan __instance)
    {
        Utilities.CalcDaysOfFuel(__instance.transferables);
    }
}