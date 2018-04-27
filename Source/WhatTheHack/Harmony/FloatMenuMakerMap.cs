using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using WhatTheHack.Duties;
using WhatTheHack.Storage;

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

                ExtendedPawnData pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(targetPawn);
                if (!pawnData.isHacked)
                {
                    Action action = delegate
                    {
                        targetPawn.SetFaction(Faction.OfPlayer);
                        if (targetPawn.story == null)
                        {
                            Log.Message("story was null");
                            targetPawn.story = new Pawn_StoryTracker(targetPawn);
                        }
                        pawnData.isHacked = true;

                    };
                    __result.Add(new FloatMenuOption("(Godmode) hack " + targetPawn.Name, action, MenuOptionPriority.Low));
                }

                if (pawnData.isHacked)
                {
                    Action action = delegate
                    {
                        List<Pawn> pawns = new List<Pawn>
                        {
                            targetPawn
                        };
                        LordMaker.MakeNewLord(Faction.OfPlayer, new LordJob_SearchAndDestroy(), targetPawn.Map, pawns);
                    };
                    __result.Add(new FloatMenuOption("(Godmode) activate " + targetPawn.Name, action, MenuOptionPriority.Low));
                    pawnData.isActive = true;
                }




            }
        }
    }
}
