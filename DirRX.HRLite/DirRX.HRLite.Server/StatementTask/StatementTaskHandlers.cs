using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.StatementTask;

namespace DirRX.HRLite
{
  partial class StatementTaskServerHandlers
  {

    public override void BeforeAbort(Sungero.Workflow.Server.BeforeAbortEventArgs e)
    {
      if (_obj.State.Properties.Status.OriginalValue == Sungero.Workflow.Task.Status.Draft)
        return;
      var performers = Sungero.Workflow.Assignments.GetAll(a => Equals(a.Task, _obj)).Select(a => a.Performer).ToList();
      if (performers.Any())
        Sungero.Docflow.PublicFunctions.Module.Remote.SendNoticesAsSubtask(StatementTasks.Resources.AbortNoticeFormat(_obj.Author.Name), performers, _obj, _obj.AbortingReason, _obj.Author, _obj.ThreadSubject);
    }

    public override void BeforeStart(Sungero.Workflow.Server.BeforeStartEventArgs e)
    {
      var statement = _obj.DocumentGroup.StatementDocuments.FirstOrDefault();      
      _obj.Author = statement.Author;
      if (!PublicFunctions.Module.Remote.CheckEmployeeConsent(HRSolution.Employees.As(_obj.Author)))
        e.AddError(Resources.EmployeeConsentError);
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.Subject = Sungero.Docflow.Resources.AutoformatTaskSubject;
    }
  }

}