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
        public bool managingPowerNetwork = false;
        public bool goingRogue = false;

        public List<Pawn> controlledMechs = new List<Pawn>();
        public List<Pawn> hackedMechs = new List<Pawn>();
        public List<Pawn> rogueMechs = new List<Pawn>();

        public List<Building_TurretGun> controlledTurrets = new List<Building_TurretGun>();
        public List<Building_TurretGun> rogueTurrets = new List<Building_TurretGun>();


        private const int MAXCONTROLLABLEMECHS = 6;
        private const int MAXCONTROLLABLETURRETS = 4;
        private const int MAXHACKABLE = 2;
        private const int NUMTEXTSHAPPY = 50;
        private const int NUMTEXTSANNOYED = 50;
        private const int NUMTEXTSMAD = 11;
        private int textTimeout = 0;
        //Increase mood when data is provided. 
        public void GiveData()
        {

        }

        public override void TickRare()
        {
            base.TickRare();
            
            if (Rand.Chance(0.02f) && textTimeout <= 0)
            {
                string randomColonistName = this.Map.mapPawns.FreeColonists.RandomElement().Name.ToString();
                string text = "WTH_RogueAI_Happy_Remark_" + Rand.RangeInclusive(0, NUMTEXTSHAPPY - 1);
                Utilities.ThrowStaticText(this.DrawPos + new Vector3(0,0,1.75f), this.Map, text.Translate(new object[] { randomColonistName }), Color.white, 6f);
                textTimeout += 24;
            }
            if(textTimeout > 0)
            {
                textTimeout--;
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {

                yield return gizmo;
            }
            yield return GetManagePowerNetworkGizmo();
            yield return GetControlMechanoidActivateGizmo();
            yield return GetControlTurretAvtivateGizmo();
            yield return GetHackingAvtivateGizmo();

            foreach (Pawn mech in controlledMechs)
            {
                yield return GetControlMechanoidCancelGizmo(mech);
            }
            int index = 1;

            foreach (Building_TurretGun turret in controlledTurrets)
            {
                yield return GetControlTurretCancelGizmo(turret, index);
                index++;
            }
            index = 1;

            foreach (Pawn mech in hackedMechs)
            {
                yield return GetHackingCancelGizmo(mech, index);
                index++;
            }
        }

        private class Command_Action_Highlight : Command_Action
        {
            public Building_RogueAI parent;
            public Thing thing;
            public override void ProcessInput(Event ev)
            {
                base.ProcessInput(ev);
            }
            public override void GizmoUpdateOnMouseover()
            {
                base.GizmoUpdateOnMouseover();
                GenDraw.DrawLineBetween(parent.Position.ToVector3Shifted(), thing.Position.ToVector3Shifted(), SimpleColor.White);
            }
        }

        private Gizmo GetManagePowerNetworkGizmo()
        {
            Command_Toggle command = new Command_Toggle();
            command.defaultLabel = "WTH_Gizmo_ManagePowerNetwork_Label".Translate();//TODO
            command.defaultDesc = "WTH_Gizmo_ManagePowerNetwork_Description".Translate();//TODO
            command.icon = ContentFinder<Texture2D>.Get("UI/RogueAI_Manage_Network", true);
            command.isActive = delegate
            {
                return managingPowerNetwork;
            };
            command.toggleAction = delegate
            {
                managingPowerNetwork = !managingPowerNetwork;
                GoRogue();
            };
            return command;
        }

        private Gizmo GetControlMechanoidCancelGizmo(Pawn mech)
        {
            Command_Action_Highlight command = new Command_Action_Highlight();
            command.defaultLabel = mech.Name.ToStringShort;//TODO
            command.defaultDesc = "WTH_Gizmo_ControlMechanoidCancel_Description".Translate();//TODO
            command.parent = this;
            command.thing = mech;
            
            bool iconFound = Base.Instance.cancelControlMechTextures.TryGetValue(mech.def.defName, out Texture2D icon);
            if (iconFound)
            {
                command.icon = icon;
            }
            command.action = delegate
            {
                CancelControlMechanoid(mech);
            };
            return command;
        }

        private void CancelControlMechanoid(Pawn mech)
        {
            ExtendedPawnData mechData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(mech);
            mechData.controllingAI = null;
            controlledMechs.Remove(mech);
            mech.drafter.Drafted = false;
        }

        private Gizmo GetControlMechanoidActivateGizmo()
        {
            Command_Target command = new Command_Target();
            command.defaultLabel = "WTH_Gizmo_ControlMechanoidActivate_Label".Translate();//TODO
            command.defaultDesc = "WTH_Gizmo_ControlMechanoidActivate_Description".Translate(new object[] { MAXCONTROLLABLEMECHS });//TODO
            command.targetingParams = GetTargetingParametersForControlling();
            command.hotKey = KeyBindingDefOf.Misc5;
            command.icon = ContentFinder<Texture2D>.Get(("UI/RogueAI_Control"));//TODO
            command.disabled = controlledMechs.Count >= MAXCONTROLLABLEMECHS;
            command.disabledReason = "TODO";
            command.action = delegate (Thing target)
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
            return command;
        }
        private static TargetingParameters GetTargetingParametersForControlling()
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

        private Gizmo GetHackingCancelGizmo(Pawn mech, int index)
        {
            Command_Action_Highlight command = new Command_Action_Highlight();
            command.defaultLabel = "WTH_Gizmo_HackingCancel_Label".Translate() + " " + index;//TODO
            command.defaultDesc = "WTH_Gizmo_HackingCancel_Description".Translate();//TODO
            command.parent = this;
            command.thing = mech;

            bool iconFound = Base.Instance.cancelControlMechTextures.TryGetValue(mech.def.defName, out Texture2D icon);
            if (iconFound)
            {
                command.icon = icon;
            }
            command.action = delegate
            {
                CancelHacking(mech);
            };
            return command;
        }

        private void CancelHacking(Pawn mech)
        {
            ExtendedPawnData mechData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(mech);
            mechData.controllingAI = null;
            mech.RevertToFaction(mechData.originalFaction);
            mech.drafter.Drafted = false;
            hackedMechs.Remove(mech);
        }

        private Gizmo GetHackingAvtivateGizmo()
        {
            Command_Target command = new Command_Target();
            command.defaultLabel = "WTH_Gizmo_HackingAvtivate_Label".Translate();//TODO
            command.defaultDesc = "WTH_Gizmo_HackingAvtivate_Description".Translate(new object[] { MAXHACKABLE });//TODO
            command.targetingParams = GetTargetingParametersForHacking();
            command.hotKey = KeyBindingDefOf.Misc5;
            command.icon = ContentFinder<Texture2D>.Get(("UI/RogueAI_Hack"));//TODO
            command.disabled = hackedMechs.Count >= MAXHACKABLE;
            command.disabledReason = "TODO";
            command.action = delegate (Thing target)
            {
                if (target is Pawn mech)
                {
                    ExtendedPawnData mechData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(mech);
                    mechData.originalFaction = mech.Faction;
                    mechData.controllingAI = this;
                    hackedMechs.Add(mech);
                    mech.SetFaction(Faction.OfPlayer);
                    mech.story = new Pawn_StoryTracker(mech);
                    mech.jobs.EndCurrentJob(JobCondition.InterruptForced);
                    mech.drafter = new Pawn_DraftController(mech);
                    mech.drafter.Drafted = true;
                }
            };
            return command;
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
                    return pawn != null && !pawn.Downed && pawn.RaceProps.IsMechanoid && pawn.Faction != Faction.OfPlayer;
                }
            };
        }

        private Gizmo GetControlTurretCancelGizmo(Building_TurretGun turret, int index)
        {
            Command_Action_Highlight command = new Command_Action_Highlight();
            command.defaultLabel = "WTH_Gizmo_ControlTurretCancel_Label".Translate() + index;//TODO
            command.defaultDesc = "WTH_Gizmo_ControlTurretCancel_Description".Translate();//TODO
            command.parent = this;
            command.thing = turret;

            bool iconFound = Base.Instance.cancelControlTurretTextures.TryGetValue(turret.def.defName, out Texture2D icon);
            if (iconFound)
            {
                command.icon = icon;
            }
            command.action = delegate
            {
                CancelControlTurret(turret);
            };
            return command;
        }

        private void CancelControlTurret(Building_TurretGun turret)
        {
            controlledTurrets.Remove(turret);
        }

        private Gizmo GetControlTurretAvtivateGizmo()
        {
            Command_Target command = new Command_Target();
            command.defaultLabel = "WTH_Gizmo_ControlTurretActivate_Label".Translate();//TODO
            command.defaultDesc = "WTH_Gizmo_ControlTurretActivate_Description".Translate(new object[] { MAXCONTROLLABLETURRETS });//TODO
            command.targetingParams = GetTargetingParametersForControlTurret();
            command.icon = ContentFinder<Texture2D>.Get(("UI/RogueAI_Control_Turret"));//TODO
            command.disabled = hackedMechs.Count >= MAXCONTROLLABLETURRETS;
            command.disabledReason = "TODO";
            command.action = delegate (Thing target)
            {
                if (target is Building_TurretGun turret)
                {
                    controlledTurrets.Add(turret);
                }
            };
            return command;
        }
        private static TargetingParameters GetTargetingParametersForControlTurret()
        {
            return new TargetingParameters
            {
                canTargetPawns = false,
                canTargetBuildings = true,
                mapObjectTargetsMustBeAutoAttackable = false,
                validator = delegate (TargetInfo targ)
                {
                    if (!targ.HasThing)
                    {
                        return false;
                    }
                    Building building = targ.Thing as Building;
                    return building != null && building is Building_TurretGun && building.Faction == Faction.OfPlayer;
                }
            };
        }

        private void GoRogue()
        {
            goingRogue = true;
            foreach(Pawn mech in controlledMechs)
            {
                CancelControlMechanoid(mech);
            }
            foreach(Building_TurretGun turret in controlledTurrets)
            {
                CancelControlTurret(turret);
            }
            foreach(Pawn mech in hackedMechs)
            {
                CancelHacking(mech);
            }
           
            List<Pawn> shouldHack = this.Map.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer).Where((Pawn p) => p.IsHacked() && !p.Downed).ToList();
            GoRogue_HackMechs(shouldHack);
            List<Building_TurretGun> shouldHackTurrets = this.Map.spawnedThings.Where((Thing t) => t is Building_TurretGun && t.Faction == Faction.OfPlayer).Cast<Building_TurretGun>().ToList();
            GoRogue_HackTurrets(shouldHackTurrets);
        }
        private void GoRogue_HackMechs(List<Pawn> shouldHack)
        {
            int nShouldHack = Rand.Range(1, 4);
            while(nShouldHack > 0 && shouldHack.Count > 0)
            {
                Pawn mech = shouldHack.RandomElement();
                if (mech.GetLord() == null || mech.GetLord().LordJob == null)
                {
                    LordMaker.MakeNewLord(Faction.OfMechanoids, new LordJob_AssaultColony(Faction.OfMechanoids, true, true, false, false, true), mech.Map, new List<Pawn> { mech });
                }
                mech.jobs.EndCurrentJob(JobCondition.InterruptForced);
                mech.RevertToFaction(Faction.OfMechanoids);
                nShouldHack--;
                shouldHack.Remove(mech);
                rogueMechs.Add(mech);
            }
         
        }
        private void GoRogue_HackTurrets(List<Building_TurretGun> turrets)
        {
            int nShouldHack = Rand.Range(1, 3);
            while (nShouldHack > 0 && turrets.Count > 0)
            {
                Building_TurretGun turret = turrets.RandomElement();
                turret.SetFaction(Faction.OfMechanoids);
                nShouldHack--;
                turrets.Remove(turret);
                rogueTurrets.Add(turret);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref mood, "mood");
            Scribe_Values.Look(ref activated, "activated");
            Scribe_Values.Look(ref goingRogue, "goingRogue");
            Scribe_Values.Look(ref managingPowerNetwork, "managingPowerNetwork");
            Scribe_Collections.Look(ref controlledMechs, "controlledMechs", LookMode.Reference);
            Scribe_Collections.Look(ref hackedMechs, "hackedMechs", LookMode.Reference);
            Scribe_Collections.Look(ref controlledTurrets, "controlledTurrets", LookMode.Reference);
            Scribe_Collections.Look(ref rogueMechs, "rogueMechs", LookMode.Reference);
            Scribe_Collections.Look(ref rogueTurrets, "rogueTurrets", LookMode.Reference);
        }
    }
}
