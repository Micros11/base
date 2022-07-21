using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Shared;
using Sungero.Company;
using DirRX.HRManagement.VacationShiftTask;

namespace DirRX.HRManagement
{
  partial class VacationShiftTaskClientHandlers
  {

    public virtual void IgnoreRequiredDurationValueInput(Sungero.Presentation.BooleanValueInputEventArgs e)
    {
      // если задачу стартует руководитель и стартует не по себе - то ошибка критичная, если сотрудник - warning
      string errorType = string.Empty;
      string requiredDurationError = string.Empty;
      // если отправляют задачу по себе, то проверка предупреждающая, т.к. сам сотрудник всё равно не может себе разрешить нарушение правила
      if (Equals(Employees.Current, _obj.Employee)) {
        errorType = "Warnings";
        requiredDurationError = VacationShiftTasks.Resources.VacationRequiredDurationWarning;
      } 
      else {
        errorType = "Errors";
        requiredDurationError = VacationShiftTasks.Resources.VacationRequiredDurationError;
      }
      // вывести сообщение, если ошибка есть, а галочка снята.
      if (_obj.IsRequiredDurationBroken.HasValue && _obj.IsRequiredDurationBroken.Value && !e.NewValue.Value) {
         e.Params.AddOrUpdate(errorType, string.Join(" ", VacationShiftTasks.Resources.VacationRequiredDurationError));        
         Logger.Debug(" ''>> повесить сообщение");
      } 
      else {
        // поставили галочку "Игнорировать..." - снять сообщение об ошибке
        e.Params.Remove(errorType);
        Logger.Debug(" ''>> снять сообщение");
      }
    }
    
    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      // Очистить предыдущие сообщения об ошибках.
      var vo = (Sungero.Domain.Shared.Validation.IValidationObject)_obj;
      vo.ValidationResult.Clear();
      
      // Выводить ошибки, если параметр не пустой.
      var warning = string.Empty;
      if (e.Params.TryGetValue("Warnings", out warning) && !string.IsNullOrEmpty(warning))
        e.AddWarning(warning);

      var errors = string.Empty;
      if (e.Params.TryGetValue("Errors", out errors) && !string.IsNullOrEmpty(errors))
        e.AddError(errors);

      // если ранее обнаружено нарушение правила 14-ти дней и не отключили проверку, то вывести соответствующее сообщение
      if ((!_obj.IgnoreRequiredDuration.HasValue || (_obj.IgnoreRequiredDuration.HasValue && !_obj.IgnoreRequiredDuration.Value))
         && (_obj.IsRequiredDurationBroken.HasValue && _obj.IsRequiredDurationBroken.Value)
         && _obj.Employee != null) {
        // Текст сообщения зависит от по себе стартуют задачу или по кому-то
        if (!Equals(Employees.Current, _obj.Employee))
          e.AddError(VacationShiftTasks.Resources.VacationRequiredDurationError);
        else
          e.AddWarning(VacationShiftTasks.Resources.VacationRequiredDurationWarning);
      }
    }

    public virtual void DataBegin1ValueInput(Sungero.Presentation.DateTimeValueInputEventArgs e)
    {
      var error = Functions.VacationShiftTask.CheckInputData(_obj, e.NewValue, _obj.Duration1);
      if (!string.IsNullOrEmpty(error))
        e.AddError(error);
    }
    
    public virtual void Duration1ValueInput(Sungero.Presentation.IntegerValueInputEventArgs e)
    {
      var error = Functions.VacationShiftTask.CheckInputData(_obj, _obj.DataBegin1, e.NewValue);
      if (!string.IsNullOrEmpty(error))
        e.AddError(error);
    }
    
    public virtual void DataBegin2ValueInput(Sungero.Presentation.DateTimeValueInputEventArgs e)
    {
      var error = Functions.VacationShiftTask.CheckInputData(_obj, e.NewValue, _obj.Duration2);
      if (!string.IsNullOrEmpty(error))
        e.AddError(error);
    }
    
    public virtual void Duration2ValueInput(Sungero.Presentation.IntegerValueInputEventArgs e)
    {
      var error = Functions.VacationShiftTask.CheckInputData(_obj, _obj.DataBegin2, e.NewValue);
      if (!string.IsNullOrEmpty(error))
        e.AddError(error);
    }

    public virtual void DataBegin3ValueInput(Sungero.Presentation.DateTimeValueInputEventArgs e)
    {
      var error = Functions.VacationShiftTask.CheckInputData(_obj, e.NewValue, _obj.Duration3);
      if (!string.IsNullOrEmpty(error))
        e.AddError(error);
    }
    
    public virtual void Duration3ValueInput(Sungero.Presentation.IntegerValueInputEventArgs e)
    {
      var error = Functions.VacationShiftTask.CheckInputData(_obj, _obj.DataBegin3, e.NewValue);
      if (!string.IsNullOrEmpty(error))
        e.AddError(error);
    }

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      PublicFunctions.Module.Remote.CheckVacationsModuleLicense();
      Functions.VacationShiftTask.SetEnabledProperties(_obj);
      Functions.VacationShiftTask.SetVisibleProperties(_obj);
    }
  }
}