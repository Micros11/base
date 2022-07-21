using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.ShiftPrepareDocsAssignment;
using DocKind = DirRX.HRManagement.Constants.Module.DocumentKind;

namespace DirRX.HRManagement.Client
{
  partial class ShiftPrepareDocsAssignmentActions
  {
    public virtual void ShowChart(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var shiftTask = VacationShiftTasks.As(_obj.Task);
      Functions.Module.VacationScheduleChart(shiftTask.Year.Value, shiftTask.Employee);
    }
    
    public virtual bool CanShowChart(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.BlockUid == Constants.VacationShiftTask.ShiftPrepareDocsAssignmentBlockId || _obj.BlockUid == Constants.VacationShiftTask.ReWorkDocumentAssignmentBlockId;
    }

    public virtual void ForReapproving(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
    }

    public virtual bool CanForReapproving(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return _obj.BlockUid == Constants.VacationShiftTask.ReWorkDocumentAssignmentBlockId;
    }

    public virtual void GenerateShiftOrder(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      Functions.VacationShiftTask.Remote.ReCreateShiftVacationOrder(VacationShiftTasks.As(_obj.Task));
      Dialogs.NotifyMessage(ShiftPrepareDocsAssignments.Resources.OrderReform);
    }

    public virtual bool CanGenerateShiftOrder(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.Status == Sungero.Workflow.AssignmentBase.Status.InProcess && _obj.BlockUid == Constants.VacationShiftTask.ShiftPrepareDocsAssignmentBlockId;
    }

    public virtual void Complete(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
    }

    public virtual bool CanComplete(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return _obj.BlockUid != Constants.VacationShiftTask.ReWorkDocumentAssignmentBlockId;
    }

  }

}