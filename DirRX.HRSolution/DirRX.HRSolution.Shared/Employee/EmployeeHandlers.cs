using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRSolution.Employee;

namespace DirRX.HRSolution
{
  partial class EmployeeSharedHandlers
  {

    public override void BusinessUnitDirRXChanged(DirRX.EssPlatformSolution.Shared.EmployeeBusinessUnitDirRXChangedEventArgs e)
    {
      base.BusinessUnitDirRXChanged(e);
      
      if (_obj.Status == Employee.Status.Active && e.NewValue != e.OldValue && _obj.Person != null)
        Functions.Employee.Remote.FillPersonalOfficeProperties(_obj);
    }
  }


}