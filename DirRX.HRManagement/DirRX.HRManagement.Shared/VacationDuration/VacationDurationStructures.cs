using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.HRManagement.Structures.VacationDuration
{
  /// <summary>
  /// Доступные дни по видам отпуска.
  /// </summary>
  partial class VacationKindDuration
  {
    public HRManagement.IVacationKind VacationKind { get; set; }
    
    public int DaysCount { get; set; }
  }
}