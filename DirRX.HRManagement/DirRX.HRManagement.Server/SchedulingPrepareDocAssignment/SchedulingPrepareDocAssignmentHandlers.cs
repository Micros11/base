using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.SchedulingPrepareDocAssignment;

namespace DirRX.HRManagement
{
  partial class SchedulingPrepareDocAssignmentServerHandlers
  {

    public override void BeforeComplete(Sungero.Workflow.Server.BeforeCompleteEventArgs e)
    {
      if (!_obj.VacationScheduleGroup.All.Any())
        e.AddError(SchedulingPrepareDocAssignments.Resources.NeedGenerateVacationSchedule);
    }
  }

}