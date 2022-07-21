using System;
using Sungero.Core;

namespace DirRX.HRManagement.Constants
{
  public static class HiringTask
  {
    /// <summary>
    /// ИД блока доработки приказа или трудового договора.
    /// </summary>
    public const string ReworkPersonnelDepBlockId = "69";
    
    /// <summary>
    /// ИД блока на подготовку документов.
    /// </summary>
    public const string TransferPrepareDocsAssignmentBlockId = "48";
    
    /// <summary>
    /// ИД блока подписания документов директором по персоналу.
    /// </summary>
    public const string SignDocBySeoBlockId = "28";

    /// <summary>
    /// ИД блока подписания документов сотрудником.
    /// </summary>
    public const string SignDocByEmployeeBlockId = "27";
    
    /// <summary>
    /// ИД блока подготовки приема АХО.
    /// </summary>
    public const string AHOServiceBlockId = "56";
    
    /// <summary>
    /// ИД блока подготовки приема ИТ-службой.
    /// </summary>
    public const string ITServiceBlockId = "54";
    
    /// <summary>
    /// ИД блока подготовки приема СБ.
    /// </summary>
    public const string SBServiceBlockId = "4";
    
    /// <summary>
    /// ИД блока организации встречи нового сотрудника руководителем.
    /// </summary>
    public const string ManagerMeetBlockId = "19";
    
    /// <summary>
    /// ИД блока печати ТД и бланка ознакомления с приказом сотрудником отдела кадров.
    /// </summary>
    public const string PrintDocsBlockId = "74";
    
    /// <summary>
    /// ИД блока печати ТД сотрудником отдела кадров.
    /// </summary>
    public const string PrintContractDocsBlockId = "76";
  }
}