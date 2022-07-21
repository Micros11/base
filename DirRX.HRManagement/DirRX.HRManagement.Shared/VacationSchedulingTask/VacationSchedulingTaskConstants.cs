using System;
using Sungero.Core;

namespace DirRX.HRManagement.Constants
{
  public static class VacationSchedulingTask
  {
    /// <summary>
    /// Период мониторинга по умолчанию, в часах.
    /// </summary>
    [Public]
    public const int DefaultMonitoringPeriod = 8;
    
    /// <summary>
    /// ИД блока печати документов ответственным.
    /// </summary>
    public const string PrintDocumentsBlockId = "8";
    
    /// <summary>
    /// ИД блока конвертации документа инициатором вручную.
    /// </summary>
    public const string ConvertToPdfABlockId = "22";
  }
}