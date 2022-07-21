using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.VacationEmployeeSchedulingAssignment;

namespace DirRX.HRManagement
{
  partial class VacationEmployeeSchedulingAssignmentVacationsVacationKindPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> VacationsVacationKindFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      var employeeVacationKinds = _obj.VacationEmployeeSchedulingAssignment.VacationDurations.Select(l => l.VacationKind);
      return query.Where(l => employeeVacationKinds.Contains(l));
    }
  }

  partial class VacationEmployeeSchedulingAssignmentServerHandlers
  {

    public override void BeforeComplete(Sungero.Workflow.Server.BeforeCompleteEventArgs e)
    {     
      var isFromEssAccount = _obj.IsFromEssAccount.HasValue && _obj.IsFromEssAccount.Value;
      if (isFromEssAccount || !Functions.Module.IsServiceUser(Users.Current))
      {
        var errorList = Functions.VacationEmployeeSchedulingAssignment.CheckAllVacationDates(_obj);
        foreach (var error in errorList)
          e.AddError(error);
        
        if (errorList.Any())
          return;
      }
    }
  }

}