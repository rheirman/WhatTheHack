using HugsLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.Storage
{
    public class ExtendedDataStorage : UtilityWorldObject, IExposable
    {
        private Dictionary<int, ExtendedPawnData> _store =
            new Dictionary<int, ExtendedPawnData>();

        private List<int> _idWorkingList;

        private List<ExtendedPawnData> _extendedPawnDataWorkingList;

        internal int lastEmergencySignalTick = 0;
        internal int lastEmergencySignalDelay = 0;
        internal int lastEmergencySignalCooldown = 0;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(
                ref _store, "store",
                LookMode.Value, LookMode.Deep,
                ref _idWorkingList, ref _extendedPawnDataWorkingList);
            Scribe_Values.Look(ref lastEmergencySignalTick, "lastEmergencySignalTick");
            Scribe_Values.Look(ref lastEmergencySignalDelay, "lastEmergencySignalDelay");
            Scribe_Values.Look(ref lastEmergencySignalDelay, "lastEmergencySignalCooldown");
        }

        // Return the associate extended data for a given Pawn, creating a new association
        // if required.
        public ExtendedPawnData GetExtendedDataFor(Pawn pawn)
        {

            var id = pawn.thingIDNumber;
            if (_store.TryGetValue(id, out ExtendedPawnData data))
            {
                return data;
            }

            var newExtendedData = new ExtendedPawnData();

            _store[id] = newExtendedData;
            return newExtendedData;
        }

        public void DeleteExtendedDataFor(Pawn pawn)
        {
            _store.Remove(pawn.thingIDNumber);
        }
    }
}
