using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.Aspirant;

namespace DirRX.HRManagement
{
  partial class AspirantSharedHandlers
  {

    public virtual void PersonChanged(DirRX.HRManagement.Shared.AspirantPersonChangedEventArgs e)
    {
      Functions.Aspirant.UpdateName(_obj, e.NewValue);
    }
  }

}