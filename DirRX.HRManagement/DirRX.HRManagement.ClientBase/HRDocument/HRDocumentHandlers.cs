using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.HRDocument;

namespace DirRX.HRManagement
{
  partial class HRDocumentClientHandlers
  {

    public override void DocumentKindValueInput(Sungero.Docflow.Client.OfficialDocumentDocumentKindValueInputEventArgs e)
    {
      base.DocumentKindValueInput(e);
      Functions.HRDocument.SetVisibleProperties(_obj, e.NewValue);
    }

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      base.Showing(e);
      Functions.HRDocument.SetVisibleProperties(_obj, _obj.DocumentKind);
    }
  }
}