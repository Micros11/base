using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Company;
using DirRX.HRManagement.VacationSchedulingTask;
using ParamKeys = DirRX.HRManagement.Constants.Module.ParamKey;

namespace DirRX.HRManagement
{
  partial class VacationSchedulingTaskServerHandlers
  {
    public override void BeforeAbort(Sungero.Workflow.Server.BeforeAbortEventArgs e)
    {
      Functions.Module.AddDocflowParam(string.Format(ParamKeys.SchedulingKeyBusinessUnit, _obj.BusinessUnit.Id), false.ToString());
      Functions.Module.SendAbortationNotices(_obj, VacationSchedulingTasks.Resources.AbortNoticeSubjectFormat(_obj.Year.Value.Year, _obj.BusinessUnit.Name), _obj.AbortingReason, _obj.Author, _obj.ThreadSubject);
    }

    public override void BeforeStart(Sungero.Workflow.Server.BeforeStartEventArgs e)
    {
      var paramName = string.Format(ParamKeys.SchedulingKeyBusinessUnit, _obj.BusinessUnit.Id);
      // Если в НОР не идет процесс планирования отпусков, то отправить задачу.
      if (!Equals(Functions.Module.GetDocflowParamValue(paramName), true.ToString()))
        Functions.Module.AddDocflowParam(paramName, true.ToString());
      else
        e.AddError(VacationSchedulingTasks.Resources.SchedulingInProgressFormat(_obj.BusinessUnit.Name));
    }

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      _obj.Subject = VacationSchedulingTasks.Resources.TaskSubjectFormat(_obj.Year.Value.Year, _obj.BusinessUnit.Name);
    }
    
    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.Subject = Sungero.Docflow.Resources.AutoformatTaskSubject;
      _obj.Year = Calendar.Today.NextYear();
      if (!Functions.Module.IsServiceUser(Users.Current))
        _obj.BusinessUnit = Employees.Current.Department.BusinessUnit;
    }
  }

}