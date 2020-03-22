using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using WhatTheHack.Storage;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(Pawn_StoryTracker), "get_DisabledWorkTagsBackstoryAndTraits")]
    class Pawn_StoryTracker_get_DisabledWorkTypes
    {
        static bool Prefix(Pawn_StoryTracker __instance, ref WorkTags __result)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if(pawn.RaceProps.IsMechanoid && pawn.IsHacked())
            {
                WorkTags shouldForbid = __result;
                ExtendedDataStorage store = Base.Instance.GetExtendedDataStorage();
                if(store != null)
                {
                    ExtendedPawnData pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(pawn);
                    foreach (WorkTypeDef def in DefDatabase<WorkTypeDef>.AllDefs)
                    {
                        if (pawnData.workTypes == null || !pawnData.workTypes.Contains(def))
                        {
                            shouldForbid |= def.workTags;
                        }
                    }
                    __result = shouldForbid;
                    return false;
                }    
            }
            return true;
        }
    }
}
