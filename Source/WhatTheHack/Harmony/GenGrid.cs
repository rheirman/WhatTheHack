using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using WhatTheHack.Comps;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(GenGrid), "Standable")]
    class GenGrid_Standable
    {
        static void Postfix(ref bool __result, IntVec3 c, Map map)
        {
            if (!__result)
            {
                List<Thing> list = map.thingGrid.ThingsListAt(c);
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] is ThingWithComps twc && twc.GetComp<CompMountable>() is CompMountable comp && comp.Active)
                    {
                        __result = true;
                    }
                }
            }
        }
    }
}
