using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.SingleEmployeeSchedulingAssignment;

namespace DirRX.HRManagement
{
  partial class SingleEmployeeSchedulingAssignmentServerHandlers
  {

    public override void BeforeComplete(Sungero.Workflow.Server.BeforeCompleteEventArgs e)
    {
      if (_obj.BlockUid == Constants.VacationSingleSchedulingTask.EmployeeSchedulingBlockId)
      {
        var errorList = Functions.SingleEmployeeSchedulingAssignment.CheckAllVacationDates(_obj);
        foreach (var error in errorList)
          e.AddError(error);
        
        if (errorList.Any())
          return;
      }
    }
  }

  partial class SingleEmployeeSchedulingAssignmentVacationsVacationKindPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> VacationsVacationKindFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      var employeeVacationKinds = _obj.SingleEmployeeSchedulingAssignment.VacationDurations.Select(l => l.VacationKind);
      return query.Where(l => employeeVacationKinds.Contains(l));
    }
  }
  
}