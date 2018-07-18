using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using WhatTheHack.Buildings;


namespace WhatTheHack.Harmony
{

    [HarmonyPatch(typeof(Building_Bed), "GetSleepingSlotPos")]
    static class Building_Bed_GetSleepingSlotPos
    {
        static void Postfix(Building_Bed __instance, ref IntVec3 __result)
        {
            if(__instance is Building_MechanoidPlatform)
            {
                __result = __instance.InteractionCell;
            }       
        }
    }
    
    
    

    //Patch is needed so mechanoids that are standing up can still have a "cur bed"
    [HarmonyPatch(typeof(Building_Bed), "GetCurOccupant")]
    static class Building_Bed_GetCurOccupant
    {
        //Copied from vanilla, prefixing and replacing is safe because of the check for Building_MechanoidPlatform
        static bool Prefix(Building_Bed __instance, int slotIndex, ref Pawn __result)
        {
            if(!(__instance is Building_BaseMechanoidPlatform)){
                return true;
            }

            if (!__instance.Spawned)
            {
                return false;
            }
            
            IntVec3 sleepingSlotPos = __instance.GetSleepingSlotPos(slotIndex);
            List<Thing> list = __instance.Map.thingGrid.ThingsListAt(sleepingSlotPos);
            for (int i = 0; i < list.Count; i++)
            {
                Pawn pawn = list[i] as Pawn;
                if (pawn != null)
                {
                    if (pawn.CurJob != null)
                    {
                        __result = pawn;
                    }
                }
            }
            return false;
        }
    }
}
