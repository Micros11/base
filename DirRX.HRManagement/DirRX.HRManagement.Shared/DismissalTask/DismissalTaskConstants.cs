using System;
using Sungero.Core;

namespace DirRX.HRManagement.Constants
{
  public static class DismissalTask
  {

    /// <summary>
    /// ИД блока согласования заявления руководителем.
    /// </summary>
    public const string ApprovalManagerAssignmentBlockId = "4";
    
    /// <summary>
    /// ИД блока на подготовку приказа об увольнении.
    /// </summary>
    public const string PrepareDocsAssignmentBlockId = "7";
    
    /// <summary>
    /// ИД блока на доработку приказа об увольнении.
    /// </summary>
    public const string ReWorkDocumentAssignmentBlockId = "11";
    
    /// <summary>
    /// ИД блока подписания документов директором по персоналу.
    /// </summary>
    public const string SignSEODocAssignmentBlockId = "9";
    
    /// <summary>
    /// ИД блока подписания заявления сотрудником.
    /// </summary>
    public const string SignEmployeeStatementAssignmentBlockId = "53";

    /// <summary>
    /// ИД блока ознакомления сотрудника с приказом.
    /// </summary>
    public const string SignEmployeeDocAssignmentBlockId = "57";
    
    /// <summary>
    /// ИД блока проверки задолженностей АХО.
    /// </summary>
    public const string AHOServiceBlockId = "39";
    
    /// <summary>
    /// ИД блока проверки задолженностей ИТ-службой.
    /// </summary>
    public const string ITServiceBlockId = "38";
    
    /// <summary>
    /// ИД блока проверки задолженностей СБ.
    /// </summary>
    public const string SBServiceBlockId = "15";
    
    /// <summary>
    /// ИД блока приема задолженностей АХО.
    /// </summary>
    public const string AHOServiceControlBlockId = "41";
    
    /// <summary>
    /// ИД блока приема задолженностей ИТ-службой.
    /// </summary>
    public const string ITServiceControlBlockId = "40";
    
    /// <summary>
    /// ИД блока приема задолженностей СБ.
    /// </summary>
    public const string SBServiceControlBlockId = "29";
    
    /// <summary>
    /// ИД блока осуществления увольнения сотрудником отдела кадров.
    /// </summary>
    public const string DismissialBlockId = "10";
    
    /// <summary>
    /// ИД блока печати документов и осуществление увольнения сотрудником отдела кадров.
    /// </summary>
    public const string PrintOrderBlockId = "66";
    
    /// <summary>
    /// ИД блока расчета сотрудника бухгалтерией.
    /// </summary>
    public const string AccounterBlockId = "22";

    /// <summary>
    /// ИД блока согласования с директором по персоналу.
    /// </summary>
    public const string ApprovalSEOAssignmentBlockId = "5";

    /// <summary>
    /// ИД блока уведомления сотруднику о задолженностях.
    /// </summary>
    public const string GoRoundNoticeBlockId = "32";
  }
}