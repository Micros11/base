using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.LocalRegulationDocument;

namespace DirRX.HRLite
{
  partial class LocalRegulationDocumentSharedHandlers
  {

    public override void LeadingDocumentChanged(Sungero.Docflow.Shared.OfficialDocumentLeadingDocumentChangedEventArgs e)
    {
      base.LeadingDocumentChanged(e);
      if (e.OldValue != null)
        _obj.Relations.RemoveFrom(Sungero.Docflow.PublicConstants.Module.AddendumRelationName, e.OldValue);
      if (e.NewValue != null)
        _obj.Relations.AddFrom(Sungero.Docflow.PublicConstants.Module.AddendumRelationName, e.NewValue);
    }

  }
}