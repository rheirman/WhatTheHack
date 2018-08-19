using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using WhatTheHack.Comps;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(Building_TurretGun), "Tick")]
    class Building_TurretGun_Tick 
    {
        static void Postfix(Building_TurretGun __instance)
        {
            if (__instance.GetComp<CompMountable>() is CompMountable comp && comp.Active)
            {
                TurretTop top = Traverse.Create(__instance).Field("top").GetValue<TurretTop>();
                float curRotation = Traverse.Create(top).Property("CurRotation").GetValue<float>();
                if(__instance.Rotation != comp.mountedTo.Rotation)
                {
                    Traverse.Create(top).Property("CurRotation").SetValue(comp.mountedTo.Rotation.AsAngle);
                    __instance.Rotation = comp.mountedTo.Rotation;
                }
            }
        }
    }
    [HarmonyPatch(typeof(TurretTop), "TurretTopTick")]
    class TurretTop_TurretTopTick
    {
        static bool Prefix(TurretTop __instance)
        {
            Building_Turret parentTurret = Traverse.Create(__instance).Field("parentTurret").GetValue<Building_Turret>();
            if (parentTurret.GetComp<CompMountable>() is CompMountable comp && comp.Active && !parentTurret.CurrentTarget.IsValid)
            {
                return false;
            }
            return true;
        }
    }
}
