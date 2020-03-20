using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;

namespace WhatTheHack.Harmony.PrisonLabour
{
    //Compatbility patch with Prison Labor mod. It overwrites the mech renaming functionality. Instead of overwriting Prison labor back, we use the prison labor functionality for renaming mechs, which is much nicer :). 
    [HarmonyPatch]
    class PrisonLabor_Patch_RenamePrisoners
    {
        public static MethodBase TargetMethod()
        {
            Assembly assembyPL = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault((Assembly assembly) => assembly.FullName.StartsWith("PrisonLabor"));
            MethodInfo stub = typeof(PrisonLabor_Patch_RenamePrisoners).GetMethod("Stub");
            if (assembyPL == null)
            {
                return stub;
            }
            Type type = assembyPL.GetTypes().FirstOrDefault((Type t) => t.Name == "EnableRenamingPrisoners");
            //Type type = assemblyCE.GetType("JobGiver_TakeAndEquip");
            if (type == null)
            {
                return stub;
            }
            MethodInfo minfo = AccessTools.Method(type, "IsColonistOrPrisonerOfColony");
            if (minfo == null)
            {
                return stub;
            }
            return minfo;
        }
        static void Postfix(Pawn pawn, ref bool __result)
        {
            if (pawn.IsHacked() && pawn.Faction == Faction.OfPlayer)
            {
                __result = true;
            }
        }

        public static bool Stub(Pawn pawn)
        {
            return false;
            //This is patched when harmony can't find the CE method ShouldReload. 
        }
    }
}
