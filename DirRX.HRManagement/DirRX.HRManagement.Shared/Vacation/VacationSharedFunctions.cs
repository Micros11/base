using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.Vacation;

namespace DirRX.HRManagement.Shared
{
  partial class VacationFunctions
  {

    /// <summary>
    /// Сформировать имя.
    /// </summary>
    public void FormatName()
    {
      _obj.Name = Vacations.Resources.NamePatternFormat(_obj.Employee != null ? _obj.Employee.Person.ShortName : string.Empty,
                                                        _obj.StartDate.HasValue ? _obj.StartDate.Value.ToString("d") : string.Empty,
                                                        (_obj.FinDate.HasValue && !Equals(_obj.StartDate, _obj.FinDate)) ? " - " + _obj.FinDate.Value.ToString("d") : string.Empty,
                                                        _obj.VacationDuration.HasValue ? _obj.VacationDuration.Value.ToString() : string.Empty,
                                                        _obj.VacationKind != null ? _obj.VacationKind.ShortName : string.Empty);
    }
  }
}