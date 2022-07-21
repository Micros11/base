using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Docflow;
using DirRX.HRManagement.ShiftSignDocsAssignment;

namespace DirRX.HRManagement.Client
{
  partial class ShiftSignDocsAssignmentActions
  {
    public virtual void ShowChart(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var shiftTask = VacationShiftTasks.As(_obj.Task);
      Functions.Module.VacationScheduleChart(shiftTask.Year.Value, shiftTask.Employee);
    }

    public virtual bool CanShowChart(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.BlockUid == Constants.VacationShiftTask.SignStatementAssignmentBlockId || _obj.BlockUid == Constants.VacationShiftTask.SignSeoDocAssignmentBlockId;
    }

    public virtual void ReWork(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      if (string.IsNullOrEmpty(_obj.ActiveText))
        e.AddError(VacationShiftTasks.Resources.ErrorTextReWorkNoActiveText);
    }

    public virtual bool CanReWork(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return _obj.BlockUid == Constants.VacationShiftTask.SignSeoDocAssignmentBlockId;
    }

    public virtual void Complete(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      // Не подписывать документ, если идет работа под пользователем личного кабинета.
      var isFromEssAccount = _obj.IsFromEssAccount.HasValue && _obj.IsFromEssAccount.Value;
      if (!isFromEssAccount)
      {
        // Если есть приказ, подписываем его, иначе - заявление.
        var documents = new List<IOfficialDocument>();
        var task = VacationShiftTasks.As(_obj.Task);
        var order = Functions.VacationShiftTask.Remote.GetEmployeeOrder(task);
        if (order != null)
          documents.Add(order);
        else
          documents.Add(Functions.VacationShiftTask.Remote.GetEmployeeStatement(task));

        // Выяснить, требуется ли усиленная подпись.
        var needAdvancedSign = Functions.Module.Remote.NeedAdvancedSign(Sungero.Company.Employees.As(_obj.Performer), _obj.BlockUid,
                                                                        new List<string>() { Constants.VacationShiftTask.SignSeoDocAssignmentBlockId },
                                                                        new List<string>() { Constants.VacationShiftTask.SignStatementAssignmentBlockId, Constants.VacationShiftTask.EmployeeSignOrderAssignmentBlockId });
        
        // Подписать документ.
        Functions.Module.ApproveDocument(Sungero.Workflow.Assignments.As(_obj), documents, e.Action.LocalizedName, e, needAdvancedSign);
      }
    }

    public virtual bool CanComplete(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

  }

}