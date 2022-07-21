using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.PaymentCalendar;

namespace DirRX.HRManagement.Client
{
  partial class PaymentCalendarFunctions
  {
    /// <summary>
    /// Заполнить платежные дни. 
    /// </summary>       
    public virtual void FillPaymentDay()
    {
      var year = _obj.Year.Value;
      var calendarDay = Calendar.BeginningOfYear(year); 
      while (calendarDay.Year == year.Year)
      {
        var rowDay = _obj.Calendar.AddNew();
        rowDay.Date = calendarDay;
        rowDay.IsPayment = Functions.Module.Remote.IsPaymentDay(calendarDay);
        calendarDay = calendarDay.NextDay();
      }
    }

  }
}