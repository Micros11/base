using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.TransferTask;

namespace DirRX.HRManagement
{
  partial class TransferTaskSharedHandlers
  {

    public virtual void AdditionalAgreementChanged(DirRX.HRManagement.Shared.TransferTaskAdditionalAgreementChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue && e.NewValue != null)
      {
        if (e.OldValue != null && _obj.HRDocumentAttachmentGroup.HRDocumentBases.Contains(e.OldValue))
          _obj.HRDocumentAttachmentGroup.HRDocumentBases.Remove(e.OldValue);
        if (!_obj.HRDocumentAttachmentGroup.HRDocumentBases.Contains(e.NewValue))
          _obj.HRDocumentAttachmentGroup.HRDocumentBases.Add(e.NewValue);
      }
    }

    public virtual void OrderChanged(DirRX.HRManagement.Shared.TransferTaskOrderChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue && e.NewValue != null)
      {
        if (e.OldValue != null && _obj.HRDocumentAttachmentGroup.HROrders.Contains(e.OldValue))
          _obj.HRDocumentAttachmentGroup.HROrders.Remove(e.OldValue);
        if (!_obj.HRDocumentAttachmentGroup.HROrders.Contains(e.NewValue))
          _obj.HRDocumentAttachmentGroup.HROrders.Add(e.NewValue);
      }
    }

    public virtual void IsManagerStartedChanged(Sungero.Domain.Shared.BooleanPropertyChangedEventArgs e)
    {
      // Невозможен одновременный старт и от руководителя, и от СОК
      var errors = new List<string>();
      if (e.NewValue.Value && _obj.IsHRStarted.Value)
        errors.Add(TransferTasks.Resources.TransferTaskStartRoleErrorFormat(Resources.RoleNameHiringResponsible));
      e.Params.AddOrUpdate("Errors", string.Join(" ", errors));
    }

    public virtual void IsHRStartedChanged(Sungero.Domain.Shared.BooleanPropertyChangedEventArgs e)
    {
      // Невозможен одновременный старт и от руководителя, и от СОК
      var errors = new List<string>();
      if (e.NewValue.Value && _obj.IsManagerStarted.Value)
        errors.Add(TransferTasks.Resources.TransferTaskStartRoleErrorFormat(Resources.RoleNameHiringResponsible));
      e.Params.AddOrUpdate("Errors", string.Join(" ", errors));
    }

    public virtual void SignImmediatelyChanged(Sungero.Domain.Shared.BooleanPropertyChangedEventArgs e)
    {
      // IsTransfer и SignImmediately - взаимоисключающие
      var errors = new List<string>();
      if (e.NewValue.Value && _obj.IsTransfer.Value)
        errors.Add(TransferTasks.Resources.TransferStartError);
      e.Params.AddOrUpdate("Errors", string.Join(" ", errors));
      
    }

    public virtual void IsTransferChanged(Sungero.Domain.Shared.BooleanPropertyChangedEventArgs e)
    {
      // IsTransfer и SignImmediately - взаимоисключающие
      if (e.NewValue.Value)
      {
        var errors = new List<string>();
        if (_obj.SignImmediately.Value)
          errors.Add(TransferTasks.Resources.TransferStartError);
        e.Params.AddOrUpdate("Errors", string.Join(" ", errors));
      }
      else
      {
        // Почистить поля при снятии галочки
        _obj.Department = Sungero.Company.Departments.Null;
        _obj.JobTitle = Sungero.Company.JobTitles.Null;
      }
      Functions.TransferTask.SetEnabledTransferProperties(_obj);
    }

    public virtual void OldDepartmentChanged(DirRX.HRManagement.Shared.TransferTaskOldDepartmentChangedEventArgs e)
    {
      var department = e.NewValue;
      // Очистить сотрудника, если очистили подразделение или подразделение поменялось на несовпадающее с подразделеним сотрудника
      if ((e.NewValue == null) || (e.OldValue != null && _obj.Employee != null && !_obj.Employee.Department.Equals(department)))
        _obj.Employee = null;
      
      // Очистить новое подразделение, если НОР нового и текущего не совпадают
      if (department != null && _obj.Department != null && !_obj.Department.BusinessUnit.Equals(department.BusinessUnit))
        _obj.Department = null;
    }

    public virtual void EmployeeChanged(DirRX.HRManagement.Shared.TransferTaskEmployeeChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue && e.NewValue != null)
      {
        _obj.Subject = TransferTasks.Resources.TransferTaskSubjectFormat(e.NewValue.Name);
        // Заполнить подразделение, если оно было пустое или если не совпадает с подразделеним сотрудника
        if (_obj.OldDepartment == null || (_obj.OldDepartment != null && !_obj.OldDepartment.Equals(e.NewValue.Department)))
          _obj.OldDepartment = e.NewValue.Department;
      }
      if (e.NewValue == null)
        _obj.Subject = TransferTasks.Resources.TransferTaskSubjectFormat(string.Empty);
    }

  }
}