using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.VacationStatementApprovalAssignment;

namespace DirRX.HRManagement
{
  partial class VacationStatementApprovalAssignmentVacationKindPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> VacationKindFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      if (Functions.Module.IsVacationScheduleUsed(VacationApprovalTasks.As(_obj.Task).Employee))
        query = query.Where(l => l.Paid.Value == false);
      return query;
    }
  }

  partial class VacationStatementApprovalAssignmentSubstitutePropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> SubstituteFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      var task = VacationApprovalTasks.As(_obj.Task);
      return query.Where(l => !l.Equals(task.Employee));
    }
  }

}