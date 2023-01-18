using HarmonyLib;
using RimWorld;
using Verse;
using WhatTheHack.Comps;

namespace WhatTheHack.Harmony;

//Don't show mounted turrets in caravan forming dialog and make sure they are brought along when leaving a map
[HarmonyPatch(typeof(TransferableOneWayWidget), "DoRow")]
internal class TransferableOneWayWidget_DoRow
{
    private static bool Prefix(TransferableOneWay trad)
    {
        if (trad.AnyThing is not ThingWithComps twc ||
            twc.TryGetComp<CompMountable>() is not { mountedTo: { } } comp)
        {
            return true;
        }

        if (comp.parent.Spawned)
        {
            comp.ToInventory();
        }

        trad.CountToTransfer = 1;
        //Traverse.Create(trad).Property("CountToTransfer").SetValue(1);
        return false;
    }
}