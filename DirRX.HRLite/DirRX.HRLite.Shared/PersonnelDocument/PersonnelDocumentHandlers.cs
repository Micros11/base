using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.PersonnelDocument;

namespace DirRX.HRLite
{
  partial class PersonnelDocumentSharedHandlers
  {
    public override void OurSignatoryChanged(Sungero.Docflow.Shared.OfficialDocumentOurSignatoryChangedEventArgs e)
    {
      base.OurSignatoryChanged(e);
      if ((e.NewValue != null) && (!Functions.PersonnelDocument.Remote.GetSignatories(_obj).Select(s => s.EmployeeId).Contains(e.NewValue.Id)))
        throw AppliedCodeException.Create(DirRX.HRLite.Resources.ErrorNotSignRightsFormat(e.NewValue.DisplayValue));
    }
  }
}