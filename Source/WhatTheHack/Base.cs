using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HugsLib;
using WhatTheHack.Storage;
using HugsLib.Utils;
using HugsLib.Settings;
using Verse;
using RimWorld;


namespace WhatTheHack
{
    public class Base : ModBase
    {
        public static Base Instance { get; private set; }
        ExtendedDataStorage _extendedDataStorage;


        //settings
        internal static SettingHandle<String> tabsHandler;
        internal static SettingHandle<int> failureChanceNothing;
        internal static SettingHandle<int> failureChanceCauseRaid;
        internal static SettingHandle<int> failureChanceShootRandomDirection;
        internal static SettingHandle<int> failureChanceHealToStanding;
        internal static SettingHandle<int> failureChanceHackPoorly;
        internal static SettingHandle<Dict2DRecordHandler> factionRestrictions;

        //temp accessible storage
        internal float daysOfFuel = 0;
        internal string daysOfFuelReason = "";

        //List<String> tabNames = new List<String>();

        public override string ModIdentifier
        {
            get { return "WhatTheHack"; }
        }
        public Base()
        {
            Instance = this;
        }
        public override void DefsLoaded()
        {
            base.DefsLoaded();
            Predicate<ThingDef> isMech = (ThingDef d) => d.race != null && d.race.IsMechanoid;
            Predicate<FactionDef> isHackingFaction = (FactionDef d) => !d.isPlayer && d != FactionDefOf.Mechanoid && d != FactionDefOf.Insect;
            List<ThingDef> allMechs = (from td in DefDatabase<ThingDef>.AllDefs where isMech(td) select td).ToList();
            List<string> allFactionNames = (from td
                                            in DefDatabase<FactionDef>.AllDefs
                                            where isHackingFaction(td)
                                            select td.defName).ToList();

            tabsHandler = Settings.GetHandle<String>("tabs", "WTH_FactionRestrictions_Label".Translate(), "WTH_FactionRestrictions_Description".Translate(), allFactionNames.First());
            tabsHandler.CustomDrawer = rect => { return GUIDrawUtility.CustomDrawer_Tabs(rect, tabsHandler, allFactionNames.ToArray(), true, (int)-rect.width, (int)rect.height + 5); };


            factionRestrictions = Settings.GetHandle<Dict2DRecordHandler>("factionRestrictions", "", "", null);
            factionRestrictions.CustomDrawer = rect => { return GUIDrawUtility.CustomDrawer_MatchingPawns_active(rect, factionRestrictions, allMechs, tabsHandler, allFactionNames.Count, "WTH_FactionRestrictions_OK".Translate(), "WTH_FactionRestrictions_NOK".Translate()); };

            failureChanceNothing = Settings.GetHandle<int>("failureChanceNothing", "WTH_FailureChance_Nothing_Title".Translate(), "WTH_FailureChance_Nothing_Description".Translate(), 70);
            failureChanceCauseRaid = Settings.GetHandle<int>("failureChanceCauseRaid", "WTH_FailureChance_CauseRaid_Title".Translate(), "WTH_FailureChance_CauseRaid_Description".Translate(), 5);
            failureChanceShootRandomDirection = Settings.GetHandle<int>("failureChanceShootRandomDirection", "WTH_FailureChance_ShootRandomDirection_Title".Translate(), "WTH_FailureChance_ShootRandomDirection_Description".Translate(), 10);
            failureChanceHealToStanding = Settings.GetHandle<int>("failureChanceHealToStanding", "WTH_FailureChance_HealToStanding_Title".Translate(), "WTH_FailureChance_HealToStanding_Description".Translate(), 5);
            failureChanceHackPoorly = Settings.GetHandle<int>("failureChanceHackPoorly", "WTH_FailureChance_HackPoorly_Title".Translate(), "WTH_FailureChance_HackPoorly_Description".Translate(), 10);
            factionRestrictions = GetDefaultForFactionRestrictions(factionRestrictions, allMechs, allFactionNames);
        }

        private static SettingHandle<Dict2DRecordHandler> GetDefaultForFactionRestrictions(SettingHandle<Dict2DRecordHandler> factionRestrictions, List<ThingDef> allMechs, List<string> allFactionNames)
        {
            if (factionRestrictions.Value == null)
            {
                factionRestrictions.Value = new Dict2DRecordHandler();
            }

            if (factionRestrictions.Value.InnerList == null)
            {
                factionRestrictions.Value.InnerList = new Dictionary<String, Dictionary<String, Record>>();    
            }
            foreach (FactionDef factionDef in from td in DefDatabase<FactionDef>.AllDefs
                                              where allFactionNames.Contains(td.defName)
                                              select td)
            {
                if (!factionRestrictions.Value.InnerList.ContainsKey(factionDef.defName))
                {
                    factionRestrictions.Value.InnerList.Add(factionDef.defName, new Dictionary<string, Record>());
                }
            }
            foreach (string name in allFactionNames)
            {
                Dictionary<string, Record> selection = factionRestrictions.Value.InnerList[name];
                GUIDrawUtility.FilterSelection(ref selection, allMechs, name);
                factionRestrictions.Value.InnerList[name] = selection;
            }
            return factionRestrictions;
        }

        public override void WorldLoaded()
        {
            _extendedDataStorage = UtilityWorldObjectManager.GetUtilityWorldObject<ExtendedDataStorage>();
            base.WorldLoaded();
            foreach (Map map in Find.Maps)
            {
                foreach (Pawn pawn in map.mapPawns.AllPawnsSpawned.Where((Pawn p) => p.health != null && p.health.hediffSet.HasHediff(WTH_DefOf.WTH_RepairModule)))
                {
                    Log.Message("initilializing comps for " + pawn.def);
                    pawn.InitializeComps();
                }
            }

            foreach (RecipeDef recipe in from rd in DefDatabase<RecipeDef>.AllDefs
                                         where rd.HasModExtension<DefModExtension_Recipe>()
                                         select rd)
            {
                DefModExtension_Recipe modExtentsion = recipe.GetModExtension<DefModExtension_Recipe>();
                recipe.deathOnFailedSurgeryChance = modExtentsion.deathOnFailedSurgeryChance;
                recipe.surgerySuccessChanceFactor = modExtentsion.surgerySuccessChanceFactor;
            }

        }
        public ExtendedDataStorage GetExtendedDataStorage()
        {
            return _extendedDataStorage;
        }

    }
}
