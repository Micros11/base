using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.TransferStatementApprovalAssignment;

namespace DirRX.HRManagement
{
  partial class TransferStatementApprovalAssignmentSharedHandlers
  {

    public virtual void IsTransferChanged(Sungero.Domain.Shared.BooleanPropertyChangedEventArgs e)
    {
      Functions.TransferStatementApprovalAssignment.SetEnabledTransferProperties(_obj);
    }
  }
}