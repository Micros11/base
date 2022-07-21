using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.VacationKind;

namespace DirRX.HRManagement
{
  partial class VacationKindServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.Required = false;
      _obj.Paid = false;
      _obj.Sid = Guid.NewGuid().ToString();
    }
  }

}