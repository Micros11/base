using System;
using Sungero.Core;

namespace DirRX.HRManagement.Constants
{
  public static class VacationShiftTask
  {
    /// <summary>
    /// ИД блока на подготовку приказа о переносе отпуска.
    /// </summary>
    public const string ShiftPrepareDocsAssignmentBlockId = "8";
    
    /// <summary>
    /// ИД блока на доработку приказа о переносе отпуска.
    /// </summary>
    public const string ReWorkDocumentAssignmentBlockId = "10";
    
    /// <summary>
    /// ИД блока подписания документов директором по персоналу.
    /// </summary>
    public const string SignSeoDocAssignmentBlockId = "9";
    
    /// <summary>
    /// ИД блока подписания заявления сотрудником.
    /// </summary>
    public const string SignStatementAssignmentBlockId = "14";
    
    /// <summary>
    /// ИД блока согласования параметров отпуска руководителем.
    /// </summary>
    public const string ManagerApprovingAssignmentBlockId = "4";
    
    /// <summary>
    /// ИД блока на доработку параметров переноса сотрудником.
    /// </summary>
    public const string EmployeeReWorkAssignmentBlockId = "22";

    /// <summary>
    /// ИД блока на доработку параметров переноса руководителем.
    /// </summary>
    public const string ManagerReWorkAssignmentBlockId = "35";
    
    /// <summary>
    /// Печать документов сотрудником отдела кадров.
    /// </summary>
    public const string PrintPaperAssignmentBlockId = "19";
    
    /// <summary>
    /// Ознакомление с приказом сотрудником.
    /// </summary>
    public const string EmployeeSignOrderAssignmentBlockId = "12";
    
  }
}