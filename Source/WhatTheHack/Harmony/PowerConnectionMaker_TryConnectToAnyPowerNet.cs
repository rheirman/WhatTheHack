using HarmonyLib;
using RimWorld;
using WhatTheHack.Comps;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(PowerConnectionMaker), "TryConnectToAnyPowerNet")]
internal class PowerConnectionMaker_TryConnectToAnyPowerNet
{
    private static bool Prefix(CompPower pc)
    {
        return pc.parent.GetComp<CompMountable>() is not { Active: true };
    }
}