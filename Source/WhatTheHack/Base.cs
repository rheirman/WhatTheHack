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

        internal static SettingHandle<String> tabsHandler;

        internal static SettingHandle<int> failureChanceNothing;
        internal static SettingHandle<int> failureChanceCauseRaid;
        internal static SettingHandle<int> failureChanceShootRandomDirection;
        internal static SettingHandle<int> failureChanceHealToStanding;
        internal static SettingHandle<int> failureChanceHackPoorly;

        List<String> tabNames = new List<String>();

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

            foreach(RecipeDef recipe in from rd in DefDatabase<RecipeDef>.AllDefs
                                 where rd.HasModExtension<DefModExtension_Recipe>()
                                 select rd)
            {
                Log.Message("modifying success chance for surgery:" + recipe.defName);
                DefModExtension_Recipe modExtentsion = recipe.GetModExtension<DefModExtension_Recipe>();
                recipe.deathOnFailedSurgeryChance = modExtentsion.deathOnFailedSurgeryChance;
                recipe.surgerySuccessChanceFactor = modExtentsion.surgerySuccessChanceFactor;
                recipe.requireBed = modExtentsion.requireBed;
            }

            foreach (FactionDef factionDef in from td in DefDatabase<FactionDef>.AllDefs
                                          select td)
            {
                tabNames.Add(factionDef.label);
            }
            tabsHandler = Settings.GetHandle<String>("tabs", "Configure faction mech usage", "", "none");
            tabsHandler.CustomDrawer = rect => { return GUIDrawUtility.CustomDrawer_Tabs(rect, tabsHandler, tabNames.ToArray(), true, (int)-rect.width, (int)rect.height); };


            failureChanceNothing = Settings.GetHandle<int>("failureChanceNothing", "WTH_FailureChance_Nothing_Title".Translate(), "WTH_FailureChance_Nothing_Description".Translate(), 50);
            failureChanceCauseRaid = Settings.GetHandle<int>("failureChanceCauseRaid", "WTH_FailureChance_CauseRaid_Title".Translate(), "WTH_FailureChance_CauseRaid_Description".Translate(), 10);
            failureChanceShootRandomDirection = Settings.GetHandle<int>("failureChanceShootRandomDirection", "WTH_FailureChance_ShootRandomDirection_Title".Translate(), "WTH_FailureChance_ShootRandomDirection_Description".Translate(), 15);
            failureChanceHealToStanding = Settings.GetHandle<int>("failureChanceHealToStanding", "WTH_FailureChance_HealToStanding_Title".Translate(), "WTH_FailureChance_HealToStanding_Description".Translate(), 10);
            failureChanceHackPoorly = Settings.GetHandle<int>("failureChanceHackPoorly", "WTH_FailureChance_HackPoorly_Title".Translate(), "WTH_FailureChance_HackPoorly_Description".Translate(), 15);
        }
        public override void WorldLoaded()
        {
            _extendedDataStorage = UtilityWorldObjectManager.GetUtilityWorldObject<ExtendedDataStorage>();
            base.WorldLoaded();
        }
        public ExtendedDataStorage GetExtendedDataStorage()
        {
            return _extendedDataStorage;
        }

    }
}
