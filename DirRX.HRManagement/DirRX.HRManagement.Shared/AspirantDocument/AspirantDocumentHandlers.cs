using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.AspirantDocument;
using DocKind = DirRX.HRManagement.Constants.Module.DocumentKind;

namespace DirRX.HRManagement
{
  partial class AspirantDocumentSharedHandlers
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

    public virtual void AspirantChanged(DirRX.HRManagement.Shared.AspirantDocumentAspirantChangedEventArgs e)
    {
      this.FillName();
    }

    public override void DocumentKindChanged(Sungero.Docflow.Shared.OfficialDocumentDocumentKindChangedEventArgs e)
    {
      base.DocumentKindChanged(e);
      this.FillName();
    }
  }
}