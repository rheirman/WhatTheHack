using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WhatTheHack.Comps;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(PowerConnectionMaker), "TryConnectToAnyPowerNet")]
    class PowerConnectionMaker_TryConnectToAnyPowerNet
    {
        static bool Prefix(CompPower pc)
        {
            if(pc.parent.GetComp<CompMountable>() is CompMountable comp && comp.Active)
            {
                return false; 
            }
            return true;
        }
    }
}
