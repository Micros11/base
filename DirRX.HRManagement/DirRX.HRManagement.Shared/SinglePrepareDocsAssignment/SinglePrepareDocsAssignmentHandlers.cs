﻿using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.SinglePrepareDocsAssignment;

namespace DirRX.HRManagement
{
  partial class SinglePrepareDocsAssignmentSharedHandlers
  {

    public virtual void OrderChanged(DirRX.HRManagement.Shared.SinglePrepareDocsAssignmentOrderChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue)
      {
        _obj.OrderAttachmentGroup.HROrders.Clear();
        _obj.OrderAttachmentGroup.HROrders.Add(e.NewValue);
      }
    }

  }
}