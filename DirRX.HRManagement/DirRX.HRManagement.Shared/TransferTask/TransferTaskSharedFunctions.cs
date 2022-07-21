using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.TransferTask;

namespace DirRX.HRManagement.Shared
{
  partial class TransferTaskFunctions
  {
    /// <summary>
    /// Установить доступность свойств задачи, касающихся перевода, в зависимости от галочки "Перевод в другое подразделение".
    /// </summary>
    public virtual void SetEnabledTransferProperties()
    {
      // Данные о переводе - доступны и обязательны, если установлена галочка "Перевод".
      var isTransfer = _obj.IsTransfer.Value;
      _obj.State.Properties.Department.IsVisible = isTransfer;
      _obj.State.Properties.Department.IsRequired = isTransfer;
      _obj.State.Properties.JobTitle.IsVisible = isTransfer;
      _obj.State.Properties.JobTitle.IsRequired = isTransfer;
    }
    
    /// <summary>
    /// Установить доступность свойств карточки в зависимости от инициатора задачи и текущего состояния полей.
    /// </summary>
    /// <remarks>
    /// Инициатор:
    ///    - сотрудник - доступных полей нет, видимых полей нет
    ///    - руководитель - доступны все поля, кроме "Согласование с сотрудником не требуется"
    ///    - СОК - доступны все поля
    /// От каких полей зависит:
    ///    - "Перевод в другое подразделение" - при установленной галочке недоступна галочка "Согласование с сотрудником не требуется"
    ///    - "Согласование с сотрудником не требуется" - при установленной галочке недоступна галочка "Перевод в другое подразделение" и соответствующая группа полей.
    /// </remarks>
    public virtual void SetEnabledProperties()
    {
      var isHRStarted = _obj.IsHRStarted.Value;
      
      // Согласование с сотрудником не требуется - доступно только СОК.
      _obj.State.Properties.SignImmediately.IsVisible = isHRStarted;
      
      // Для задачи, запущенной от руководителя или СОК Дата обязательна для заполнения.
      if (_obj.IsManagerStarted.Value || isHRStarted)
        _obj.State.Properties.TransferDate.IsRequired = true;
      // Для задачи, запущенной от сотрудника скрыть все поля.
      else
      {
        _obj.State.Properties.OldDepartment.IsVisible = false;
        _obj.State.Properties.Employee.IsVisible = false;
        _obj.State.Properties.TransferDate.IsVisible = false;
        _obj.State.Properties.IsTransfer.IsVisible = false;
      }
      
      this.SetEnabledTransferProperties();
    }
  }
}