using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using WhatTheHack.Buildings;
using WhatTheHack.Storage;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(StatWorker), "ShouldShowFor")]
    static class StatWorker_ShouldShowFor
    {
        static bool Prefix(StatWorker __instance, StatRequest req, ref bool __result)
        {
            StatDef stat = Traverse.Create(__instance).Field("stat").GetValue<StatDef>();
            if (stat.category == WTH_DefOf.WTH_StatCategory_HackedMechanoid && req.Thing is Pawn pawn)
            {
                __result = pawn.IsHacked();
                return false;
            }
            if(stat.category == WTH_DefOf.WTH_StatCategory_Colonist && req.Thing is Pawn pawn2)
            {
                __result = pawn2.IsColonistPlayerControlled;
                return false;
            }
            if(stat.category == WTH_DefOf.WTH_StatCategory_Platform && req.Thing is Building_BaseMechanoidPlatform)
            {
                __result = true;
                return false;
            }
            if(stat.category == WTH_DefOf.WTH_StatCategory_HackedMechanoid || stat.category == WTH_DefOf.WTH_StatCategory_Colonist || stat.category == WTH_DefOf.WTH_StatCategory_Platform)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
        
    [HarmonyPatch(typeof(StatWorker), "IsDisabledFor")]
    static class StatWorker_IsDisabledFor
    {
        static bool Prefix(Thing thing, ref bool __result)
        {

            if(thing is Pawn && ((Pawn)thing).RaceProps.IsMechanoid)
            {
                Pawn pawn = (Pawn)thing;
                if (pawn.IsHacked())
                {
                    __result = false;
                    return false;
                }
            }
            return true;
        }

    }
        

}
