using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using WhatTheHack.Buildings;

namespace WhatTheHack.Jobs
{
    class WorkGiver_HackRogueAI : WorkGiver_Scanner
    {
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Building_RogueAI rogueAI = t as Building_RogueAI;
            if (rogueAI != null && rogueAI.goingRogue)
            {
                LocalTargetInfo target = rogueAI;
                if (pawn.CanReserveAndReach(target, PathEndMode.ClosestTouch, Danger.Deadly, 10, 1, null, forced))
                {
                    return true;
                }
            }
            return false;
        }
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Building_RogueAI rogueAI = t as Building_RogueAI;
            return new Job(WTH_DefOf.WTH_HackRogueAI, rogueAI);
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
                return ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial);
            }
        }
        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }
    }
}
