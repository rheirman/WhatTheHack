using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(PawnColumnDefgenerator), "ImpliedPawnColumnDefs")]
    class PawnColumnDefGenerator_ImpliedPawnColumnDefs
    {
        static void Postfix(ref IEnumerable<PawnColumnDef> __result)
        {
            __result = ImpliedPawnColumnDefsForMechs();
        }
        static IEnumerable<PawnColumnDef> ImpliedPawnColumnDefsForMechs()
        {
            PawnTableDef workTable = WTH_DefOf.WTH_Work_Mechanoids;
            bool moveWorkTypeLabelDown = false;
            List<WorkTypeDef> allowed = new List<WorkTypeDef>();
            //TODO: Store this somewhere global.
            allowed.Add(WorkTypeDefOf.Hauling);
            allowed.Add(WorkTypeDefOf.Growing);
            allowed.Add(WTH_DefOf.Cleaning);
            allowed.Add(WTH_DefOf.PlantCutting);

            foreach (WorkTypeDef def in (from d in WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder
                                         where d.visible && allowed.Contains(d)
                                         select d).Reverse<WorkTypeDef>())
            {
                moveWorkTypeLabelDown = !moveWorkTypeLabelDown;
                PawnColumnDef d2 = new PawnColumnDef();
                d2.defName = "WorkPriority_" + def.defName;
                d2.workType = def;
                d2.moveWorkTypeLabelDown = moveWorkTypeLabelDown;
                d2.workerClass = typeof(PawnColumnWorker_WorkPriority);
                d2.sortable = true;
                d2.modContentPack = def.modContentPack;
                workTable.columns.Insert(workTable.columns.FindIndex((PawnColumnDef x) => x.Worker is PawnColumnWorker_CopyPasteWorkPriorities) + 1, d2);
                yield return d2;
            }
        }
    }
}
