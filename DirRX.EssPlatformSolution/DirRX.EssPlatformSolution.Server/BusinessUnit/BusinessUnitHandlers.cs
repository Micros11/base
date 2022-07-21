using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.EssPlatformSolution.BusinessUnit;

namespace DirRX.EssPlatformSolution
{
  partial class BusinessUnitServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      base.Created(e);
      _obj.UseESSDirRX = false;
    }
  }

}