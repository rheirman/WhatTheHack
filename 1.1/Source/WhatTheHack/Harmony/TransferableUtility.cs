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
    [HarmonyPatch(typeof(TransferableUtility),"CanStack")]
    class TransferableUtility_CanStack
    {
        static bool Prefix(Thing thing, ref bool __result)
        {
            if((thing is Pawn && ((Pawn)thing).IsHacked()) || thing is Building_PortableChargingPlatform)
            {
                __result = false; 
                return false;
            }
            if(thing is ThingWithComps twc && twc.TryGetComp<CompMountable>() is CompMountable comp && comp.mountedTo != null)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
}
