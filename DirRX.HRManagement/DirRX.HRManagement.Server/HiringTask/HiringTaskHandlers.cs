using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.HiringTask;

namespace DirRX.HRManagement
{
  partial class HiringTaskServerHandlers
  {

    public override void BeforeResume(Sungero.Workflow.Server.BeforeResumeEventArgs e)
    {
      _obj.Employee.Status = Sungero.Company.Employee.Status.Active;
      _obj.Employee.Save();
    }

    public override void BeforeAbort(Sungero.Workflow.Server.BeforeAbortEventArgs e)
    {
      if (_obj.State.Properties.Status.OriginalValue == Sungero.Workflow.Task.Status.Draft)
        return;
      
      _obj.Employee.Status = Sungero.Company.Employee.Status.Closed;
      _obj.Employee.Save();
      
      Functions.Module.SendAbortationNotices(_obj, HiringTasks.Resources.AbortNoticeFormat(_obj.Employee.Name), _obj.AbortingReason, _obj.Author, _obj.ThreadSubject);
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      if (_obj.Person != null)
        _obj.Subject = HiringTasks.Resources.HiringTaskSubjectFormat(_obj.Person.Name);
      else
        _obj.Subject = Sungero.Docflow.Resources.AutoformatTaskSubject;
      
      _obj.ExistsProcPersonalDataAgreement = false;
      _obj.ActiveText = HiringTasks.Resources.HiringTaskActiveText;
      
    }
  }

}