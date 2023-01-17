using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using WhatTheHack.Buildings;
using WhatTheHack.ThinkTree;
using WhatTheHack.Storage;

namespace WhatTheHack.Harmony
{

    [HarmonyPatch(typeof(FloatMenuMakerMap), "AddJobGiverWorkOrders")]
    static class FloatMenuMakerMap_AddJobGiverWorkOrders
    {
        static bool Prefix(Pawn pawn)
        {
            if (pawn.IsHacked())
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
    [HarmonyPatch(typeof(FloatMenuMakerMap), "ChoicesAtFor")]
    static class FloatMenuMakerMap_ChoicesAtFor
    {
        static void Postfix(Vector3 clickPos, Pawn pawn, ref List<FloatMenuOption> __result)
        {
            foreach (LocalTargetInfo current in GenUI.TargetsAt(clickPos, TargetingParameters.ForAttackHostile(), true))
            {

                if (!(current.Thing is Pawn) || !((Pawn)current.Thing).RaceProps.IsMechanoid)
                {
                    return;
                }
                Pawn targetPawn = current.Thing as Pawn;

                if (targetPawn.OnHackingTable())
                {
                    Action action = delegate
                    {
                        Job job = new Job(WTH_DefOf.WTH_ClearHackingTable, targetPawn, targetPawn.CurrentBed());
                        job.count = 1;
                        pawn.jobs.TryTakeOrderedJob(job);
                    };
                    __result.Add(new FloatMenuOption("WTH_Menu_ClearTable".Translate(), action, MenuOptionPriority.Low));
                }
            }
        }

    }
}
