using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.StatementDocument;

namespace DirRX.HRLite
{
  partial class StatementDocumentServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      base.BeforeSave(e);
      if (_obj.StatementDate.HasValue)
        _obj.RegistrationDate = _obj.StatementDate;      
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      base.Created(e);
      _obj.StatementDate = Calendar.Today;
    }
  }

}