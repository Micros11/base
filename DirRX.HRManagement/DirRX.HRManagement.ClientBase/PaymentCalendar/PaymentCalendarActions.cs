using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.PaymentCalendar;

namespace DirRX.HRManagement.Client
{
  partial class PaymentCalendarActions
  {

    public virtual void FillDate(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      if (_obj.Calendar.Any())
      {
        if (Sungero.Docflow.PublicFunctions.Module.ShowConfirmationDialog(PaymentCalendars.Resources.ConfirmDialogFillDateMessage, null,
                                                                    null, Constants.PaymentCalendar.ConfirmDialogFillDateId))
          _obj.Calendar.Clear();
        else
          return;
      }
      Functions.PaymentCalendar.FillPaymentDay(_obj);
    }

    public virtual bool CanFillDate(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.Year.HasValue;
    }

  }

}