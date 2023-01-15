using HarmonyLib;
using RimWorld;
using WhatTheHack.Comps;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(Building_TurretGun), "Tick")]
internal class Building_TurretGun_Tick
{
    private static void Postfix(Building_TurretGun __instance, TurretTop ___top)
    {
        if (__instance.GetComp<CompMountable>() is not { Active: true } comp)
        {
            return;
        }

        //var curRotation = Traverse.Create(___top).Property("CurRotation").GetValue<float>();
        if (__instance.Rotation == comp.mountedTo.Rotation)
        {
            return;
        }

        ___top.CurRotation = comp.mountedTo.Rotation.AsAngle;
        //Traverse.Create(___top).Property("CurRotation").SetValue(comp.mountedTo.Rotation.AsAngle);
        __instance.Rotation = comp.mountedTo.Rotation;
    }
}