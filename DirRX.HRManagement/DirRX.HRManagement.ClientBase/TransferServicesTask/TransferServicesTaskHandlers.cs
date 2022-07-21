using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.TransferServicesTask;

namespace DirRX.HRManagement
{
  partial class TransferServicesTaskClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      PublicFunctions.Module.Remote.CheckPersonnelModuleLicense();
    }

  }
}