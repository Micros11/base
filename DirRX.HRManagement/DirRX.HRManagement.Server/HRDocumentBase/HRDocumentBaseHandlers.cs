using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.HRDocumentBase;

namespace DirRX.HRManagement
{
  partial class HRDocumentBaseDepartmentPropertyFilteringServerHandler<T>
  {

    public override IQueryable<T> DepartmentFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      query = query.Where(d => d.Status == Sungero.CoreEntities.DatabookEntry.Status.Active);           
      if (_obj.BusinessUnit != null)
        query = query.Where(d => Equals(d.BusinessUnit, _obj.BusinessUnit));
      return query;
    }
  }
}