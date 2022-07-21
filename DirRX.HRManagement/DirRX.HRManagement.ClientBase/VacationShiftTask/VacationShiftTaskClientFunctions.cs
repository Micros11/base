using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Company;
using DirRX.HRManagement.VacationShiftTask;

namespace DirRX.HRManagement.Client
{
  partial class VacationShiftTaskFunctions
  {
    /// <summary>
    /// Проверить корректность введенной даты и продолжительности. 
    /// </summary>
    /// <param name="date">Дата.</param>
    /// <param name="duration">Продолжительность.</param>
    /// <returns>Текст сообщения об ошибке.</returns>
    public string CheckInputData(DateTime? date, int? duration)
    {
      if (date.HasValue || duration.HasValue)
      {
        if (date.HasValue && date.Value <= Calendar.Today)
          return VacationShiftTasks.Resources.StartDateLessTodayError;
        else if (duration.HasValue && duration.Value <= 0)
          return VacationDepartmentSchedulingTasks.Resources.NegativeDurationError;
        else if (duration.HasValue && date.HasValue)
          return Functions.Module.Remote.CheckPossibleEndDate(_obj.Employee, date.Value, duration.Value);
      }
      return string.Empty;      
    }      
  }
}