using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.TransferStatementApprovalAssignment;

namespace DirRX.HRManagement.Shared
{
  partial class TransferStatementApprovalAssignmentFunctions
  {
    /// <summary>
    /// Установить доступность свойств карточки, касающихся перевода, в зависимости от контекста и блока.
    /// </summary>
    /// <remarks>
    /// Блок:
    ///    - согласование руководителем - видимость, доступность и обязательность всех полей группы регулируется контекстом
    ///    - доработка сотрудником - доступных полей нет, видимых полей нет
    ///    - доработка руководителем - видимость, доступность и обязательность всех полей группы регулируется контекстом
    ///    - прочие блоки - видимость полей группы регулируется контекстом, поля недоступны
    /// Контекст:
    ///    - "Перевод в другое подразделение" - видима соответствующая группа полей.
    /// </remarks>
    public virtual void SetEnabledTransferProperties()
    {
      var isTransfer = _obj.IsTransfer.Value;
      
      // Блок доработки.
      // Блок согласования руководителем - поля доступны и обязательны.
      // Остальные блоки - видимы все поля, доступных нет.
      if (_obj.BlockUid == Constants.TransferTask.InitiatorReWorkBlockId)
      {
        var task = TransferTasks.As(_obj.Task);
        // Если на доработку пришло руководителю или СОК - поля доступны и обязательны.
        // Если на доработку пришло сотруднику - поля невидимы.
        if (task.IsManagerStarted.Value || task.IsHRStarted.Value)
        {
          _obj.State.Properties.JobTitle.IsVisible = isTransfer;
          _obj.State.Properties.JobTitle.IsEnabled = isTransfer;
          _obj.State.Properties.JobTitle.IsRequired = isTransfer;
          _obj.State.Properties.Department.IsVisible = isTransfer;
          _obj.State.Properties.Department.IsEnabled = isTransfer;
          _obj.State.Properties.Department.IsRequired = isTransfer;
        }
        else
        {
          _obj.State.Properties.JobTitle.IsVisible = false;
          _obj.State.Properties.Department.IsVisible = false;
        }
      }
      else if (_obj.BlockUid == Constants.TransferTask.ApprovalOldManagerAssignmentBlockId)
      {
        _obj.State.Properties.JobTitle.IsVisible = isTransfer;
        _obj.State.Properties.JobTitle.IsEnabled = isTransfer;
        _obj.State.Properties.JobTitle.IsRequired = isTransfer;
        _obj.State.Properties.Department.IsVisible = isTransfer;
        _obj.State.Properties.Department.IsEnabled = isTransfer;
        _obj.State.Properties.Department.IsRequired = isTransfer;
      }
      else
      {
        _obj.State.Properties.JobTitle.IsVisible = isTransfer;
        _obj.State.Properties.JobTitle.IsEnabled = false;
        _obj.State.Properties.Department.IsVisible = isTransfer;
        _obj.State.Properties.Department.IsEnabled = false;
      }
    }
    
    /// <summary>
    /// Установить доступность свойств карточки в зависимости от контекста и блока.
    /// </summary>
    /// <remarks>
    /// Блок:
    ///    - согласование руководителем - видимы все поля, доступны и обязательны поля, кроме данных сотрудника
    ///    - согласование новым руководителем - видимы все поля, доступных нет
    ///    - согласование сотрудником - видимы все поля, доступных нет
    ///    - доработка сотрудником - доступных полей нет, видимых полей нет
    ///    - доработка руководителем - видимы все поля, доступны и обязательны поля, кроме данных сотрудника
    /// Контекст:
    ///    - "Перевод в другое подразделение" - видима соответствующая группа полей.
    /// </remarks>
    public virtual void SetEnabledProperties()
    {
      // Блок доработки.
      // Блок согласования руководителем - доступны все поля, но "Перевод в другое подразделение" видима только до принятия решения о переводе -
      // пока руководитель не выполнит с результатом "Согласовать". Дата обязательна.
      // Остальные блоки - видимы все поля, кроме галочки "Перевод в другое подразделение", доступных нет.
      if (_obj.BlockUid == Constants.TransferTask.InitiatorReWorkBlockId)
      {
        // На доработке нельзя проставить или снять галочку "Перевод в другое подразделение" - скрыть ее.
        _obj.State.Properties.IsTransfer.IsVisible = false;
        var task = TransferTasks.As(_obj.Task);
        // Если на доработку пришло руководителю или СОК- поля доступны, дата обязательна.
        // Если на доработку пришло сотруднику - поля скрыты.
        if (task.IsManagerStarted.Value || task.IsHRStarted.Value)
        {
          _obj.State.Properties.TransferDate.IsRequired = true;
        }
        else
        {
          _obj.State.Properties.Employee.IsVisible = false;
          _obj.State.Properties.OldDepartment.IsVisible = false;
          _obj.State.Properties.TransferDate.IsVisible = false;
        }
      }
      else if (_obj.BlockUid == Constants.TransferTask.ApprovalOldManagerAssignmentBlockId)
      {
        _obj.State.Properties.TransferDate.IsRequired = true;
        _obj.State.Properties.IsTransfer.IsVisible = !TransferStatementApprovalAssignments.GetAll(v => v.Task.Equals(_obj.Task) && v.BlockUid == _obj.BlockUid &&
                                                                                                  v.Result == DirRX.HRManagement.TransferStatementApprovalAssignment.Result.Complete).Any();
      }
      else
      {
        _obj.State.Properties.TransferDate.IsEnabled = false;
        _obj.State.Properties.IsTransfer.IsVisible = false;
      }
      
      // Настроить обязательность и доступность полей, касающихся перевода.
      this.SetEnabledTransferProperties();
    }
  }
}