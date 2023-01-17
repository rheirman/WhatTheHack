using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using WhatTheHack.Comps;

namespace WhatTheHack.Harmony
{
    //Don't show mounted turrets in caravan forming dialog and make sure they are brought along when leaving a map
    [HarmonyPatch(typeof(TransferableOneWayWidget),"DoRow")]
    class TransferableOneWayWidget_DoRow
    {
        static bool Prefix(Rect rect, TransferableOneWay trad, int index, float availableMass)
        {
            if (trad.AnyThing is ThingWithComps twc && twc.TryGetComp<CompMountable>() is CompMountable comp && comp.mountedTo != null)
            {
                if (comp.parent.Spawned)
                {
                    comp.ToInventory();
                }
                Traverse.Create(trad).Property("CountToTransfer").SetValue(1);
                return false;
            }
            return true;
        }
    }
}
