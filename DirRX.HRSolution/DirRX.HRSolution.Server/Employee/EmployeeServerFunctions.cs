using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRSolution.Employee;

namespace DirRX.HRSolution.Server
{
  partial class EmployeeFunctions
  {

    /// <summary>
    /// заполнить свойства КЭДО.
    /// </summary>
    [Remote]
    public override void FillPersonalOfficeProperties()
    {
      base.FillPersonalOfficeProperties();
      
      if (_obj.Person != null)
      {
        var mainEmployee = EssPlatform.PublicFunctions.Module.Remote.GetPersonMainEmployee(_obj.Person.Id, _obj.BusinessUnitDirRX);
        _obj.ConsentDirRX = mainEmployee != null ? Employees.As(mainEmployee).ConsentDirRX : HRSolution.Employee.ConsentDirRX.NotSent;
      }
    }

  }
}