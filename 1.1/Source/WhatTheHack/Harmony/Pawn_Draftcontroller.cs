using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Verse;
using WhatTheHack.Needs;
using WhatTheHack.Storage;

namespace WhatTheHack.Harmony
{
    //Make sure mechanoids are activated automatically when drafted. 
    [HarmonyPatch(typeof(Pawn_DraftController), "set_Drafted")]
    static class Pawn_Draftcontroller_set_Drafted
    {
        static void Postfix(Pawn_DraftController __instance)
        {
            ExtendedDataStorage store = Base.Instance.GetExtendedDataStorage();
            if (store != null)
            {
                ExtendedPawnData pawnData = store.GetExtendedDataFor(__instance.pawn);
                if (__instance.Drafted)
                {
                    pawnData.isActive = true;
                }
            }

        }
    }

    [HarmonyPatch(typeof(Pawn_DraftController), "GetGizmos")]
    static class Pawn_DraftController_GetGizmos
    {
        static void Postfix(Pawn_DraftController __instance, ref IEnumerable<Gizmo> __result)
        {
            __result = PatchGetGizmos(__instance, __result);
        }

        private static IEnumerable<Gizmo> PatchGetGizmos(Pawn_DraftController __instance, IEnumerable<Gizmo> __result)
        {
            foreach (Gizmo gizmo in __result)
            {
                if (gizmo is Command_Toggle)
                {
                    Command_Toggle toggleCommand = gizmo as Command_Toggle;
                    if (toggleCommand.defaultDesc == "CommandToggleDraftDesc".Translate())
                    {
                        DisableCommandIfMechanoidPowerLow(__instance, toggleCommand);
                        DisableCommandIfNotActivated(__instance, toggleCommand);
                        yield return toggleCommand;
                        continue;
                    }
                }
                yield return gizmo;
            }
        }

        private static void DisableCommandIfMechanoidPowerLow(Pawn_DraftController __instance, Command_Toggle toggleCommand)
        {
            ExtendedDataStorage store = Base.Instance.GetExtendedDataStorage();
            if (store != null)
            {
                if (__instance.pawn.ShouldRecharge())
                {
                    toggleCommand.Disable("WTH_Reason_PowerLow".Translate());
                }
                if (__instance.pawn.ShouldBeMaintained())
                {
                    toggleCommand.Disable("WTH_Reason_MaintenanceLow".Translate());
                }
            }
        }

        private static void DisableCommandIfNotActivated(Pawn_DraftController __instance, Command_Toggle toggleCommand)
        {
            if(toggleCommand.isActive() && !__instance.pawn.IsActivated())
            {
                __instance.Drafted = false;
            }
        }
    }

    }

