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
        public List<Pawn> controlledMechs = new List<Pawn>();
        private const int MAXCONTROLLABLE = 6;

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
            foreach (Gizmo gizmo in base.GetGizmos())
            {

                yield return gizmo;
            }
            yield return GetRemoteControlActivateGizmo();
            foreach (Pawn mech in controlledMechs)
            {
                yield return GetRemoteControlCancelGizmo(mech);
            }
        }

        private class Command_Action_Highlight : Command_Action
        {
            public Building_RogueAI parent;
            public Pawn pawn;
            public override void ProcessInput(Event ev)
            {
                base.ProcessInput(ev);
            }
            public override void GizmoUpdateOnMouseover()
            {
                Log.Message("GizmoUpdateOnMouseover");
                base.GizmoUpdateOnMouseover();
                GenDraw.DrawLineBetween(parent.Position.ToVector3Shifted(), pawn.Position.ToVector3Shifted(), SimpleColor.White);
            }
        }

        private Gizmo GetRemoteControlCancelGizmo(Pawn mech)
        {
            Command_Action_Highlight command = new Command_Action_Highlight();
            command.defaultLabel = mech.Name.ToStringShort;//TODO
            command.defaultDesc = "WTH_Gizmo_RemoteControlActivate_Description".Translate();//TODO
            command.parent = this;
            command.pawn = mech;
            bool iconFound = Base.Instance.cancelControlTextures.TryGetValue(mech.def.defName, out Texture2D icon);
            if (iconFound)
            {
                command.icon = icon;
            }
            command.action = delegate
            {
                ExtendedPawnData mechData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(mech);
                mechData.controllingAI = null;
                controlledMechs.Remove(mech);
                mechData.isActive = false;
                mech.drafter.Drafted = false;
            };
            return command;
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
                if (target is Pawn mech)
                {
                    ExtendedPawnData mechData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(mech);
                    mechData.controllingAI = this;
                    controlledMechs.Add(mech);
                    mechData.isActive = true;
                    mech.drafter.Drafted = true;
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
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref mood, "mood");
            Scribe_Values.Look(ref activated, "activated");
            Scribe_Collections.Look(ref controlledMechs, "controlledMechs", LookMode.Reference);

        }
    }
}
