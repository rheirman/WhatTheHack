using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using WhatTheHack.Storage;

namespace WhatTheHack.Harmony
{
    /*
    [HarmonyPatch(typeof(JobDriver_Wait), "CheckForAutoAttack")]
    static class JobDriver_Wait_CheckForAutoAttack
    {
        static void Prefix(JobDriver_Wait __instance)
        {
            if (__instance.pawn.RaceProps.IsMechanoid && __instance.pawn.Faction == Faction.OfPlayer) {
                Log.Message("checkforautoattack called");

                bool flag = __instance.pawn.story == null || !__instance.pawn.story.WorkTagIsDisabled(WorkTags.Violent);
                if (flag && __instance.pawn.Faction != null && __instance.job.def == JobDefOf.WaitCombat && (__instance.pawn.drafter == null || __instance.pawn.drafter.FireAtWill))
                {
                    bool allowManualCastWeapons = !__instance.pawn.IsColonist;
                    Verb verb = __instance.pawn.TryGetAttackVerb(allowManualCastWeapons);
                    if (verb != null && !verb.verbProps.MeleeRange)
                    {
                        TargetScanFlags targetScanFlags = TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedLOSToNonPawns | TargetScanFlags.NeedThreat;
                        if (verb.IsIncendiary())
                        {
                            targetScanFlags |= TargetScanFlags.NeedNonBurning;
                        }
                        Thing thing = (Thing)AttackTargetFinder.BestShootTargetFromCurrentPosition(__instance.pawn, null, verb.verbProps.range, verb.verbProps.minRange, targetScanFlags);
                        if (thing != null)
                        {
                            Log.Message("try start attack");
                            Log.Message("target def: " + thing.def.defName);
                            __instance.pawn.TryStartAttack(thing);
                            return;
                        }
                    }
                    else
                    {
                        Log.Message("verb was null");
                    }
                }
            }

        }

    }
            */
}
