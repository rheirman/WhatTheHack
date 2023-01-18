using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(PawnColumnDefgenerator), "ImpliedPawnColumnDefs")]
internal class PawnColumnDefGenerator_ImpliedPawnColumnDefs
{
    private static void Postfix(ref IEnumerable<PawnColumnDef> __result)
    {
        __result = ImpliedPawnColumnDefsForMechs();
    }

    private static IEnumerable<PawnColumnDef> ImpliedPawnColumnDefsForMechs()
    {
        var workTable = WTH_DefOf.WTH_Work_Mechanoids;
        var moveWorkTypeLabelDown = false;
        var allowed = new List<WorkTypeDef>
        {
            //TODO: Store this somewhere global.
            WorkTypeDefOf.Hauling,
            WorkTypeDefOf.Growing,
            WTH_DefOf.Cleaning,
            WTH_DefOf.PlantCutting
        };

        foreach (var def in (from d in WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder
                     where d.visible && allowed.Contains(d)
                     select d).Reverse())
        {
            moveWorkTypeLabelDown = !moveWorkTypeLabelDown;
            var d2 = new PawnColumnDef
            {
                defName = $"WorkPriority_{def.defName}",
                workType = def,
                moveWorkTypeLabelDown = moveWorkTypeLabelDown,
                workerClass = typeof(PawnColumnWorker_WorkPriority),
                sortable = true,
                modContentPack = def.modContentPack
            };
            workTable.columns.Insert(
                workTable.columns.FindIndex(x => x.Worker is PawnColumnWorker_CopyPasteWorkPriorities) + 1, d2);
            yield return d2;
        }
    }
}