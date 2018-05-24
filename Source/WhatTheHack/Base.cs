using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HugsLib;
using WhatTheHack.Storage;
using HugsLib.Utils;
using HugsLib.Settings;
using Verse;

namespace WhatTheHack
{
    public class Base : ModBase
    {
        public static Base Instance { get; private set; }
        ExtendedDataStorage _extendedDataStorage;
        internal static SettingHandle<int> failureChanceNothing;
        internal static SettingHandle<int> failureChanceCauseRaid;
        internal static SettingHandle<int> failureChanceShootRandomDirection;
        internal static SettingHandle<int> failureChanceHealToStanding;
        internal static SettingHandle<int> failureChanceHackPoorly;

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
            failureChanceNothing = Settings.GetHandle<int>("failureChanceNothing", "WTH_FailureChanceNothing_Title".Translate(), "WTH_FailureChanceNothing_Description".Translate(), 40);
            failureChanceCauseRaid = Settings.GetHandle<int>("failureChanceCauseRaid", "WTH_FailureChanceCauseRaid_Title".Translate(), "WTH_FailureChanceCauseRaid_Description".Translate(), 15);
            failureChanceShootRandomDirection = Settings.GetHandle<int>("failureChanceShootRandomDirection", "WTH_FailureChanceShootRandomDirection_Title".Translate(), "WTH_FailureChanceShootRandomDirection_Description".Translate(), 20);
            failureChanceHealToStanding = Settings.GetHandle<int>("failureChanceHealToStanding", "WTH_FailureChanceHealToStanding_Title".Translate(), "WTH_FailureChanceHealToStanding_Description".Translate(), 10);
            failureChanceHackPoorly = Settings.GetHandle<int>("failureChanceHackPoorly", "WTH_FailureChanceHackPoorly_Title".Translate(), "WTH_FailureChanceHackPoorly_Description".Translate(), 15);
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
