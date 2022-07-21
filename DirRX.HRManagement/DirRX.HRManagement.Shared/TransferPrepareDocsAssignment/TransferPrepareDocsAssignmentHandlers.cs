using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.TransferPrepareDocsAssignment;

namespace DirRX.HRManagement
{
  partial class TransferPrepareDocsAssignmentSharedHandlers
  {

    public virtual void OrderChanged(DirRX.HRManagement.Shared.TransferPrepareDocsAssignmentOrderChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue && e.NewValue != null)
      {
        if (e.OldValue != null && _obj.HRDocumentAttachmentGroup.HROrders.Contains(e.OldValue))
          _obj.HRDocumentAttachmentGroup.HROrders.Remove(e.OldValue);
        if (!_obj.HRDocumentAttachmentGroup.HROrders.Contains(e.NewValue))
          _obj.HRDocumentAttachmentGroup.HROrders.Add(e.NewValue);
      }
    }

    public virtual void AdditionalAgreementChanged(DirRX.HRManagement.Shared.TransferPrepareDocsAssignmentAdditionalAgreementChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue && e.NewValue != null)
      {
        if (e.OldValue != null && _obj.HRDocumentAttachmentGroup.HRDocumentBases.Contains(e.OldValue))
          _obj.HRDocumentAttachmentGroup.HRDocumentBases.Remove(e.OldValue);
        if (!_obj.HRDocumentAttachmentGroup.HRDocumentBases.Contains(e.NewValue))
          _obj.HRDocumentAttachmentGroup.HRDocumentBases.Add(e.NewValue);
      }
    }
  }

}