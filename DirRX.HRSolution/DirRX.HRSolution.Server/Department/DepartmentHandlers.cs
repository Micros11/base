using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRSolution.Department;

namespace DirRX.HRSolution
{
  partial class DepartmentServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      base.Created(e);
      _obj.SignKindDirRX = Department.SignKindDirRX.AsBusinessUnit;
    }
  }

}