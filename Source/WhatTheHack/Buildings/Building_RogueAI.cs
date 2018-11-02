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

namespace WhatTheHack.Buildings
{
    public class Building_RogueAI : Building
    {
        private float mood = 0.5f;
        private bool activated = false;
        List<Pawn> controlledMechs = new List<Pawn>();


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref mood, "mood");
            Scribe_Values.Look(ref activated, "activated");
            Scribe_Collections.Look(ref controlledMechs, "controlledMechs");

        }
        //Increase mood when data is provided. 
        public void GiveData()
        {

        }
        //Start events when in bad mood; 
        public override void TickLong()
        {
            base.TickLong();
            
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach(Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            yield return GetRemoteControlActivateGizmo();
            foreach(Pawn mech in controlledMechs)
            {
                yield return GetRemoteControlCancelGizmo(mech);
            }
        }

        private Gizmo GetRemoteControlCancelGizmo(Pawn mech)
        {
            Command_Action command_Target = new Command_Action();
            command_Target.defaultLabel = mech.Name.ToStringShort;//TODO
            command_Target.defaultDesc = "WTH_Gizmo_RemoteControlActivate_Description".Translate();//TODO
            PawnKindLifeStage curKindLifeStage = mech.ageTracker.CurKindLifeStage;
            command_Target.icon = curKindLifeStage.bodyGraphicData.Graphic.MatEast.mainTexture as Texture2D;
            command_Target.action = delegate
            {
                ExtendedPawnData mechData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(mech);
                mechData.controllingAI = null;
                controlledMechs.Remove(mech);
                mechData.isActive = false;
                mech.drafter.Drafted = false;
            };
            return command_Target;
        }

        private Gizmo GetRemoteControlActivateGizmo()
        {
            Command_Target command_Target = new Command_Target();
            command_Target.defaultLabel = "WTH_Gizmo_RemoteControlActivate_Label".Translate();//TODO
            command_Target.defaultDesc = "WTH_Gizmo_RemoteControlActivate_Description".Translate();//TODO
            command_Target.targetingParams = GetTargetingParametersForHacking();
            command_Target.hotKey = KeyBindingDefOf.Misc5;
            command_Target.icon = ContentFinder<Texture2D>.Get(("Things/MechControllerBelt"));//TODO
            command_Target.action = delegate (Thing target)
            {
                if (target is Pawn)
                {
                    Pawn mech = (Pawn)target;
                    ExtendedPawnData mechData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(mech);
                    mechData.controllingAI = this;
                    controlledMechs.Add(mech);
                    mechData.isActive = true;
                    mech.drafter.Drafted = true;
                    //Find.Selector.ClearSelection();
                    //Find.Selector.Select(mech);
                }
            };
            return command_Target;
        }
        private static TargetingParameters GetTargetingParametersForHacking()
        {
            return new TargetingParameters
            {
                canTargetPawns = true,
                canTargetBuildings = false,
                mapObjectTargetsMustBeAutoAttackable = false,
                validator = delegate (TargetInfo targ)
                {
                    if (!targ.HasThing)
                    {
                        return false;
                    }
                    Pawn pawn = targ.Thing as Pawn;
                    return pawn != null && !pawn.Downed && pawn.IsHacked();
                }
            };
        }
    }
}
