using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.VacationDepartmentSchedulingTask;
using ParamKeys = DirRX.HRManagement.Constants.Module.ParamKey;

namespace DirRX.HRManagement
{
  partial class VacationDepartmentSchedulingTaskServerHandlers
  {

    public override void BeforeAbort(Sungero.Workflow.Server.BeforeAbortEventArgs e)
    {
      Functions.Module.AddDocflowParam(string.Format(ParamKeys.SchedulingKeyDepartment, _obj.Department.Id), false.ToString());
      var mainTaskAuthor = _obj.MainSchedulingTask.Author;
      var performers = new List<IUser>();
      performers.Add(Functions.Module.GetManager(_obj.Department));
      Sungero.Docflow.PublicFunctions.Module.Remote.SendNoticesAsSubtask(VacationDepartmentSchedulingTasks.Resources.AbortNoticeSubjectFormat(_obj.Year.Value.Year, _obj.Department.Name),
                                                                         performers,
                                                                         _obj,
                                                                         _obj.AbortingReason,
                                                                         mainTaskAuthor,
                                                                         _obj.ThreadSubject);
    }

    public override void BeforeStart(Sungero.Workflow.Server.BeforeStartEventArgs e)
    {
      Functions.Module.AddDocflowParam(string.Format(ParamKeys.SchedulingKeyDepartment, _obj.Department.Id), true.ToString());
    }
  }
}