using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.HRManagement.Structures.Vacation
{

  /// <summary>
  /// Даты начала недель.
  /// </summary>
  partial class DaysOfWeekBeginning
  {
    public DateTime BeginingDate { get; set; }
    
    public int WeekIndex { get; set; }
  }
  
  /// <summary>
  /// Даты отпуска.
  /// </summary>
  [Public]
  partial class VacationDates
  {
    public DateTime BeginDate { get; set; }
    
    public DateTime EndDate { get; set; }
    
    public int Duration { get; set; }
    
    public string Status { get; set; }
    
    public IVacationKind VacationKind { get; set; }
  }
 
  /// <summary>
  /// Количество дней в отпуске.
  /// </summary>
  [Public]
  partial class AmountDaysInVacation
  {
    public int AmountDays { get; set; }
    
    public string Holidays { get; set; }
    
    public string AmountDaysWithHolidays { get; set; }
  }
}