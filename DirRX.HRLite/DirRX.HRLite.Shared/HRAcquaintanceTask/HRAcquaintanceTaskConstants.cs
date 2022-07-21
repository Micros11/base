using System;
using Sungero.Core;

namespace DirRX.HRLite.Constants
{
  public static class HRAcquaintanceTask
  {

    /// <summary>
    /// Константы ширины полей в листе ознакомления.
    /// </summary>
    public const double NumberWidth = 20;
    public const double NameWidth = 150;
    public const double JobTitleWidth = 150;
    public const double StatusWidth = 100;
    public const double DateWidth = 100;
    public const double CommentaryWidth = 130;
    
    /// <summary>
    /// ИД блока "Конвертация документа инициатором".
    /// </summary>
    public const string ConvertToPdfaByInitiatorBlockId = "62";
    
    /// <summary>
    /// Период мониторинга по умолчанию, в минутах.
    /// </summary>
    [Public]
    public const int DefaultMonitoringPeriod = 10;
  }
}