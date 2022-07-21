using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.InformingTask;

namespace DirRX.HRLite
{
  partial class InformingTaskClientHandlers
  {

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      var employees = Functions.InformingTask.Remote.GetRecipients(_obj);
      if (employees.Any() && (EssPlatform.PublicFunctions.Module.Remote.HasEmployeeNotInESS(employees) || !Functions.Module.Remote.CheckEmployeeConsent(employees)))
        e.AddWarning(DirRX.HRLite.InformingTasks.Resources.NotAllEmployeeInforming, _obj.Info.Actions.ShowEmployeeNoESS);
    }

  }
}