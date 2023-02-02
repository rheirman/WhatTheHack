using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WhatTheHack.Needs;

namespace WhatTheHack.Jobs
{
    class WorkGiver_PerformMaintenanceUrgent : WorkGiver_PerformMaintenance
    {
        protected override float GetThresHold(Need_Maintenance need)
        {
            return need.MaxLevel * need.PercentageThreshLowMaintenance;
        }
    }
}
