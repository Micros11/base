using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.TransferStatementApprovalAssignment;

namespace DirRX.HRManagement
{
  partial class TransferStatementApprovalAssignmentDepartmentPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> DepartmentFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      return query.Where(l => l.BusinessUnit.Equals(_obj.OldDepartment.BusinessUnit));
    }
  }
}