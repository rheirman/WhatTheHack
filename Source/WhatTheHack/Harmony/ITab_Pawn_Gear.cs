using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(ITab_Pawn_Gear), "InterfaceDrop")]
    class ITab_Pawn_Gear_InterfaceDrop
    {
        static bool Prefix(ITab_Pawn_Gear __instance, Thing t)
        {
            Log.Message("calling InterfaceDrop");
            Pawn pawn = Traverse.Create(__instance).Property("SelPawnForGear").GetValue<Pawn>();
            if(pawn != null && pawn.IsHacked())
            {
                Log.Message("forbidding drop");
                Messages.Message("WTH_Message_CannotDrop".Translate(), MessageTypeDefOf.RejectInput);
                return false;
            }
            return true;
        }
    }
}
