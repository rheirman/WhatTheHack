using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(ArmorUtility), "ApplyArmor")]
    class ArmorUtility_ApplyArmor
    {
        static void Prefix(ref float armorRating, Pawn pawn)
        {
            if (pawn.RaceProps.IsMechanoid)
            {
                foreach(Hediff h in pawn.health.hediffSet.hediffs)
                {
                    if(h.def.GetModExtension<DefModextension_Hediff>() is DefModextension_Hediff modExt && modExt.armorFactor > 0f){
                        armorRating *= h.def.GetModExtension<DefModextension_Hediff>().armorFactor;
                    }
                }           
            }
        }
    }
}