using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.BilaterialDocument;

namespace DirRX.HRLite
{
  partial class BilaterialDocumentSharedHandlers
  {
    public override void OurSignatoryChanged(Sungero.Docflow.Shared.OfficialDocumentOurSignatoryChangedEventArgs e)
    {
      base.OurSignatoryChanged(e);
      if ((e.NewValue != null) && (!Functions.BilaterialDocument.Remote.GetSignatories(_obj).Select(s => s.EmployeeId).Contains(e.NewValue.Id)))
        throw AppliedCodeException.Create(DirRX.HRLite.Resources.ErrorNotSignRightsFormat(e.NewValue.DisplayValue));
    }
  }

  partial class BilaterialDocumentEmployeesSharedHandlers
  {
    public virtual void EmployeesEmployeeChanged(DirRX.HRLite.Shared.BilaterialDocumentEmployeesEmployeeChangedEventArgs e)
    {
      Functions.BilaterialDocument.FillName(_obj.BilaterialDocument);
    }
  }

  partial class BilaterialDocumentEmployeesSharedCollectionHandlers
  {
    public virtual void EmployeesDeleted(Sungero.Domain.Shared.CollectionPropertyDeletedEventArgs e)
    {
      Functions.BilaterialDocument.FillName(_obj);
    }
  }
}