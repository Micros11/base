using System;
using Sungero.Core;

namespace DirRX.HRManagement.Constants
{
  public static class VacationDepartmentSchedulingTask
  {
    /// <summary>
    /// ИД блока на планирование оптуска сотрудниками.
    /// </summary>
    public const int EmployeeSchedulingBlockId = 3;
    
    /// <summary>
    /// ИД блока на проверку отпусков руководителем.
    /// </summary>
    public const int ManagerCheckBlockId = 5;
    
    /// <summary>
    /// ИД блока на отправку уведомления об оптусках сотрудникам.
    /// </summary>
    public const int EmployeeNoticeBlockId = 8;
    
    /// <summary>
    /// ИД блока на отправку уведомления ответственному о завершении.
    /// </summary>
    public const int ResponsibleNoticeBlockId = 9;
  }
}