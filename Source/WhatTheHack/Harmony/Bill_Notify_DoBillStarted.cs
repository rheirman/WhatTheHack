using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(Bill), "Notify_DoBillStarted")]
internal class Bill_Notify_DoBillStarted
{
    private static void Prefix(Bill __instance, Pawn billDoer)
    {
        if (__instance is not Bill_Medical medicalBill)
        {
            return;
        }

        var pawn = medicalBill.GiverPawn;
        //var pawn = Traverse.Create(medicalBill).Property("GiverPawn").GetValue<Pawn>();
        if (medicalBill.recipe != WTH_DefOf.WTH_InduceEmergencySignal)
        {
            return;
        }

        var shouldCancel = false;
        if (Base.Instance.EmergencySignalRaidInbound())
        {
            Messages.Message("WTH_Message_EmergencySignalRaidInbound".Translate(),
                new GlobalTargetInfo(pawn.Position, pawn.Map), MessageTypeDefOf.RejectInput);
            shouldCancel = true;
        }
        else if (Base.Instance.EmergencySignalRaidCoolingDown())
        {
            Messages.Message("WTH_Message_EmgergencySignalRaidCoolingDown".Translate(),
                new GlobalTargetInfo(pawn.Position, pawn.Map), MessageTypeDefOf.RejectInput);
            shouldCancel = true;
        }

        if (!shouldCancel)
        {
            return;
        }

        medicalBill.billStack.Delete(medicalBill);
        billDoer.jobs.EndCurrentJob(JobCondition.Incompletable);
    }
}