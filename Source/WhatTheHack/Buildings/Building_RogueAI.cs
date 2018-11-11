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
using WhatTheHack.Comps;
using WhatTheHack.Duties;
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

        private const int MAXCONTROLLABLEMECHS = 6;
        private const int MAXCONTROLLABLETURRETS = 4;
        private const int MAXHACKABLE = 2;
        private const int NUMTEXTSHAPPY = 50;
        private const int NUMTEXTSANNOYED = 50;
        private const int NUMTEXTSMAD = 15;
        private const int MINLEVELMANAGEPOWER = 2;
        private const int MINLEVELCONTROLTURRET = 3;
        private const int MINLEVELCONTROLMECH = 4;
        private const int MINLEVELHACKMECH = 5;
        private const int MINTEXTTIMEOUT = 6;
        private const float TEXTDURATION = 4f;


        private float moodDrainCtrlMech = 0.1f;
        private float moodDrainCtrlTur = 0.1f;
        private float moodDrainHack = 0.4f;
        private float moodDrainNoPower = 0.2f;
        private float moodDrainDamage = 2.0f;
        private float moodDrainForceTalkGibberish = 5.0f;
        public float moodDrainPreventZzztt = 0.5f;
        private int abilityWarmUpTicks = 0;
        private int currentAbilityTicksTotal = 0;

        private int textTimeout = 0;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            UpdateGlower(CurMoodCategory);
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
        private CompPowerPlant_RogueAI PowerPlantComp
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
                if (RefuelableComp.FuelPercentOfMax < 0.2f)
                {
                    return Mood.Mad;
                }
                else if (RefuelableComp.FuelPercentOfMax < 0.5f)
                {
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
        }

        private void MaybeTalkGibberish()
        {
            float textChance = goingRogue ? 1.0f : 0.02f;
            if (Rand.Chance(textChance) && textTimeout <= 0)
            {
                TalkGibberish();
            }
        }

        private void TalkGibberish()
        {
            if (IsConscious)
            {
                string randomColonistName = this.Map.mapPawns.FreeColonists.RandomElement().Name.ToStringShort;
                string text = "";
                Color color = Color.white;
                if (CurMoodCategory == Mood.Happy)
                {
                    text = "WTH_RogueAI_Happy_Remark_" + Rand.RangeInclusive(0, NUMTEXTSHAPPY - 1);
                }
                else if (CurMoodCategory == Mood.Annoyed)
                {
                    text = "WTH_RogueAI_Annoyed_Remark_" + Rand.RangeInclusive(0, NUMTEXTSANNOYED - 1);
                }
                else
                {
                    color = Color.red;
                    text = "WTH_RogueAI_Mad_Remark_" + +Rand.RangeInclusive(0, NUMTEXTSMAD - 1);
                }
                Utilities.ThrowStaticText(this.DrawPos + new Vector3(0, 0, 1.75f), this.Map, text.Translate(new object[] { randomColonistName }), color, TEXTDURATION);
                textTimeout += MINTEXTTIMEOUT;
            }
        }

        private void MaybeGoRogue()
        {
            //change to go rogue at tick rare gets increasingly large, starting at 1/50; 
            float goRogueChance = 1 / (RefuelableComp.Fuel * 2.5f);
            if (Rand.Chance(goRogueChance))
            {
                GoRogue();
            }
        }
        public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostApplyDamage(dinfo, totalDamageDealt);
            DrainMood(moodDrainDamage);
            if(textTimeout <= 0)
            {
                Utilities.ThrowStaticText(this.DrawPos + new Vector3(0, 0, 1.75f), this.Map, "WTH_RogueAI_Hurt_Remark".Translate(), Color.white, TEXTDURATION);
                textTimeout += MINTEXTTIMEOUT;
            }
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
                if (textTimeout <= 0)
                {
                    Utilities.ThrowStaticText(this.DrawPos + new Vector3(0, 0, 1.75f), this.Map, "WTH_RogueAI_NoPower_Remark".Translate(), Color.white, TEXTDURATION);
                    textTimeout += MINTEXTTIMEOUT;
                }
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
                UpdateGlower(CurMoodCategory);
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
            foreach(ActionItem actionItem in queuedActions)
            {
                actionItem.Tick();
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
            command.defaultLabel = "WTH_Gizmo_TalkGibberish_Label".Translate();//TODO
            command.defaultDesc = "WTH_Gizmo_TalkGibberish_Description".Translate(moodDrainForceTalkGibberish);//TODO
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
            command.defaultLabel = "WTH_Gizmo_ManagePowerNetwork_Label".Translate();//TODO
            command.defaultDesc = "WTH_Gizmo_ManagePowerNetwork_Description".Translate();//TODO
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
            command.defaultDesc = "WTH_Gizmo_ControlMechanoidActivate_Description".Translate(MAXCONTROLLABLEMECHS);//TODO
            command.targetingParams = GetTargetingParametersForControlling();
            command.hotKey = KeyBindingDefOf.Misc5;
            command.icon = ContentFinder<Texture2D>.Get(("UI/RogueAI_Control"));//TODO

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
                    DoAbility(delegate { ControlMechanoid(mech); }, 500);        
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
            command.defaultDesc = "WTH_Gizmo_HackingAvtivate_Description".Translate(MAXHACKABLE);//TODO
            command.targetingParams = GetTargetingParametersForHacking();
            command.hotKey = KeyBindingDefOf.Misc5;
            command.icon = ContentFinder<Texture2D>.Get(("UI/RogueAI_Hack"));//TODO 
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
                    DoAbility(delegate { HackMech(mech); }, 1000);
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
            command.defaultDesc = "WTH_Gizmo_ControlTurretActivate_Description".Translate(MAXCONTROLLABLETURRETS);//TODO
            command.targetingParams = GetTargetingParametersForControlTurret();
            command.icon = ContentFinder<Texture2D>.Get(("UI/RogueAI_Control_Turret"));//TODO
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
                    DoAbility(delegate { controlledTurrets.Add(turret); }, 500);
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
            OverlayComp.UnsetLookAround();
            goingRogue = true;
            this.SetFaction(Faction.OfMechanoids);
            UpdateGlower(Mood.Mad);
            CancelLinks();

            List<Pawn> shouldHack = this.Map.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer).Where((Pawn p) => p.IsHacked() && !p.Downed).ToList();
            GoRogue_HackMechs(shouldHack);
            List<Building_TurretGun> shouldHackTurrets = this.Map.spawnedThings.Where((Thing t) => t is Building_TurretGun && t.Faction == Faction.OfPlayer).Cast<Building_TurretGun>().ToList();
            GoRogue_HackTurrets(shouldHackTurrets);
            GoRogue_CauseZzztts();
            queuedActions.Add(new ActionItem(
                action: delegate
                {
                    StopGoingRogue();
                    Log.Message("stop going rogue called");
                },
                tickUntilAction: Rand.Range(7500, 10000)
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
            UpdateGlower(Mood.Annoyed);
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
        }

        private void UpdateGlower(Mood mood)
        {
            CompGlower glowerComp = GetComp<CompGlower>();
            if (mood == Mood.Happy)
            {
                glowerComp.Props.glowRadius = 4f;
                glowerComp.Props.glowColor = new ColorInt(35, 152, 255, 0);
            }
            else if (mood == Mood.Annoyed)
            {
                glowerComp.Props.glowRadius = 4f;
                glowerComp.Props.glowColor = new ColorInt(255, 119, 35, 0);
            }
            else
            {
                glowerComp.Props.glowRadius = 4f;
                glowerComp.Props.glowColor = new ColorInt(255, 0, 0, 0);
            }
            if (goingRogue)
            {
                glowerComp.Props.glowRadius = 12f;
            }
            else
            {
                glowerComp.Props.glowRadius = 4f;
            }
            glowerComp.UpdateLit(this.Map);
            Map.glowGrid.MarkGlowGridDirty(this.Position);
        }
        private void GoRogue_HackMechs(List<Pawn> shouldHack)
        {
            int nShouldHack = Rand.Range(1, 4);
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
        }

        public override void ExposeData()
        {
            base.ExposeData();
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
