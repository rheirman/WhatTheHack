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
using WhatTheHack.Buildings;
using WhatTheHack.Duties;
using WhatTheHack.Storage;

namespace WhatTheHack.Harmony
{
    /*This Harmony Postfix makes the creature respond to clicks on the map screen, so it can be controlled
 */
     /*
    [HarmonyPatch(typeof(FloatMenuMakerMap), "CanTakeOrder")]
    public static class FloatMenuMakerMap_CanTakeOrder
    {
        public static void Postfix(Pawn pawn, ref bool __result)
        {
            __result = pawn.CanTakeOrder();
        }
    }
    */
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

                if (!targetPawn.IsHacked() && targetPawn.Downed && !pawn.OnHackingTable())
                {

                    Building_HackingTable closestAvailableTable = Utilities.GetAvailableHackingTable(pawn, targetPawn);
                    if (closestAvailableTable != null)
                    {
                        Action action = delegate
                        {
                            Job job = new Job(WTH_DefOf.CarryToHackingTable, targetPawn, closestAvailableTable);
                            job.count = 1;
                            pawn.jobs.TryTakeOrderedJob(job);
                        };
                        __result.Add(new FloatMenuOption("Carry to hacking table " + targetPawn.Name, action, MenuOptionPriority.Low));
                    }                
                    else if (!pawn.OnHackingTable())
                    {
                        __result.Add(new FloatMenuOption("Carry to hacking table (no free table reachable)" + targetPawn.Name, null, MenuOptionPriority.Low));
                    }
                }

                //Following menu options are only for testing, and only enabled when godmode is on. 
                if (!Verse.DebugSettings.godMode)
                {
                    return;
                }
                ExtendedPawnData pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(targetPawn);
                /*
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
                */


                //Allows you to instantly activate mechanoid with the right click menu.
                if (targetPawn.IsHacked())
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
