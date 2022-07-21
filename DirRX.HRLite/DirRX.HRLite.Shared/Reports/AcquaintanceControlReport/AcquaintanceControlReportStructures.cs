using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.HRLite.Structures.AcquaintanceControlReport
{
  // Структура для пакетной записи данных (одной строки) по отчету во временную таблицу.
  partial class TableLine
  {
    public string Department { get; set; }
    
    public string Performer { get; set; }
    
    public string JobTitle { get; set; }
    
    public string Result { get; set; }
    
    public int ResultOrder { get; set; }
    
    public string ReportSessionId { get; set; }
  }
}