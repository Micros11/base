using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.InformingTask;

namespace DirRX.HRLite
{
  partial class InformingTaskServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.Subject = InformingTasks.Resources.InformingTaskSubject;
    }
  }

}