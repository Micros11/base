using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.TransferTask;
using DocKind = DirRX.HRManagement.Constants.Module.DocumentKind;

namespace DirRX.HRManagement.Server
{
  partial class TransferTaskFunctions
  {

    /// <summary>
    /// Перенести данные об изменениях условий труда из задачи в документы.
    /// </summary>
    public void FillTransferDataToDocs()
    {
      if (_obj.Order != null)
      {
        var order = HROrders.As(_obj.Order);
        if (order.EventDate != _obj.TransferDate)
          order.EventDate = _obj.TransferDate;
        if (order.NewDepartment != _obj.Department)
          order.NewDepartment = _obj.Department;
        if (order.NewJobTitle != _obj.JobTitle)
          order.NewJobTitle = _obj.JobTitle;
        if (order.State.IsChanged)
          order.Save();
      }
      if (_obj.AdditionalAgreement != null)
      {
        var additionalAgreement = HRDocuments.As(_obj.AdditionalAgreement);
        if (additionalAgreement.TransferDate != _obj.TransferDate)
          additionalAgreement.TransferDate = _obj.TransferDate;
        if (additionalAgreement.NewDepartment != _obj.Department)
          additionalAgreement.NewDepartment = _obj.Department;
        if (additionalAgreement.NewJobTitle != _obj.JobTitle)
          additionalAgreement.NewJobTitle = _obj.JobTitle;
        if (additionalAgreement.State.IsChanged)
          additionalAgreement.Save();
      }
    }
    
    /// <summary>
    /// Построить модель состояния инструкции.
    /// </summary>
    /// <returns>Модель состояния.</returns>
    [Remote(IsPure = true)]
    public StateView GetTransferTaskState()
    {
      var instruction = string.Empty;
      if (_obj.IsHRStarted.Value)
        instruction = TransferTasks.Resources.HRTaskInstruction;
      else if (_obj.IsManagerStarted.Value)
        instruction = TransferTasks.Resources.ManagerTaskInstruction;
      else
        instruction = TransferTasks.Resources.EmployeeTaskInstruction;
      
      return Functions.Module.GetAssignmentStateView(instruction);
    }

    /// <summary>
    /// Перенести данные об изменениях условий труда из задачи в задание.
    /// </summary>
    /// <param name="assignment">Задание.</param>
    public void FillTransferDataFromTaskToAssignment(ITransferStatementApprovalAssignment assignment)
    {
      assignment.Department = _obj.Department;
      assignment.TransferDate = _obj.TransferDate;
      assignment.JobTitle = _obj.JobTitle;
      assignment.Employee = _obj.Employee;
      assignment.OldDepartment = _obj.OldDepartment;
      assignment.IsTransfer = _obj.IsTransfer;
    }

    /// <summary>
    /// Перенести данные об изменениях условий труда из задания в задачу.
    /// </summary>
    /// <param name="assignment">Задание.</param>
    public void FillTransferDataFromAssignmentToTask(ITransferStatementApprovalAssignment assignment)
    {
      _obj.Department = assignment.Department;
      _obj.TransferDate = assignment.TransferDate;
      _obj.JobTitle = assignment.JobTitle;
      _obj.Employee = assignment.Employee;
      _obj.OldDepartment = assignment.OldDepartment;
      _obj.IsTransfer = assignment.IsTransfer;
    }

    /// <summary>
    /// Получить доп. соглашение сотрудника.
    /// </summary>
    /// <returns>Доп. соглашение.</returns>
    [Public, Remote(IsPure = true)]
    public virtual IHRDocument GetAdditionalAgreement()
    {
      return _obj.AdditionalAgreement;
    }

    /// <summary>
    /// Получить приказ об изменениях условий труда сотрудника.
    /// </summary>
    /// <returns>Приказ.</returns>
    [Public, Remote(IsPure = true)]
    public virtual IHROrder GetEmployeeOrder()
    {
      return _obj.Order;
    }

    /// <summary>
    /// Получить заявление об изменениях условий труда сотрудника.
    /// </summary>
    /// <returns>Заявление.</returns>
    [Public, Remote(IsPure = true)]
    public virtual IHRDocument GetEmployeeStatement()
    {
      return _obj.Statement;
    }

    /// <summary>
    /// Создать заявление об изменениях условий труда.
    /// </summary>
    /// <returns>Заявление.</returns>
    public IHRDocument CreateTransferStatement()
    {
      var document = Functions.Module.CreateStatement(DocKind.TransferStatementKind, _obj.Employee, _obj.Started.Value);
      document.TransferDate = _obj.TransferDate;
      document.NewDepartment = _obj.Department;
      document.NewJobTitle = _obj.JobTitle;
      document.Save();
      return document;
    }

    /// <summary>
    /// Создать приказ об изменениях условий труда.
    /// </summary>
    /// <param name="baseDoc">Документ-основание.</param>
    /// <returns>Документ.</returns>
    [Remote]
    public IHROrder CreateTransferOrder(Sungero.Docflow.IOfficialDocument baseDoc)
    {
      var docKind = _obj.IsTransfer.Value ? DocKind.TransferOrderKind : DocKind.ChangeWorkConditionsOrderKind;
      var document = Functions.Module.CreateOrder(docKind, _obj.Employee, baseDoc);
      document.EventDate = _obj.TransferDate;
      document.NewDepartment = _obj.Department;
      document.NewJobTitle = _obj.JobTitle;
      document.Save();
      return document;
    }

    /// <summary>
    /// Переформировать приказ об изменениях условий труда.
    /// </summary>
    /// <param name="order">Приказ.</param>
    public void ReCreateTransferOrder(IHROrder order)
    {
      Functions.Module.RegenerateOrder(order);
    }

    /// <summary>
    /// Сформировать доп соглашение.
    /// </summary>
    /// <returns>Доп. соглашение.</returns>
    [Remote]
    public IHRDocument CreateTransferHRDocument()
    {
      var docKind = _obj.IsTransfer.Value ? DocKind.TransferAdditionalAgreementKind : DocKind.ChangeWorkConditionsAdditionalAgreementKind;
      var agreement = Functions.Module.CreateNewHRDocument(docKind, _obj.Employee, _obj.TransferDate.Value);
      if (_obj.IsTransfer.Value)
      {
        agreement.NewDepartment = _obj.Department;
        agreement.NewJobTitle = _obj.JobTitle;
      }
      agreement.Save();
      return agreement;
    }

    /// <summary>
    /// Переформировать доп соглашение и приказ.
    /// </summary>
    [Remote]
    public void ReCreateEmployeeHRDocuments()
    {
      Functions.Module.RegenerateDocument(_obj.AdditionalAgreement);
      Functions.TransferTask.ReCreateTransferOrder(_obj, _obj.Order);
    }
  }
}