using System;
using Sungero.Core;

namespace DirRX.HRManagement.Constants
{
  public static class VacationSingleSchedulingTask
  {
    /// <summary>
    /// ИД блока планирования/исправления отпуска.
    /// </summary>
    public const string EmployeeSchedulingBlockId = "3";
    
    /// <summary>
    /// ИД блока согласования с руководителем.
    /// </summary>
    public const string ManagerSchedulingBlockId = "4";
    
    /// <summary>
    /// ИД блока на подписание заявления инициатором.
    /// </summary>
    public const string SignInitiatorStatementBlockId = "5";
    
    /// <summary>
    /// ИД блока ознакомления с приказом сотрудника.
    /// </summary>
    public const string SignInitiatorOrderBlockId = "10";
    
    /// <summary>
    /// ИД блока подписания приказа директором по персоналу.
    /// </summary>
    public const string SignSeoDocAssignmentBlockId = "7";
    
    /// <summary>
    /// ИД блока на формирование приказа.
    /// </summary>
    public const string PrepareOrderBlockId = "6";
    
    /// <summary>
    /// ИД блока доработки согласования приказа.
    /// </summary>
    public const string ReworkPrepareDocumentAssignmentBlockId = "8";
    
    /// <summary>
    /// Печать приказа сотрудником отдела кадров.
    /// </summary>
    public const string PrintPaperAssignmentBlockId = "16";
  }
}