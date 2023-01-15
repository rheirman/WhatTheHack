using HarmonyLib;
using RimWorld;
using Verse;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(Building_TurretGun), "DrawExtraSelectionOverlays")]
internal class Building_TurretGun_DrawExtraSelectionOverlays
{
    private static void Postfix(Building_TurretGun __instance)
    {
        if (Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(__instance.Map).rogueAI is not { } controller)
        {
            return;
        }

        if (controller.controlledTurrets.Contains(__instance))
        {
            GenDraw.DrawLineBetween(__instance.Position.ToVector3Shifted(), controller.Position.ToVector3Shifted(),
                SimpleColor.Green);
        }
    }
}