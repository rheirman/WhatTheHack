using HarmonyLib;
using RimWorld;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(Dialog_FormCaravan), "TryFormAndSendCaravan")]
internal class Dialog_FormCaravan_TryFormAndSendCaravan
{
    private static void Postfix(Dialog_FormCaravan __instance)
    {
        Utilities.LinkPortablePlatforms(__instance.transferables);
    }
}