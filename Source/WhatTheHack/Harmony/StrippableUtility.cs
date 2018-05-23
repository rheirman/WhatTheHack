using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(StrippableUtility), "CanBeStrippedByColony")]
    class WorkGiver_Strip_HasJobOnThing
    {
        static bool Prefix(Thing th, ref bool __result)
        {
            if(th is Pawn){
                Pawn pawn = (Pawn)th;
                if(pawn.RaceProps.IsMechanoid 
                    && pawn.equipment != null && pawn.equipment.Primary != null && pawn.equipment.Primary.def.destroyOnDrop 
                    && pawn.inventory != null && pawn.inventory.innerContainer.Count == 0
                    && pawn.apparel != null && pawn.apparel.WornApparelCount == 0)
                {
                    Log.Message("HasJobOnThing returning false");
                    __result = false;
                    return false;
                }
            }
            return true;
        }
    }
}
