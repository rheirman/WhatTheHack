using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using WhatTheHack.Comps;

namespace WhatTheHack.Harmony
{
    //TODO: transpile this for better performance. 
    [HarmonyPatch(typeof(GenGrid), "Standable")]
    class GenGrid_Standable
    {
        static void Postfix(ref bool __result, IntVec3 c, Map map)
        {
            if (!__result)
            {
                if (!map.pathGrid.Walkable(c))
                {
                    __result = false;
                    return;
                }
                List<Thing> list = map.thingGrid.ThingsListAt(c);
                foreach (Thing t in list)
                {
                    if (t is ThingWithComps twc && twc.GetComp<CompMountable>() is CompMountable comp && comp.Active)
                    {
                        __result = true;
                    }
                }
            }
        }
    }
}
