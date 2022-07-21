using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.DismissalPrepareDocsAssignment;

namespace DirRX.HRManagement
{
  partial class DismissalPrepareDocsAssignmentClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      _obj.State.Properties.DismissalDate.IsVisible = _obj.BlockUid == Constants.DismissalTask.PrepareDocsAssignmentBlockId;
    }

  }
}