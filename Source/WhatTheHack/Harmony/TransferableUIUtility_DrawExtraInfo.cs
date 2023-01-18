using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(TransferableUIUtility), "DrawExtraInfo")]
internal class TransferableUIUtility_DrawExtraInfo
{
    private static void Prefix(ref List<TransferableUIUtility.ExtraInfo> info)
    {
        var color = Color.white;

        if (Base.Instance.daysOfFuel < 1.0f)
        {
            color = Color.red;
        }

        if (Base.Instance.daysOfFuelReason != "")
        {
            info.Add(new TransferableUIUtility.ExtraInfo("WTH_ExtraInfoKey_DaysOfFuel".Translate(),
                Base.Instance.daysOfFuel.ToString("0.#"), color, Base.Instance.daysOfFuelReason));
        }
    }
}