using System;
using Sungero.Core;

namespace DirRX.HRManagement.Constants
{
  public static class VacationApprovalTask
  {
    /// <summary>
    /// ИД блока на формирование приказа.
    /// </summary>
    public const string PrepareOrderBlockId = "7";
    
    /// <summary>
    /// ИД блока доработки согласования приказа.
    /// </summary>
    public const string ReworkPrepareDocumentAssignmentBlockId = "19";
    
    /// <summary>
    /// ИД блока подписания приказа директором по персоналу.
    /// </summary>
    public const string SignSeoDocAssignmentBlockId = "8";
    
    /// <summary>
    /// ИД блока доработки измения дат отпуска исполнителем.
    /// </summary>
    public const string InitiatorReApprovingAssignmentBlockId = "42";
    
    /// <summary>
    /// ИД блока на подписание заявления инициатором.
    /// </summary>
    public const string SignInitiatorStatementBlockId = "43";
    
    /// <summary>
    /// ИД блока ознакомления с приказом сотрудника.
    /// </summary>
    public const string SignInitiatorOrderBlockId = "9";
    
    /// <summary>
    /// ИД блока согласования с руководителем.
    /// </summary>
    public const string ApprovingBlockId = "41";
    
    /// <summary>
    /// ИД блока печати приказа сотрудником отдела кадров.
    /// </summary>
    public const string PrintDocumentBlockId = "37";
    
    /// <summary>
    /// ИД блока начисления отпускных бухгалтерией.
    /// </summary>
    public const string AccountBlockId = "11";
  }
}