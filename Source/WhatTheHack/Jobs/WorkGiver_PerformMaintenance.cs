using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using WhatTheHack.Needs;

namespace WhatTheHack.Jobs
{
    class WorkGiver_PerformMaintenance : WorkGiver_Scanner
    {
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Pawn targetPawn = t as Pawn;
            bool result;
            if (targetPawn != null && targetPawn.needs != null && targetPawn.needs.TryGetNeed<Need_Maintenance>() is Need_Maintenance need && targetPawn.OnBaseMechanoidPlatform())
            {
                LocalTargetInfo target = targetPawn;
                if (pawn.CanReserve(target, 1, -1, null, forced) && need.CurLevel < GetThresHold(need) && FindMechanoidParts(pawn, targetPawn) != null)
                {
                    result = true;
                    return result;
                }
            }
            result = false;
            return result;
        }
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Pawn targetPawn = t as Pawn;
            Thing thing = FindMechanoidParts(pawn, targetPawn);
            return new Job(WTH_DefOf.WTH_PerformMaintenance, targetPawn, thing);
        }

        protected virtual float GetThresHold(Need_Maintenance need)
        {
            return need.MaxLevel * 0.5f;
        }

        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.InteractionCell;
            }
        }

        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                return ThingRequest.ForGroup(ThingRequestGroup.Pawn);
            }
        }
        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }

        private static Thing FindMechanoidParts(Pawn hacker, Pawn targetPawn)
        {
            Thing result = null;
            if (targetPawn.needs.TryGetNeed<Need_Maintenance>() is Need_Maintenance need)
            {
                Predicate<Thing> predicate = (Thing m) => !m.IsForbidden(hacker) && hacker.CanReserve(m, 10, 1, null, false);
                IntVec3 position = targetPawn.Position;
                Map map = targetPawn.Map;
                List<Thing> searchSet = targetPawn.Map.listerThings.ThingsOfDef(WTH_DefOf.WTH_MechanoidParts);
                PathEndMode peMode = PathEndMode.ClosestTouch;
                TraverseParms traverseParams = TraverseParms.For(hacker, Danger.Deadly, TraverseMode.ByPawn, false);
                Predicate<Thing> validator = predicate;
                result = GenClosest.ClosestThing_Global_Reachable(position, map, searchSet, peMode, traverseParams, 9999f, validator);
            }
            return result;
        }

    }
}
