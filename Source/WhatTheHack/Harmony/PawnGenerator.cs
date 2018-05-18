using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using WhatTheHack.Storage;

namespace WhatTheHack.Harmony
{
    /*
    //Since equipment dissapears when mechanoid is downed, we need to store it, so the mechanoid can use it when it is activated.
    [HarmonyPatch(typeof(PawnGenerator), "GeneratePawn")]
    [HarmonyPatch(new Type[]{typeof(PawnGenerationRequest)})]
    class PawnGenerator_GeneratePawn
    {
        static void Postfix(ref Pawn __result)
        {
            if (__result.RaceProps.IsMechanoid)
            {
                if(__result.equipment.Primary != null)
                {
                    __result.equipment.Primary.def.destroyOnDrop = false;
                }
                Log.Message("GeneratePawn mechanoid postfix");
                
                ExtendedDataStorage store = Base.Instance.GetExtendedDataStorage();
                if(store != null)
                {

                    ExtendedPawnData pawnData = store.GetExtendedDataFor(__result);
                    pawnData.carriedThing = __result.equipment.Primary;

                }
                
            }
        }
    }
    */


}
