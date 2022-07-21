using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Company;
using DirRX.HRManagement.ShiftStatementApprovalAssignment;
using DateStatuses = DirRX.HRManagement.Constants.Module.DateStatus;

namespace DirRX.HRManagement
{

  partial class ShiftStatementApprovalAssignmentClientHandlers
  {

    public virtual void IgnoreRequiredDurationValueInput(Sungero.Presentation.BooleanValueInputEventArgs e)
    {
      // если задание руководителю - то ошибка критичная, если сотруднику - warning
      var errorType = "Warnings";
      // если задание "согласование с руководителем", то сообщение будет критичное
      if (_obj.BlockUid == Constants.VacationShiftTask.ManagerApprovingAssignmentBlockId)
        errorType = "Errors";
      if (_obj.IsRequiredDurationBroken.HasValue && _obj.IsRequiredDurationBroken.Value &&
         !e.NewValue.Value) {
        e.Params.AddOrUpdate(errorType, string.Join(" ", VacationShiftTasks.Resources.VacationRequiredDurationError));
      } 
      else 
      {
        // поставили галочку "Игнорировать..." - снять сообщение об ошибке
        e.Params.Remove(errorType);
      }
    }

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      // Очистить предыдущие сообщения об ошибках.
      var canComplete = true;
      if (e.Params.TryGetValue("CanComplete", out canComplete) && canComplete || !e.Params.TryGetValue("CanComplete", out canComplete))
      {
        var vo = (Sungero.Domain.Shared.Validation.IValidationObject)_obj;
        vo.ValidationResult.Clear();
      }
      
      // Выводить ошибки, если параметр не пустой.
      var warning = string.Empty;
      if (e.Params.TryGetValue("Warnings", out warning) && !string.IsNullOrEmpty(warning))
        e.AddWarning(warning);
      
      var errors = string.Empty;
      if (e.Params.TryGetValue("Errors", out errors) && !string.IsNullOrEmpty(errors))
        e.AddError(errors);

      var task = VacationShiftTasks.As(_obj.Task);
      // спецобработка на 14 дней имеет смысл только для задач на отзыв
      if (task.IsVacationRecall.Value) {
        // если правило 14-ти дней было нарушено при вводе даты отзыва и не было отключена проверка
        // то вывести соответствующее предупреждение
        if ((_obj.IsRequiredDurationBroken.HasValue && _obj.IsRequiredDurationBroken.Value)
            && (_obj.IgnoreRequiredDuration.HasValue && !_obj.IgnoreRequiredDuration.Value)) {
          // критичность зависит от этапа задачи
          // для доработки сотрудником - предупреждение
          // для согласования руководителем - ошибка
          if (_obj.BlockUid == Constants.VacationShiftTask.ManagerApprovingAssignmentBlockId)
            e.AddError(VacationShiftTasks.Resources.VacationRequiredDurationError);
          else
            e.AddWarning(VacationShiftTasks.Resources.VacationRequiredDurationWarning);
        }
      }
    }

    public virtual void DataBegin1ValueInput(Sungero.Presentation.DateTimeValueInputEventArgs e)
    {
      var error = Functions.VacationShiftTask.CheckInputData(VacationShiftTasks.As(_obj.Task), e.NewValue, _obj.Duration1);
      if (!string.IsNullOrEmpty(error))
        e.AddError(error);
    }
    
    public virtual void Duration1ValueInput(Sungero.Presentation.IntegerValueInputEventArgs e)
    {
      var error = Functions.VacationShiftTask.CheckInputData(VacationShiftTasks.As(_obj.Task), _obj.DataBegin1, e.NewValue);
      if (!string.IsNullOrEmpty(error))
        e.AddError(error);
    }

    public virtual void DataBegin2ValueInput(Sungero.Presentation.DateTimeValueInputEventArgs e)
    {
      var error = Functions.VacationShiftTask.CheckInputData(VacationShiftTasks.As(_obj.Task), e.NewValue, _obj.Duration2);
      if (!string.IsNullOrEmpty(error))
        e.AddError(error);
    }
    
    public virtual void Duration2ValueInput(Sungero.Presentation.IntegerValueInputEventArgs e)
    {
      var error = Functions.VacationShiftTask.CheckInputData(VacationShiftTasks.As(_obj.Task), _obj.DataBegin2, e.NewValue);
      if (!string.IsNullOrEmpty(error))
        e.AddError(error);
    }
    
    public virtual void Duration3ValueInput(Sungero.Presentation.IntegerValueInputEventArgs e)
    {
      var error = Functions.VacationShiftTask.CheckInputData(VacationShiftTasks.As(_obj.Task), _obj.DataBegin3, e.NewValue);
      if (!string.IsNullOrEmpty(error))
        e.AddError(error);
    }
    
    public virtual void DataBegin3ValueInput(Sungero.Presentation.DateTimeValueInputEventArgs e)
    {
      var error = Functions.VacationShiftTask.CheckInputData(VacationShiftTasks.As(_obj.Task), e.NewValue, _obj.Duration3);
      if (!string.IsNullOrEmpty(error))
        e.AddError(error);
    }

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      Functions.ShiftStatementApprovalAssignment.SetEnabledProperties(_obj);
      Functions.ShiftStatementApprovalAssignment.SetVisibleProperties(_obj);
    }

  }
}