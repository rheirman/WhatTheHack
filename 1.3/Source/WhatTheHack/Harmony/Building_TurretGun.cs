using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using WhatTheHack.Buildings;
using WhatTheHack.Comps;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(Building_TurretGun), "Tick")]
    class Building_TurretGun_Tick
    {
        static void Postfix(Building_TurretGun __instance, TurretTop ___top)
        {
            if (__instance.GetComp<CompMountable>() is CompMountable comp && comp.Active)
            {
                //TODO: look into performance of this. Try to avoid Traverse. 
                float curRotation = Traverse.Create(___top).Property("CurRotation").GetValue<float>();
                if (__instance.Rotation != comp.mountedTo.Rotation)
                {
                    Traverse.Create(___top).Property("CurRotation").SetValue(comp.mountedTo.Rotation.AsAngle);
                    __instance.Rotation = comp.mountedTo.Rotation;
                }
            }
        }
    }
    [HarmonyPatch(typeof(TurretTop), "TurretTopTick")]
    class TurretTop_TurretTopTick
    {
        static bool Prefix(TurretTop __instance, Building_Turret ___parentTurret)
        {
            if (___parentTurret.GetComp<CompMountable>() is CompMountable comp && comp.Active && !___parentTurret.CurrentTarget.IsValid)
            {
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(Building_TurretGun), "get_CanSetForcedTarget")]
    class Building_TurretGun_get_CanSetForcedTarget
    {
        static void Postfix(Building_TurretGun __instance, ref bool __result)
        {

            if (Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(__instance.Map).rogueAI is Building_RogueAI rogueAI)
            {
                if (rogueAI.controlledTurrets.Contains(__instance))
                {
                    __result = true;
                }
            }
        }
    }
    [HarmonyPatch(typeof(Building_TurretGun), "DrawExtraSelectionOverlays")]
    class Building_TurretGun_DrawExtraSelectionOverlays
    {            
        static void Postfix(Building_TurretGun __instance)
        {
            if (Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(__instance.Map).rogueAI is Building_RogueAI controller)
            {
                if (controller.controlledTurrets.Contains(__instance))
                {
                    GenDraw.DrawLineBetween(__instance.Position.ToVector3Shifted(), controller.Position.ToVector3Shifted(), SimpleColor.Green);
                }
            }
        }
    }
}
