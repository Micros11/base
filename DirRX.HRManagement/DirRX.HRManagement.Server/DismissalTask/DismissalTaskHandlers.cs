using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.DismissalTask;

namespace DirRX.HRManagement
{
  partial class DismissalTaskServerHandlers
  {

    public override void BeforeAbort(Sungero.Workflow.Server.BeforeAbortEventArgs e)
    {
      if (_obj.State.Properties.Status.OriginalValue == Sungero.Workflow.Task.Status.Draft)
        return;
      Functions.Module.SendAbortationNotices(_obj, DismissalTasks.Resources.AbortNoticeFormat(_obj.Employee.Name), _obj.AbortingReason, _obj.Author, _obj.ThreadSubject);
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      var employee = Sungero.Company.Employees.As(_obj.Author);
      if (employee != null)
      {
        _obj.Employee = employee;
        _obj.EmployeeAttachmentGroup.Employees.Add(employee);
      }
      _obj.DismissalDate = Calendar.Now.AddWorkingDays(10);
      _obj.ActiveText = DismissalTasks.Resources.ActiveTextOnCreate;
    }
  }

}