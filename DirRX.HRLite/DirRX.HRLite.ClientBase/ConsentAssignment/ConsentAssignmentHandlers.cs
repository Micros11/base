using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.ConsentAssignment;

namespace DirRX.HRLite
{
  partial class ConsentAssignmentClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      // Сообщить о запрете возможности выполнения задания сотрудником на стороне RX.
      if (!Functions.ConsentAssignment.Remote.IsPossiblePerformAssignment(_obj))
        e.AddError(ConsentAssignments.Resources.DenyPerformingFromWeb);
    }

  }
}