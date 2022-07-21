using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.RefusalAcqAssignment;

namespace DirRX.HRLite
{
  partial class RefusalAcqAssignmentClientHandlers
  {

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      _obj.State.Properties.RefusalResult.IsRequired = true;
      _obj.State.Properties.RefusalResult.Properties.Result.IsRequired = true;
      _obj.State.Properties.RefusalResult.Properties.Performer.IsRequired = true;
    }
  }

}