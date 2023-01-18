using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace WhatTheHack.Harmony.PrisonLabour;

//Compatbility patch with Prison Labor mod. It overwrites the mech renaming functionality. Instead of overwriting Prison labor back, we use the prison labor functionality for renaming mechs, which is much nicer :). 
[HarmonyPatch]
internal class PrisonLabor_Patch_RenamePrisoners
{
    public static MethodBase TargetMethod()
    {
        var assembyPL = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(assembly => assembly.FullName.StartsWith("PrisonLabor"));
        var stub = typeof(PrisonLabor_Patch_RenamePrisoners).GetMethod("Stub");
        if (assembyPL == null)
        {
            return stub;
        }

        var type = assembyPL.GetTypes().FirstOrDefault(t => t.Name == "EnableRenamingPrisoners");
        //Type type = assemblyCE.GetType("JobGiver_TakeAndEquip");
        if (type == null)
        {
            return stub;
        }

        var minfo = AccessTools.Method(type, "IsColonistOrPrisonerOfColony");
        if (minfo == null)
        {
            return stub;
        }

        return minfo;
    }

    private static void Postfix(Pawn pawn, ref bool __result)
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