using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.LNALists;

namespace DirRX.HRManagement
{
  partial class LNAListsDepartmentsSharedHandlers
  {

    public virtual void DepartmentsDepartmentChanged(DirRX.HRManagement.Shared.LNAListsDepartmentsDepartmentChangedEventArgs e)
    {
      var rec = LNAListses.As(_obj.RootEntity);
      if (rec.BusinessUnit == null)
        rec.BusinessUnit = e.NewValue.BusinessUnit;
    }
  }

  partial class LNAListsSharedHandlers
  {

    public virtual void BusinessUnitChanged(DirRX.HRManagement.Shared.LNAListsBusinessUnitChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue && e.OldValue != null)
        _obj.Departments.Clear();
    }
  }
}