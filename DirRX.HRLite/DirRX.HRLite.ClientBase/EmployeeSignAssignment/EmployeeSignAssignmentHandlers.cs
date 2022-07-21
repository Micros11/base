using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.EmployeeSignAssignment;

namespace DirRX.HRLite
{
  partial class EmployeeSignAssignmentClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      // Сообщить о запрете возможности выполнения задания сотрудником на стороне RX.
      if (!Functions.EmployeeSignAssignment.Remote.IsPossiblePerformAssignment(_obj))
        e.AddError(EmployeeSignAssignments.Resources.DenyPerformingFromWeb);
    }

  }
}