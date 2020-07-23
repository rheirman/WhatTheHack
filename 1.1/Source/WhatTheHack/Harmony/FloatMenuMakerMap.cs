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

    [HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
    static class FloatMenuMakerMap_AddHumanlikeOrders
    {
        static void Postfix(ref Vector3 clickPos, ref Pawn pawn, ref List<FloatMenuOption> opts)
        {
            if (!pawn.story.traits.HasTrait(Utilities.hackerDef))
                return;

            TargetingParameters targetingParameters = new TargetingParameters
            {
                canTargetPawns = true,
                canTargetBuildings = false
            };

            Pawn localpawn = pawn;

            foreach (LocalTargetInfo localTargetInfo in GenUI.TargetsAt(clickPos, targetingParameters, true))
            {
                Pawn target = (Pawn)localTargetInfo.Thing;

                if (target.Dead
                    || !target.Downed
                    || target.def.race == null
                    || !target.def.race.IsMechanoid
                    || target.Faction == Faction.OfPlayer)
                {
                    continue;
                }

                void HackAction()
                {
                    JobDef hackJobDef = new JobDef
                    {
                        defName = "WTH_HackDownedMecha",
                        driverClass = typeof(Jobs.JobDriver_HackDownedMecha),
                        reportString = "Hacking downed mecha",
                        casualInterruptible = false
                    };
                    Job hack = new Job(hackJobDef, target);
                    localpawn.jobs.TryTakeOrderedJob(hack);
                }
                opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption($"Hack mechanoid", HackAction, MenuOptionPriority.InitiateSocial, null, localTargetInfo.Thing), pawn, target));
            }
        }
    }
}
