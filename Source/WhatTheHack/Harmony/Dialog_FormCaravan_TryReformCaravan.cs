using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using WhatTheHack.Comps;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(Dialog_FormCaravan), "TryReformCaravan")]
internal class Dialog_FormCaravan_TryReformCaravan
{
    //Make sure mounted turrets are brought along with the caravan. 
    private static void Prefix(Dialog_FormCaravan __instance)
    {
        var mountedTurretTows = __instance.transferables.Where(tow =>
                tow.AnyThing is ThingWithComps twc &&
                twc.TryGetComp<CompMountable>() is { mountedTo: { } })
            .ToList();

        foreach (var tow in mountedTurretTows)
        {
            var comp = tow.AnyThing.TryGetComp<CompMountable>();
            comp.ToInventory();
            tow.CountToTransfer = 1;
            //Traverse.Create(tow).Property("CountToTransfer").SetValue(1);
        }
    }
}