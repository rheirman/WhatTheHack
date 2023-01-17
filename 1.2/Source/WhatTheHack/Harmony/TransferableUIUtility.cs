using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(TransferableUIUtility), "DrawExtraInfo")]
    class TransferableUIUtility_DrawExtraInfo
    {
        static void Prefix(ref List<TransferableUIUtility.ExtraInfo> info)
        {
            Color color = Color.white;

            if(Base.Instance.daysOfFuel < 1.0f)
            {
                color = Color.red;
            }

            if(Base.Instance.daysOfFuelReason != "")
            {
                info.Add(new TransferableUIUtility.ExtraInfo("WTH_ExtraInfoKey_DaysOfFuel".Translate(), Base.Instance.daysOfFuel.ToString("0.#"), Color.white, Base.Instance.daysOfFuelReason));
            }
        }
    }
}
