using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using WhatTheHack.Buildings;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(Bill_Medical), "ShouldDoNow")]
    class Bill_Medical_ShouldDoNow
    {
        static void Postfix(Bill_Medical __instance, ref bool __result)
        {
            Pawn pawn = Traverse.Create(__instance).Property("GiverPawn").GetValue<Pawn>();
            if (__instance.recipe == WTH_DefOf.WTH_HackMechanoid && pawn.CurrentBed() is Building_HackingTable hackingTable && !hackingTable.HasPowerNow())
            {
                __result = false;
            }
            /*
            if (__instance.recipe == WTH_DefOf.WTH_HackMechanoid && pawn.OnHackingTable() && !((Building_HackingTable)pawn.CurrentBed()).HasPowerNow())
            {
                __result = false;
            }
            */
        }
    }
    [HarmonyPatch(typeof(Bill_Medical), "Notify_DoBillStarted")]
    class Bill_Medical_Notify_DoBillStarted
    {
        static void Prefix(Bill_Medical __instance, Pawn billDoer)
        {
            Pawn pawn = Traverse.Create(__instance).Property("GiverPawn").GetValue<Pawn>();
            if (__instance.recipe == WTH_DefOf.WTH_InduceEmergencySignal)
            {
                bool shouldCancel = false;
                if (Base.Instance.EmergencySignalRaidInbound())
                {
                    Messages.Message("WTH_Message_EmergencySignalRaidInbound".Translate(), new RimWorld.Planet.GlobalTargetInfo(pawn.Position, pawn.Map), MessageTypeDefOf.RejectInput);
                    shouldCancel = true;
                }
                else if (Base.Instance.EmergencySignalRaidCoolingDown())
                {
                    Messages.Message("WTH_Message_EmgergencySignalRaidCoolingDown".Translate(), new RimWorld.Planet.GlobalTargetInfo(pawn.Position, pawn.Map), MessageTypeDefOf.RejectInput);
                    shouldCancel = true;
                }
                if (shouldCancel)
                {
                    __instance.billStack.Delete(__instance);
                    billDoer.jobs.EndCurrentJob(Verse.AI.JobCondition.Incompletable);
                }
            }
        }
    }
}
