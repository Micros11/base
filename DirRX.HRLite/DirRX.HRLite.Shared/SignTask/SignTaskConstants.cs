using System;
using Sungero.Core;

namespace DirRX.HRLite.Constants
{
  public static class SignTask
  {
    /// <summary>
    /// ИД блока "Конвертация документа инициатором".
    /// </summary>
    public const string ConvertToPdfaByInitiatorBlockId = "23";
    
    /// <summary>
    /// Период мониторинга по умолчанию, в минутах.
    /// </summary>
    [Public]
    public const int DefaultMonitoringPeriod = 10;
  }
}