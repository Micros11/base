using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Company;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.TransferTask;
using DocKind = DirRX.HRManagement.Constants.Module.DocumentKind;
using HRRoles = DirRX.HRManagement.Constants.Module.HRRoles;

namespace DirRX.HRManagement
{
  partial class TransferTaskDepartmentPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> DepartmentFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      if (_obj.OldDepartment != null)
        return query.Where(l => l.BusinessUnit.Equals(_obj.OldDepartment.BusinessUnit));
      return query;
    }
  }

  partial class TransferTaskOldDepartmentPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> OldDepartmentFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      var users = Sungero.CoreEntities.Substitutions.ActiveSubstitutedUsersWithoutSystem.ToList();
      users.Add(Users.Current);
      var userResponsibleInBusinessUnits = users.Where(v => v.IncludedIn(Functions.Module.GetRole(DirRX.HRLite.PublicConstants.Module.Roles.HRSpecialist))).Select(u => Employees.As(u).Department.BusinessUnit);
      if (userResponsibleInBusinessUnits.Any())
        return query.Where(d => userResponsibleInBusinessUnits.Contains(d.BusinessUnit) && d.Status == Sungero.Company.Department.Status.Active);
      else
        return query.Where(d => users.Contains(Users.As(d.Manager)) && d.Status == Sungero.Company.Department.Status.Active);
    }
  }

  partial class TransferTaskEmployeePropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> EmployeeFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      if (_obj.OldDepartment != null)
        query = query.Where(l => l.Department.Equals(_obj.OldDepartment));
      
      var users = Sungero.CoreEntities.Substitutions.ActiveSubstitutedUsersWithoutSystem.ToList();
      users.Add(Users.Current);
      var userResponsibleInBusinessUnits = users.Where(v => v.IncludedIn(Functions.Module.GetRole(DirRX.HRLite.PublicConstants.Module.Roles.HRSpecialist))).Select(u => Employees.As(u).Department.BusinessUnit);
      if (userResponsibleInBusinessUnits.Any())
        return query.Where(v => userResponsibleInBusinessUnits.Contains(v.Department.BusinessUnit) && v.Status == Sungero.Company.Employee.Status.Active);
      else
        return query.Where(v => users.Contains(Users.As(v.Department.Manager)) && v.Status == Sungero.Company.Employee.Status.Active);
    }
  }

  partial class TransferTaskServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      // Стартовать задачу может только сотрудник
      if (!Sungero.Company.Employees.Is(_obj.Author))
        e.Params.AddOrUpdate("Error", Sungero.Docflow.Resources.CantSendTaskByNonEmployee.ToString());
      _obj.IsHRStarted = false;
      _obj.IsManagerStarted = false;
      _obj.IsTransfer = false;
      _obj.SignImmediately = false;
      _obj.Subject = Sungero.Docflow.Resources.AutoformatTaskSubject;
    }

    public override void BeforeAbort(Sungero.Workflow.Server.BeforeAbortEventArgs e)
    {
      if (_obj.State.Properties.Status.OriginalValue == Sungero.Workflow.Task.Status.Draft)
        return;
      Functions.Module.SendAbortationNotices(_obj, TransferTasks.Resources.AbortNoticeFormat(_obj.Employee.Name), _obj.AbortingReason, _obj.Author, _obj.ThreadSubject);
    }

    public override void BeforeStart(Sungero.Workflow.Server.BeforeStartEventArgs e)
    {
      // СОК должен вложить на старте приказ и проект доп. соглашения
      if (_obj.IsHRStarted.Value && !_obj.HRDocumentAttachmentGroup.All.Any())
        e.AddError(_obj.Info.Properties.Order, DirRX.HRManagement.TransferTasks.Resources.NeedGenerateOrderAndAgreement);
      // Сотрудник должен заполнить текст задачи
      if (!_obj.IsHRStarted.Value && !_obj.IsManagerStarted.Value && _obj.ActiveText == string.Empty)
        e.AddError(_obj.Info.Properties.Texts, DirRX.HRManagement.TransferTasks.Resources.EmptyActiveTextError);
      // Руководитель должен заполнить текст задачи, если это не перевод
      if (_obj.IsManagerStarted.Value && !_obj.IsTransfer.Value && _obj.ActiveText == string.Empty)
        e.AddError(_obj.Info.Properties.Texts, DirRX.HRManagement.TransferTasks.Resources.EmptyActiveTextError);
      // Дата изменения условий труда должна быть больше сегодня
      if (_obj.TransferDate != null && _obj.TransferDate <= Calendar.Today)
        e.AddError(_obj.Info.Properties.TransferDate, TransferTasks.Resources.TransferDateError);
      // Вложить карточку сотрудника
      if (!_obj.EmploymentAttachmentGroup.Employees.Contains(_obj.Employee))
        _obj.EmploymentAttachmentGroup.Employees.Add(_obj.Employee);
      // Проверить, не поставлены ли документы из параметров задачи в очередь на удаление (вариант запуска от СОК)
      // (заглушка для кейса: ошибочно поставили галочку "Перевод в другое подразделение", документы удалились из вложений и параметров,
      //  добавились в очередь на удаление. Инициатор нажал Отмену, документы восстановились, но в очереди остались)
      if (_obj.IsHRStarted.Value && _obj.Order != null)
      {
        var orderQueueItem = DocumentDeleteQueueItems.GetAll(d => d.DocumentId == _obj.Order.Id).FirstOrDefault();
        if (orderQueueItem != null)
          DocumentDeleteQueueItems.Delete(orderQueueItem);
        var agreementQueueItem = DocumentDeleteQueueItems.GetAll(d => d.DocumentId == _obj.AdditionalAgreement.Id).FirstOrDefault();
        if (agreementQueueItem != null)
          DocumentDeleteQueueItems.Delete(agreementQueueItem);
      }
    }
  }
}