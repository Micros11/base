using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.HRAcquaintanceAssignment;

namespace DirRX.HRLite
{
  partial class HRAcquaintanceAssignmentClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      // Сообщить о запрете возможности выполнения задания сотрудником на стороне RX.
      if (!Functions.HRAcquaintanceAssignment.Remote.IsPossiblePerformAssignment(_obj))
        e.AddError(HRAcquaintanceAssignments.Resources.DenyPerformingFromWeb);
    }

  }
}