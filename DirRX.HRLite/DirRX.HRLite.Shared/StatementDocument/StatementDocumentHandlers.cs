using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.StatementDocument;

namespace DirRX.HRLite
{
  partial class StatementDocumentSharedHandlers
  {

    public override void DocumentKindChanged(Sungero.Docflow.Shared.OfficialDocumentDocumentKindChangedEventArgs e)
    {
      Functions.StatementDocument.FillName(_obj);
    }

    public virtual void StatementDateChanged(Sungero.Domain.Shared.DateTimePropertyChangedEventArgs e)
    {
      if (e.NewValue.HasValue)
        Functions.StatementDocument.FillName(_obj);
    }

    public override void AuthorChanged(Sungero.Content.Shared.ElectronicDocumentAuthorChangedEventArgs e)
    {
      base.AuthorChanged(e);
      var employee = Sungero.Company.Employees.As(e.NewValue);
      if (employee != null)
      {
        _obj.Department = employee.Department;
        _obj.BusinessUnit = employee.Department.BusinessUnit;
      }
      Functions.StatementDocument.FillName(_obj);

    }

  }
}