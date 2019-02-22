using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(Pawn_StoryTracker), "get_DisabledWorkTypes")]
    class Pawn_StoryTracker_get_DisabledWorkTypes
    {
        static bool Prefix(Pawn_StoryTracker __instance, ref List<WorkTypeDef> __result)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if(pawn.RaceProps.IsMechanoid && pawn.IsHacked())
            {
                List<WorkTypeDef> shouldForbid = new List<WorkTypeDef>();

                foreach (WorkTypeDef def in DefDatabase<WorkTypeDef>.AllDefs)
                {
                    if (!Base.allowedMechWork.Contains(def))
                    {
                        shouldForbid.Add(def);
                    }
                }
                __result = shouldForbid;
                return false;
            }
            return true;
        }
    }
}
