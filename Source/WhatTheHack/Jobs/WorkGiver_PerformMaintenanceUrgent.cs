using WhatTheHack.Needs;

namespace WhatTheHack.Jobs;

internal class WorkGiver_PerformMaintenanceUrgent : WorkGiver_PerformMaintenance
{
    protected override float GetThresHold(Need_Maintenance need)
    {
        return need.MaxLevel * need.PercentageThreshLowMaintenance;
    }
}