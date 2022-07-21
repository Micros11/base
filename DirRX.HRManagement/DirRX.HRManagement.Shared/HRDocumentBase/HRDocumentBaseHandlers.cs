using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.HRDocumentBase;

namespace DirRX.HRManagement
{
  partial class HRDocumentBaseSharedHandlers
  {

    public override void BusinessUnitChanged(Sungero.Docflow.Shared.OfficialDocumentBusinessUnitChangedEventArgs e)
    {
      var businessUnit = e.NewValue;
      if (businessUnit != e.OldValue && _obj.Department != null && !Equals(businessUnit, _obj.Department.BusinessUnit))
        _obj.Department = null;
    }

    public override void DepartmentChanged(Sungero.Docflow.Shared.OfficialDocumentDepartmentChangedEventArgs e)
    {
      var department = e.NewValue;
      if (department != null && department.BusinessUnit != null)
        _obj.BusinessUnit = department.BusinessUnit;
    }
  }
}