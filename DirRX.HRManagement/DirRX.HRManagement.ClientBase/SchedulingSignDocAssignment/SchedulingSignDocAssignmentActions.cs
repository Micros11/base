using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.SchedulingSignDocAssignment;

namespace DirRX.HRManagement.Client
{
  partial class SchedulingSignDocAssignmentActions
  {
    public virtual void Sign(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      var documents = new List<Sungero.Docflow.IOfficialDocument>();
      var task = VacationSchedulingTasks.As(_obj.Task);
      var scheduleReport = task.ScheduleReport;
      documents.Add(scheduleReport);
      // Подписать документ
      Functions.Module.ApproveDocument(Sungero.Workflow.Assignments.As(_obj), documents, e.Action.LocalizedName, e, true);
      
      if (!scheduleReport.LastVersionApproved.Value)
        e.AddError(SchedulingSignDocAssignments.Resources.SignError);
    }

    public virtual bool CanSign(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }
  }
}