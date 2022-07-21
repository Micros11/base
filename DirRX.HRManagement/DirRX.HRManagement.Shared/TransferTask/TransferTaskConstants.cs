using System;
using Sungero.Core;

namespace DirRX.HRManagement.Constants
{
  public static class TransferTask
  {
    /// <summary>
    /// ИД блока доработки согласования приказа и доп. соглашения.
    /// </summary>
    public const string ReworkTransferPrepareDocsAssignmentBlockId = "32";
    
    /// <summary>
    /// ИД блока согласования заявления текущим руководителем.
    /// </summary>
    public const string ApprovalOldManagerAssignmentBlockId = "3";
    
    /// <summary>
    /// ИД блока согласования заявления новым руководителем.
    /// </summary>
    public const string ApprovalNewManagerAssignmentBlockId = "4";
    
    /// <summary>
    /// ИД блока подготовки документов.
    /// </summary>
    public const string TransferPrepareDocsAssignmentBlockId = "5";
    
    /// <summary>
    /// ИД блока подписания документов директором по персоналу.
    /// </summary>
    public const string SignManagerDocAssignmentBlockId = "7";
    
    /// <summary>
    /// ИД блока подписания документов сотрудником.
    /// </summary>
    public const string SignEmployeeDocAssignmentBlockId = "8";
    
    /// <summary>
    /// ИД блока печати доп. соглашения, заявления и бланка ознакомления с приказом.
    /// </summary>
    public const string PrintBlockId = "67";
    
    /// <summary>
    /// ИД блока печати доп. соглашения.
    /// </summary>
    public const string PrintAddAgreementBlockId = "68";
    
    /// <summary>
    /// ИД блока доработки инициатором.
    /// </summary>
    public const string InitiatorReWorkBlockId = "94";
    
    /// <summary>
    /// ИД блока согласования с сотрудником.
    /// </summary>
    public const string EmployeeStetementBlockId = "78";
    
    #region Блоки версии V1, для обеспечения совместимости
    
    /// <summary>
    /// ИД блока согласования заявления специалистом ОК.
    /// </summary>
    public const string ApprovalHiringResponsibleAssignmentBlockId = "18";
    
    /// <summary>
    /// ИД блока согласования c директором по персоналу.
    /// </summary>
    public const string SEOBlockId = "34";
    
    /// <summary>
    /// ИД блока подписания заявления сотрудником.
    /// </summary>
    public const string SignStatementAssignmentBlockId = "52";
    
    /// <summary>
    /// ИД блока осуществления перевода АХО.
    /// </summary>
    public const string AHOServiceBlockId = "21";
    
    /// <summary>
    /// ИД блока осуществления перевода ИТ-службой.
    /// </summary>
    public const string ITServiceBlockId = "23";
    
    /// <summary>
    /// ИД блока осуществления перевода СБ.
    /// </summary>
    public const string SBServiceBlockId = "22";
    
    /// <summary>
    /// ИД блока осуществления перевода Бухгалтерией.
    /// </summary>
    public const string AccountBlockId = "24";
    
    #endregion
  }
}