using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.ConsentTask;

namespace DirRX.HRLite
{
  partial class ConsentTaskServerHandlers
  {

    public override void BeforeAbort(Sungero.Workflow.Server.BeforeAbortEventArgs e)
    {
      Functions.Module.ChangeConsentStatusAsync(_obj.Employee, HRSolution.Employee.ConsentDirRX.NotSent.ToString());
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.Subject = Sungero.Docflow.Resources.AutoformatTaskSubject;
    }
  }

}