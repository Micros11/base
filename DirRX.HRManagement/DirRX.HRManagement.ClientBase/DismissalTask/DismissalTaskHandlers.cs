﻿using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.DismissalTask;

namespace DirRX.HRManagement
{
  partial class DismissalTaskClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      PublicFunctions.Module.Remote.CheckPersonnelModuleLicense();
    }
  }

}