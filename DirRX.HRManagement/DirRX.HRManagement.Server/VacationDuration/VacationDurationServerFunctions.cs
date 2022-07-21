using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.VacationDuration;

namespace DirRX.HRManagement.Server
{
  partial class VacationDurationFunctions
  {
    /// <summary>
    /// Заполнить общее количество доступных дней отпуска.
    /// </summary>
    [Remote]
    public virtual void FillTotalDaysInfo()
    {
      if (_obj.AvailableDuration.Any())
        _obj.TotalDays = _obj.AvailableDuration.Where(d => d.DaysCount.HasValue).Sum(x => x.DaysCount.Value);
      else 
        _obj.TotalDays = 0;
      _obj.TotalDaysInfo = VacationDurations.Resources.TotalDaysFormat(_obj.TotalDays);
    }
  }
}