using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.VacationDuration;

namespace DirRX.HRManagement
{
  partial class VacationDurationAvailableDurationSharedCollectionHandlers
  {

    public virtual void AvailableDurationDeleted(Sungero.Domain.Shared.CollectionPropertyDeletedEventArgs e)
    {
      Functions.VacationDuration.Remote.FillTotalDaysInfo(_obj);
    }
  }

  partial class VacationDurationSharedHandlers
  {

  }

  partial class VacationDurationAvailableDurationSharedHandlers
  {

    public virtual void AvailableDurationDaysCountChanged(Sungero.Domain.Shared.IntegerPropertyChangedEventArgs e)
    {
      Functions.VacationDuration.Remote.FillTotalDaysInfo(_obj.VacationDuration);
    }

  }
}