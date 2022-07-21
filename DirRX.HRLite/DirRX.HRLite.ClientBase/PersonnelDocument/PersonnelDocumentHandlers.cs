using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.PersonnelDocument;

namespace DirRX.HRLite
{
  partial class PersonnelDocumentClientHandlers
  {

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      base.Refresh(e);
      _obj.State.Properties.RegistrationNumber.IsRequired = false;
      _obj.State.Properties.RegistrationDate.IsRequired = false;
    }

  }
}