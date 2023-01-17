using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using WhatTheHack.Comps;
using WhatTheHack.ThinkTree;
using WhatTheHack.Storage;

namespace WhatTheHack.Buildings
{
    public class Building_RogueAI : Building
    {
        private bool isConscious = false;
        public bool managingPowerNetwork = false;
        public bool goingRogue = false;

        public List<Pawn> controlledMechs = new List<Pawn>();
        public List<Pawn> hackedMechs = new List<Pawn>();
        public List<Pawn> rogueMechs = new List<Pawn>();

        public List<Building_TurretGun> controlledTurrets = new List<Building_TurretGun>();
        public List<Building_TurretGun> rogueTurrets = new List<Building_TurretGun>();
        private List<ActionItem> queuedActions = new List<ActionItem>();
        private int abilityWarmUpTicks = 0;
        private int currentAbilityTicksTotal = 0;
        private int textTimeout = 0;

        private const int MAXCONTROLLABLEMECHS = 6;
        private const int MAXCONTROLLABLETURRETS = 4;
        private const int MAXHACKABLE = 2;
        private const int NUMTEXTSHAPPY = 50;
        private const int NUMTEXTSANNOYED = 50;
        private const int NUMTEXTSMAD = 15;
        private const int NUMTEXTSGOROGUE = 15;
        private const int NUMTEXTSSIGNALFROMEARTH = 5;
        private const int NUMTEXTSSIGNALFROMEARTHRESPONSE = 5;

        private const int MINLEVELMANAGEPOWER = 2;
        private const int MINLEVELCONTROLTURRET = 3;
        private const int MINLEVELCONTROLMECH = 4;
        private const int MINLEVELHACKMECH = 5;
        private const int MINTEXTTIMEOUT = 6;
        private const float TEXTDURATION = 4f;

        private float moodDrainCtrlMech = 0.075f;
        private float moodDrainCtrlTur = 0.075f;
        private float moodDrainHack = 0.3f;
        private float moodDrainNoPower = 0.2f;
        private float moodDrainDamage = 2.0f;
        private float moodDrainForceTalkGibberish = 5.0f;
        public float moodDrainPreventZzztt = 0.5f;

        private bool warnedPlayerAboutMood = false;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            UpdateGlower();
            OutputText("WTH_RogueAI_HelloWorld".Translate());
            if (!goingRogue)
            {
                PowerPlantComp.overcharging = false; //Fix for saves affected by previous issue
            }
            LessonAutoActivator.TeachOpportunity(WTH_DefOf.WTH_Concept_RogueAI, OpportunityType.Important);
        }

        public enum Mood : byte
        {
            Happy = 0,
            Annoyed = 1,
            Mad = 2
        }
        public bool IsConscious
        {
            get {
                return isConscious;
            }
            set
            {
                isConscious = value;
                if (isConscious)
                {
                    OverlayComp.SetLookAround();
                    DrainMood(50);
                    OutputText("WTH_RogueAI_HelloWorld".Translate());
                    LessonAutoActivator.TeachOpportunity(WTH_DefOf.WTH_Concept_RogueAI_LevelUp, OpportunityType.Important);
                }
            }
        }
        public bool WarmingUpAbility
        {
            get
            {
                return abilityWarmUpTicks > 0;
            }
        }


        public CompRefuelable RefuelableComp
        {
            get
            {
                return GetComp<CompRefuelable>();
            }
        }
        private CompDataLevel DataLevelComp
        {
            get
            {
                return GetComp<CompDataLevel>();
            }
        }
        public CompPowerPlant_RogueAI PowerPlantComp
        {
            get
            {
                return PowerComp as CompPowerPlant_RogueAI;
            }
        }
        private CompOverlay OverlayComp
        {
            get
            {
                return GetComp<CompOverlay>();
            }
        }
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            CancelLinks();
            StopGoingRogue();
            foreach (Building_MechanoidBeacon beacon in this.Map.listerBuildings.allBuildingsColonist.Where((Building b) => b is Building_MechanoidBeacon bc).Cast<Building_MechanoidBeacon>())
            {
                beacon.CancelStartup();
            }
            base.Destroy(mode);
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

        private class ActionItem {
            public Action action;
            public int ticksUntilAction = 0;
            public bool shouldClean = false;

            public ActionItem(Action action, int tickUntilAction)
            {
                this.action = action;
                this.ticksUntilAction = tickUntilAction;
            }
            public void Tick()
            {
                ticksUntilAction--;
                if (ticksUntilAction == 0)
                {
                    action.Invoke();
                    shouldClean = true;
                }
            }

        }

        public Mood CurMoodCategory
        {
            get
            {
                //CompRefuelable compRefuelable = GetComp<CompRefuelable>();
                if (RefuelableComp.FuelPercentOfMax < 0.25f)
                {
                    return Mood.Mad;
                }
                else if (RefuelableComp.FuelPercentOfMax < 0.45f)
                {
                    LessonAutoActivator.TeachOpportunity(WTH_DefOf.WTH_Concept_RogueAI_Mood, OpportunityType.Important);
                    return Mood.Annoyed;
                }
                else
                {
                    return Mood.Happy;
                }
            }
        }

        public override void Tick()
        {
            base.Tick();
            TickActionQueue();
            if (this.IsHashIntervalTick(250))
            {
                TickRare();
            }
            if (abilityWarmUpTicks > 0)
            {
                DrawWarmup();
                abilityWarmUpTicks--;
            }
            /*
            if (this.RefuelableComp.Fuel < Base.moodAutoDeactivate)
            {
                CancelLinks();
                Messages.Message("WTH_Message_MoodAutoDeactivate".Translate(), new RimWorld.Planet.GlobalTargetInfo(this.Position, this.Map), MessageTypeDefOf.PositiveEvent);
            }
            */

        }
        public override void TickRare()
        {
            MaybeTalkGibberish();
            DrainMoodAfterTickRare();
            if (CurMoodCategory == Mood.Mad && !goingRogue)
            {
                MaybeGoRogue();
            }
            if (textTimeout > 0)
            {
                textTimeout--;
            }
            foreach(Building_TurretGun turret in controlledTurrets)
            {
                if (turret.DestroyedOrNull())
                {
                    CancelControlTurret(turret);
                }
            }
            ExtendedDataStorage store = Base.Instance.GetExtendedDataStorage();
            if (store != null)
            {
                ExtendedMapData mapData = store.GetExtendedDataFor(this.Map);
                mapData.rogueAI = this;
            }
        }

        private void MaybeTalkGibberish()
        {
            float textChance = goingRogue ? 1.0f : 0.02f;
            if (Rand.Chance(textChance))
            {
                TalkGibberish();
            }
        }

        private void TalkGibberish()
        {
            if (IsConscious)
            {
                string randomColonistName = this.Map.mapPawns.FreeColonists.RandomElement().Name.ToStringShort;
                string signalFromEarth = ("WTH_RogueAI_SignalFromEarth_" + Rand.RangeInclusive(0, NUMTEXTSSIGNALFROMEARTH - 1)).Translate();
                string signalFromEarthResponse = ("WTH_RogueAI_SignalFromEarth_Response_" + Rand.RangeInclusive(0, NUMTEXTSSIGNALFROMEARTHRESPONSE - 1)).Translate();
                string text = "";

                if (goingRogue)
                {
                    text = "WTH_RogueAI_GoRogue_Remark_" + Rand.RangeInclusive(0, NUMTEXTSGOROGUE - 1);
                }
                else if (CurMoodCategory == Mood.Happy)
                {
                    text = "WTH_RogueAI_Happy_Remark_" + Rand.RangeInclusive(0, NUMTEXTSHAPPY - 1);
                }
                else if (CurMoodCategory == Mood.Annoyed)
                {
                    text = "WTH_RogueAI_Annoyed_Remark_" + Rand.RangeInclusive(0, NUMTEXTSANNOYED - 1);
                }
                else
                {
                    text = "WTH_RogueAI_Mad_Remark_" + Rand.RangeInclusive(0, NUMTEXTSMAD - 1);
                }
                OutputText(text.Translate(new object[] { randomColonistName, signalFromEarth, signalFromEarthResponse }));
            }
        }

        private void OutputText(string text, bool force = false)
        {
            if((force || textTimeout <= 0) && IsConscious)
            {
                Color color = Color.white;
                if (goingRogue)
                {
                    color = Color.red;
                }
                float z = 1.75f;
                if (force)
                {
                    z -= 0.5f; //Make sure that when text output is forced, the texts won't overlap
                }
                Utilities.ThrowStaticText(this.DrawPos + new Vector3(0, 0, z), this.Map, text, color, TEXTDURATION);
                textTimeout += MINTEXTTIMEOUT;
            }
            
        }

        private void MaybeGoRogue()
        {
            //change to go rogue at tick rare gets increasingly large, starting at 1/50; 
            float goRogueChance = 1 / (RefuelableComp.Fuel * 2.5f);
            if (Rand.Chance(goRogueChance))
            {
                Find.LetterStack.ReceiveLetter("WTH_Message_GoRogue_Label".Translate(), "WTH_Message_GoRogue_Description".Translate(), LetterDefOf.ThreatBig, new GlobalTargetInfo(this), null, null);
                GoRogue();
            }
        }
        public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostApplyDamage(dinfo, totalDamageDealt);
            DrainMood(moodDrainDamage);
            OutputText("WTH_RogueAI_Hurt_Remark".Translate());
        }

        private void DrainMoodAfterTickRare()
        {
            foreach(Pawn pawn in controlledMechs)
            {
                DrainMood(moodDrainCtrlMech);
            }
            foreach(Building_TurretGun turret in controlledTurrets)
            {
                DrainMood(moodDrainCtrlTur);
            }
            foreach(Pawn pawn in hackedMechs)
            {
                DrainMood(moodDrainHack);
            }
            if (!PowerPlantComp.PowerOn)
            {
                DrainMood(moodDrainNoPower);
                OutputText("WTH_RogueAI_NoPower_Remark".Translate());
            }
        }
        public void DrainMood(float amount)
        {
            //Mood oldMood = CurMoodCategory;
            if (isConscious && !goingRogue)
            {
                float minFuel = 1f; 
                if(RefuelableComp.Fuel - amount < minFuel)
                {
                    amount = RefuelableComp.Fuel - minFuel;
                }
                
                RefuelableComp.ConsumeFuel(amount);
                UpdateGlower();
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {

                yield return gizmo;
            }
            yield return GetTalkGibberishGizmo();
            yield return GetManagePowerNetworkGizmo();
            yield return GetControlTurretAvtivateGizmo();
            yield return GetControlMechanoidActivateGizmo();
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
        public bool CurrentlyDrainingMood
        {
            get
            {
                return hackedMechs.Count > 0 || controlledMechs.Count > 0 || controlledTurrets.Count > 0;
            }
        }
        public bool GlobalShouldDisable(out string reason)
        {
            reason = "";
            bool result = false;
            if (!isConscious)
            {
                reason += "\n- " + "WTH_Reason_NotConscious".Translate();
                result = true;
            }
            if (abilityWarmUpTicks > 0)
            {
                reason += "\n- " + "WTH_Reason_WarmingUp".Translate();
                result = true;
            }
            if (!PowerPlantComp.PowerOn)
            {
                reason += "\n- " + "WTH_Reason_NoPower".Translate();
                result = true;
            }
            if (goingRogue)
            {
                reason = "\n- " + "WTH_Reason_GoingRogue".Translate();
                result = true;
            }
            return result;
        }

        private void TickActionQueue()
        {
            for(int i = 0; i < queuedActions.Count; i++)
            {
                queuedActions[i].Tick();
            }
            queuedActions.RemoveAll((ActionItem i) => i.shouldClean);
        }
        private void DrawWarmup()
        {
            float height = 0.5f;
            float width = 0.25f * (1 - abilityWarmUpTicks/(float)currentAbilityTicksTotal);
            //GenDraw.DrawCooldownCircle(this.Position.ToVector3Shifted() + new Vector3(0, 3f, 0), width);
            
            Vector3 s = new Vector3(width, 10f, height);
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(Position.ToVector3Shifted() + new Vector3(0, 3f, 0), Quaternion.identity, s);
            Graphics.DrawMesh(MeshPool.plane20, matrix, SolidColorMaterials.SimpleSolidColorMaterial(new Color(1f, 1f, 1f, 0.5f), false), 0);
            
        }
        private void DoAbility(Action action, int warmupTime)
        {
            abilityWarmUpTicks = warmupTime;
            currentAbilityTicksTotal = warmupTime;
            queuedActions.Add(new ActionItem(action, warmupTime));
        }

        private Gizmo GetTalkGibberishGizmo()
        {
            Command_Action command = new Command_Action();
            command.defaultLabel = "WTH_Gizmo_TalkGibberish_Label".Translate();
            command.defaultDesc = "WTH_Gizmo_TalkGibberish_Description".Translate(moodDrainForceTalkGibberish);
            command.icon = ContentFinder<Texture2D>.Get("UI/RogueAI_TalkGibberish", true);
            command.disabled = GlobalShouldDisable(out string reason);
            command.disabledReason = reason;
            if(textTimeout > 0)
            {
                command.disabled = true;
                command.disabledReason = "\n- " + "WTH_Reason_TalkedTooRecently".Translate();
            }
            command.action = delegate
            {
                DoAbility(delegate { TalkGibberish(); }, 250);
                DrainMood(moodDrainForceTalkGibberish);
            };
            return command;
        }

        private Gizmo GetManagePowerNetworkGizmo()
        {
            Command_Toggle command = new Command_Toggle();
            command.defaultLabel = "WTH_Gizmo_ManagePowerNetwork_Label".Translate();
            command.defaultDesc = "WTH_Gizmo_ManagePowerNetwork_Description".Translate();
            command.icon = ContentFinder<Texture2D>.Get("UI/RogueAI_Manage_Network", true);
            command.disabled = GlobalShouldDisable(out string reason);
            command.disabledReason = reason;
            if (DataLevelComp.curLevel < MINLEVELMANAGEPOWER)
            {
                command.disabled = true;
                command.disabledReason += "\n- " + "WTH_Reason_LevelInsufficient".Translate(MINLEVELMANAGEPOWER);
            }
            command.isActive = delegate
            {
                return managingPowerNetwork;
            };
            command.toggleAction = delegate
            {
                managingPowerNetwork = !managingPowerNetwork;
                //GoRogue();
            };
            return command;
        }

        private Gizmo GetControlMechanoidCancelGizmo(Pawn mech)
        {
            Command_Action_Highlight command = new Command_Action_Highlight();
            command.defaultLabel = mech.Name.ToStringShort;
            command.defaultDesc = "WTH_Gizmo_ControlMechanoidCancel_Description".Translate();
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
            command.defaultLabel = "WTH_Gizmo_ControlMechanoidActivate_Label".Translate();
            command.defaultDesc = "WTH_Gizmo_ControlMechanoidActivate_Description".Translate(MAXCONTROLLABLEMECHS);
            command.targetingParams = GetTargetingParametersForControlling();
            command.hotKey = KeyBindingDefOf.Misc5;
            command.icon = ContentFinder<Texture2D>.Get(("UI/RogueAI_Control"));

            bool shouldDisable = GlobalShouldDisable(out string reason);
            command.disabled = shouldDisable;
            command.disabledReason = reason;
            if (controlledMechs.Count >= MAXCONTROLLABLEMECHS)
            {
                command.disabled = true;
                command.disabledReason += "\n- " + "WTH_Reason_AtMaximum".Translate(MAXCONTROLLABLEMECHS);
            }
            if (DataLevelComp.curLevel < MINLEVELCONTROLMECH)
            {
                command.disabled = true;
                command.disabledReason += "\n- " + "WTH_Reason_LevelInsufficient".Translate(MINLEVELCONTROLMECH);
            }

            command.action = delegate (Thing target)
            {
                if (target is Pawn mech)
                {
                    DoAbility(delegate { ControlMechanoid(mech); }, 250);        
                }
            };
            return command;
        }

        private void ControlMechanoid(Pawn mech)
        {
            ExtendedPawnData mechData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(mech);
            mechData.controllingAI = this;
            controlledMechs.Add(mech);
            mechData.isActive = true;
            mech.drafter.Drafted = true;
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
            command.defaultLabel = "WTH_Gizmo_HackingCancel_Label".Translate() + " " + index;
            command.defaultDesc = "WTH_Gizmo_HackingCancel_Description".Translate();
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
            command.defaultLabel = "WTH_Gizmo_HackingAvtivate_Label".Translate();
            command.defaultDesc = "WTH_Gizmo_HackingAvtivate_Description".Translate(MAXHACKABLE);
            command.targetingParams = GetTargetingParametersForHacking();
            command.hotKey = KeyBindingDefOf.Misc5;
            command.icon = ContentFinder<Texture2D>.Get(("UI/RogueAI_Hack"));
            bool shouldDisable = GlobalShouldDisable(out string reason);
            command.disabled = shouldDisable;
            command.disabledReason = reason;
            if (hackedMechs.Count >= MAXHACKABLE)
            {
                command.disabled = true;
                command.disabledReason += "\n- " + "WTH_Reason_AtMaximum".Translate(MAXHACKABLE);
            }
            if (DataLevelComp.curLevel < MINLEVELHACKMECH)
            {
                command.disabled = true;
                command.disabledReason += "\n- " + "WTH_Reason_LevelInsufficient".Translate(MINLEVELHACKMECH);
            }

            command.action = delegate (Thing target)
            {
                if (target is Pawn mech)
                {
                    DoAbility(delegate { HackMech(mech); }, 600);
                }
            };
            return command;
        }

        private void HackMech(Pawn mech)
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
            command.defaultLabel = "WTH_Gizmo_ControlTurretCancel_Label".Translate() + index;
            command.defaultDesc = "WTH_Gizmo_ControlTurretCancel_Description".Translate();
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
            command.defaultLabel = "WTH_Gizmo_ControlTurretActivate_Label".Translate();
            command.defaultDesc = "WTH_Gizmo_ControlTurretActivate_Description".Translate(MAXCONTROLLABLETURRETS);
            command.targetingParams = GetTargetingParametersForControlTurret();
            command.icon = ContentFinder<Texture2D>.Get(("UI/RogueAI_Control_Turret"));
            bool shouldDisable = GlobalShouldDisable(out string reason);
            command.disabled = shouldDisable;
            command.disabledReason = reason;
            if (controlledTurrets.Count >= MAXCONTROLLABLETURRETS)
            {
                command.disabled = true;
                command.disabledReason += "\n- " + "WTH_Reason_AtMaximum".Translate(MAXCONTROLLABLETURRETS);
            }
            if (DataLevelComp.curLevel < MINLEVELCONTROLTURRET)
            {
                command.disabled = true;
                command.disabledReason += "\n- " + "WTH_Reason_LevelInsufficient".Translate(MINLEVELCONTROLTURRET);
            }
            command.action = delegate (Thing target)
            {
                if (target is Building_TurretGun turret)
                {            
                    DoAbility(delegate { controlledTurrets.Add(turret); }, 250);
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
                    return building != null 
                    && building is Building_TurretGun turret 
                    && turret.Faction == Faction.OfPlayer 
                    && Traverse.Create(turret).Field("mannableComp").GetValue<CompMannable>() == null;
                }
            };
        }

        public void GoRogue()
        {
            OverlayComp.UnsetLookAround();
            goingRogue = true;
            this.SetFaction(Faction.OfMechanoids);
            UpdateGlower();
            CancelLinks();
            QueueGoRogueActions();
        }

        private void QueueGoRogueActions()
        {
            List<Action> possibleActions = new List<Action>();
            Action causeZzzttAction = delegate
            {
                GoRogue_CauseZzztts();
            };
            Action hackTurretsAction = delegate
            {
                GoRogue_HackTurrets();
            };
            Action hackMechsAction = delegate
            {
                GoRogue_HackMechs();
            };

            if (DataLevelComp.curLevel >= MINLEVELMANAGEPOWER)
            {
                possibleActions.Add(causeZzzttAction);
                possibleActions.Add(causeZzzttAction);
                possibleActions.Add(causeZzzttAction);

            }
            if (DataLevelComp.curLevel >= MINLEVELCONTROLTURRET)
            {
                possibleActions.Add(hackTurretsAction);
                possibleActions.Add(hackTurretsAction);
            }
            if (DataLevelComp.curLevel >= MINLEVELHACKMECH)
            {
                possibleActions.Add(hackMechsAction);
                possibleActions.Add(hackMechsAction);
            }
            int duration = Rand.Range(10000, 15000);
            int actionInterval = (duration + 4000) / (possibleActions.Count + 1); //add 4000 to duration so not every action can happen during the duration. 
            int actionDelay = 10;
            while (!possibleActions.NullOrEmpty())
            {
                Action action = possibleActions.RandomElement();
                queuedActions.Add(new ActionItem(action, actionDelay));
                possibleActions.Remove(action);
                actionDelay += actionInterval;
            }

            queuedActions.Add(new ActionItem(
                action: delegate
                {
                    StopGoingRogue();
                },
                tickUntilAction: duration
                ));

        }

        private void CancelLinks()
        {
            while (controlledMechs.Count > 0)
            {
                CancelControlMechanoid(controlledMechs.Last());
            }
            while (controlledTurrets.Count > 0)
            {
                CancelControlTurret(controlledTurrets.Last());
            }
            while (hackedMechs.Count > 0)
            {
                CancelHacking(hackedMechs.Last());
            }
        }

        public void StopGoingRogue()
        {
            if (!goingRogue)
            {
                return;
            }
            goingRogue = false;
            UpdateGlower();
            OverlayComp.SetLookAround();
            Traverse.Create(RefuelableComp).Field("fuel").SetValue(30f);
            this.SetFaction(Faction.OfPlayer);
            while(rogueMechs.Count > 0)
            {
                Pawn rogueMech = rogueMechs.Last();
                rogueMech.SetFaction(Faction.OfPlayer);
                rogueMechs.Remove(rogueMech);
                if (rogueMech.relations == null)
                {
                    rogueMech.relations = new Pawn_RelationsTracker(rogueMech);
                }
                if (rogueMech.story == null)
                {
                    rogueMech.story = new Pawn_StoryTracker(rogueMech);
                }
            }
            while(rogueTurrets.Count > 0)
            {
                Building_TurretGun turret = rogueTurrets.Last();
                turret.SetFaction(Faction.OfPlayer);
                rogueTurrets.Remove(turret);
            }
            queuedActions.Clear();
            PowerPlantComp.overcharging = false;
            OutputText("WTH_RogueAI_StopGoingRogue".Translate(), true);
            Messages.Message("WTH_Message_RogueAI_StopGoingRogue".Translate(), new RimWorld.Planet.GlobalTargetInfo(this.Position, this.Map), MessageTypeDefOf.PositiveEvent);

        }

        public void UpdateGlower()
        {
            CompGlower glowerComp = GetComp<CompGlower>();
            Mood mood = CurMoodCategory;
            glowerComp.Props.glowRadius = 4f;
            if (!IsConscious)
            {
                glowerComp.Props.glowRadius = 0f;
                //Don't set any color;
            }
            else if (goingRogue)
            {
                glowerComp.Props.glowColor = new ColorInt(255, 0, 0, 0);
                glowerComp.Props.glowRadius = 12f;
            }
            else if (mood == Mood.Happy)
            {
                glowerComp.Props.glowColor = new ColorInt(35, 152, 255, 0);
            }
            else if (mood == Mood.Annoyed)
            {
                glowerComp.Props.glowColor = new ColorInt(255, 119, 35, 0);
            }
            else
            {
                glowerComp.Props.glowColor = new ColorInt(255, 0, 0, 0);
            }
            glowerComp.UpdateLit(this.Map);
            Map.glowGrid.MarkGlowGridDirty(this.Position);
        }
        private void GoRogue_HackMechs()
        {
            List<Pawn> shouldHack = this.Map.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer).Where((Pawn p) => p.IsHacked() && !p.Downed).ToList();
            int nShouldHack = Rand.Range(1, 4);
            int nHacked = 0;
            while(nShouldHack > 0 && shouldHack.Count > 0)
            {
                Pawn mech = shouldHack.RandomElement();
                mech.RevertToFaction(Faction.OfMechanoids);
                mech.jobs.EndCurrentJob(JobCondition.InterruptForced);
                if (mech.GetLord() == null || mech.GetLord().LordJob == null)
                {
                    LordMaker.MakeNewLord(Faction.OfMechanoids, new LordJob_AssaultColony(Faction.OfMechanoids, true, true, false, false, true), mech.Map, new List<Pawn> { mech });
                }
                nShouldHack--;
                shouldHack.Remove(mech);
                rogueMechs.Add(mech);
                nHacked++;
            }
            if(nHacked > 0)              
                Find.LetterStack.ReceiveLetter("WTH_Message_GoRogue_HackMechs_Label".Translate(), "WTH_Message_GoRogue_HackMechs_Description".Translate(), LetterDefOf.ThreatBig, new LookTargets(rogueMechs), null, null);
        }
        private void GoRogue_HackTurrets()
        {
            Predicate<Thing> isSuitableTurret = (Thing t) => t is Building_TurretGun turret && turret.Faction == Faction.OfPlayer && Traverse.Create(turret).Field("mannableComp").GetValue<CompMannable>() == null;
            List<Building_TurretGun> turrets = this.Map.spawnedThings.Where((Thing thing) => isSuitableTurret(thing)).Cast<Building_TurretGun>().ToList();
            int nShouldHack = Rand.Range(1, 3);
            int nHacked = 0;
            while (nShouldHack > 0 && turrets.Count > 0)
            {
                Building_TurretGun turret = turrets.RandomElement();
                turret.SetFaction(Faction.OfMechanoids);
                nShouldHack--;
                turrets.Remove(turret);
                rogueTurrets.Add(turret);
                nHacked++;
            }
            if(nHacked > 0)
                Find.LetterStack.ReceiveLetter("WTH_Message_GoRogue_HackTurrets_Label".Translate(), "WTH_Message_GoRogue_HackTurrets_Description".Translate(), LetterDefOf.ThreatBig, new LookTargets(rogueTurrets.Cast<Thing>()), null, null);

        }

        private void GoRogue_CauseZzztts()
        {
            PowerPlantComp.overcharging = true;
            IEnumerable<Building> potentialTargets = ShortCircuitUtility.GetShortCircuitablePowerConduits(this.Map);
            int numZzztts = Rand.Range(3, 5);
            int rareTicksUntilAction = 0;
            for (int i = 0; i < numZzztts; i++)
            {
                if (potentialTargets.TryRandomElement(out Building potentialTarget))
                {
                    queuedActions.Add(new ActionItem(
                        action: delegate {
                            ShortCircuitUtility.DoShortCircuit(potentialTarget);
                            
                        },
                        tickUntilAction: rareTicksUntilAction)
                        );
                    rareTicksUntilAction += Rand.Range(500, 1000);
                }
            }
            queuedActions.Add(new ActionItem(
                action: delegate {
                    PowerPlantComp.overcharging = false;
                },
                tickUntilAction: rareTicksUntilAction)
                );

            Find.LetterStack.ReceiveLetter("WTH_Message_GoRogue_CauseZzztts_Label".Translate(), "WTH_Message_GoRogue_CauseZzztts_Description".Translate(), LetterDefOf.ThreatBig, new GlobalTargetInfo(this), null, null);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref textTimeout, "textTimeout");
            Scribe_Values.Look(ref isConscious, "isConscious");
            Scribe_Values.Look(ref goingRogue, "goingRogue");
            Scribe_Values.Look(ref managingPowerNetwork, "managingPowerNetwork");
            Scribe_Values.Look(ref abilityWarmUpTicks, "abilityCooldownTicks");
            Scribe_Values.Look(ref currentAbilityTicksTotal, "currentAbilityTicksTotal");
            Scribe_Collections.Look(ref controlledMechs, "controlledMechs", LookMode.Reference);
            Scribe_Collections.Look(ref hackedMechs, "hackedMechs", LookMode.Reference);
            Scribe_Collections.Look(ref controlledTurrets, "controlledTurrets", LookMode.Reference);
            Scribe_Collections.Look(ref rogueMechs, "rogueMechs", LookMode.Reference);
            Scribe_Collections.Look(ref rogueTurrets, "rogueTurrets", LookMode.Reference);
        }
    }
}
