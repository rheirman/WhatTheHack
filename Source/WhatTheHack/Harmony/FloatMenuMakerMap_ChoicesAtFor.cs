using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(FloatMenuMakerMap), "ChoicesAtFor")]
internal static class FloatMenuMakerMap_ChoicesAtFor
{
    private static void Postfix(Vector3 clickPos, Pawn pawn, ref List<FloatMenuOption> __result)
    {
        foreach (var current in GenUI.TargetsAt(clickPos, TargetingParameters.ForAttackHostile(), true))
        {
            if (!(current.Thing is Pawn targetPawn) || !targetPawn.IsMechanoid())
            {
                return;
            }

            if (!targetPawn.OnHackingTable())
            {
                continue;
            }

            void Action()
            {
                var job = new Job(WTH_DefOf.WTH_ClearHackingTable, targetPawn, targetPawn.CurrentBed()) { count = 1 };
                pawn.jobs.TryTakeOrderedJob(job);
            }

            __result.Add(new FloatMenuOption("WTH_Menu_ClearTable".Translate(), Action, MenuOptionPriority.Low));
        }
    }
}