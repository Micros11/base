using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.PaymentCalendar;

namespace DirRX.HRManagement
{
  partial class PaymentCalendarServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      var year = _obj.Year.Value.Year;
      // Сообщить о дублирующих днях.
      var duplicate = _obj.Calendar.GroupBy(c => c.Date).Where(i => i.Count() > 1);
      foreach (var duplDate in duplicate)
        e.AddError(PaymentCalendars.Resources.DuplicateDateFormat(duplDate.First().Date.Value.ToShortDateString()));
      
      if (_obj.Calendar.Any(c => c.Date.Value.Year != year))
        e.AddError(PaymentCalendars.Resources.IncorrectYear);
        
      // Сообщить о дублирующих платежных календарях.       
      var paymentCalendars = PaymentCalendars.GetAll(c => !Equals(c.Id, _obj.Id) && c.Year.Equals(_obj.Year)); 
      if (paymentCalendars.Any())
      {
        if (_obj.BusinessUnits.Any())
        {
          foreach (var busUnit in _obj.BusinessUnits)
          {
            if (paymentCalendars.Where(c => c.BusinessUnits.Any(b => Equals(b.BusinessUnit, busUnit.BusinessUnit))).Any())
              e.AddError(PaymentCalendars.Resources.DuplicateYearAndBusinessUnitFormat(year, busUnit.BusinessUnit.Name));  
          }
        }
        else
          if (paymentCalendars.Any(c => !c.BusinessUnits.Any()))
            e.AddError(PaymentCalendars.Resources.DuplicateYearFormat(year));
      }
          
      // Обработать названия НОР для отображения в списке.
      var businessUnitNames = string.Empty;
      if (_obj.BusinessUnits.Any())
        businessUnitNames = string.Join("; ", _obj.BusinessUnits.OrderBy(l => l.BusinessUnit.Name).Select(l => l.BusinessUnit.Name));
      _obj.BusinessUnitNames = businessUnitNames;
      
      // Сформировать наименование календаря.
      _obj.Name = string.Format(DirRX.HRManagement.PaymentCalendars.Resources.PaymentCalendarName, businessUnitNames, businessUnitNames == string.Empty ? string.Empty : " ", year);
      
    }
    
    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      if (PaymentCalendars.GetAll().Any())
      {
        DateTime maxYear = PaymentCalendars.GetAll().Max(c => c.Year.Value);
        _obj.Year = maxYear.NextYear();
      }
      else 
        _obj.Year = Calendar.Today.BeginningOfYear();
    }
  }

}