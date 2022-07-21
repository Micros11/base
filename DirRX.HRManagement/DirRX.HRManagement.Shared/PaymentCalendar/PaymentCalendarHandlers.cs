using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.PaymentCalendar;

namespace DirRX.HRManagement
{
  partial class PaymentCalendarCalendarSharedHandlers
  {

    public virtual void CalendarDateChanged(Sungero.Domain.Shared.DateTimePropertyChangedEventArgs e)
    {
      _obj.DayOfWeek = e.NewValue == null ? string.Empty : e.NewValue.Value.ToString("ddd");
    }
  }
}