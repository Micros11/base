using System;
using Sungero.Core;

namespace DirRX.HRManagement.Constants
{
  public static class VacationAlertTask
  {
    /// <summary>
    /// ИД блока подтверждения дат отпуска сотрудником.
    /// </summary>
    public const string EmployeeConfirmationBlockId = "4";
    
    /// <summary>
    /// ИД блока распечатки и подписания сотрудником.
    /// </summary>
    public const string EmployeePrintBlockId = "31";
    
    /// <summary>
    /// ИД блока начисления отпускных бухгалтерией.
    /// </summary>
    public const string AccountBlockId = "5";
    
    /// <summary>
    /// ИД блока подтверждения ответственным по отпускам.
    /// </summary>
    public const string ResponsibleConfirmationBlockId = "27";
    
    /// <summary>
    /// ИД блока уведомления руководителю о замещении сотрудника.
    /// </summary>
    public const string ManagerSubstitutionNoticeBlockId = "11";
    
    /// <summary>
    /// Период мониторинга по умолчанию, в часах.
    /// </summary>
    [Public]
    public const int DefaultMonitoringPeriod = 8;
  }
}