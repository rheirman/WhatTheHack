using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using WhatTheHack.Comps;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(EdificeGrid), "Register")]
    class EdificeGrid_Register
    {
        static bool Prefix(Building ed)
        {
            if(ed.TryGetComp<CompMountable>() is CompMountable comp && comp.Active)
            {
                return false;
            }
            return true;
        }
    }
}
