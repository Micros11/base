using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.HRAcquaintanceTask;

namespace DirRX.HRLite.Client
{
  partial class HRAcquaintanceTaskFunctions
  {
    /// <summary>
    /// Получить отчет "Отчет об ознакомлении".
    /// </summary>
    /// <returns>Отчет.</returns>
    public virtual Sungero.Reporting.IReport GetAcquaintanceControlReport()
    {
      var report = Reports.GetAcquaintanceControlReport();
      report.ReportSessionId = System.Guid.NewGuid().ToString();
      report.AcquaintanceTask = _obj;
      return report;
    }
  }
}