using HarmonyLib;
using RimWorld;
using WhatTheHack.Comps;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(TurretTop), "TurretTopTick")]
internal class TurretTop_TurretTopTick
{
    private static bool Prefix(Building_Turret ___parentTurret)
    {
        return ___parentTurret.GetComp<CompMountable>() is not { Active: true } ||
               ___parentTurret.CurrentTarget.IsValid;
    }
}