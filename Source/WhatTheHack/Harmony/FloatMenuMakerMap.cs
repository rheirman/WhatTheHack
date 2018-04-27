using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace WhatTheHack.Harmony
{
    //Useful for testing, only enabled when godmode is on. Allows you to instantly hack mechanoid with the right click menu.
    [HarmonyPatch(typeof(FloatMenuMakerMap), "ChoicesAtFor")]
    static class FloatMenuMakerMap_ChoicesAtFor
    {
        static void Postfix(Vector3 clickPos, Pawn pawn, ref List<FloatMenuOption> __result)
        {
            foreach (LocalTargetInfo current in GenUI.TargetsAt(clickPos, TargetingParameters.ForAttackHostile(), true))
            {
                if (!Verse.DebugSettings.godMode)
                {
                    return;
                }
                if (!(current.Thing is Pawn) || !((Pawn)current.Thing).RaceProps.IsMechanoid)
                {
                    return;
                }
                Pawn targetPawn = current.Thing as Pawn;
                Action action = delegate
                {
                    targetPawn.SetFaction(Faction.OfPlayer);
                };
                __result.Add(new FloatMenuOption("(Godmode) hack " + targetPawn.Name, action, MenuOptionPriority.Low));
            }
        }
    }
}
