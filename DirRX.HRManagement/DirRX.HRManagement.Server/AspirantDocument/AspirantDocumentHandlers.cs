using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.AspirantDocument;

namespace DirRX.HRManagement
{
  partial class AspirantDocumentDepartmentPropertyFilteringServerHandler<T>
  {

    public override IQueryable<T> DepartmentFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      query = query.Where(d => d.Status == Sungero.CoreEntities.DatabookEntry.Status.Active);           
      if (_obj.BusinessUnit != null)
        query = query.Where(d => Equals(d.BusinessUnit, _obj.BusinessUnit));
      return query;
    }
  }

  partial class AspirantDocumentFilteringServerHandler<T>
  {

    public override IQueryable<T> Filtering(IQueryable<T> query, Sungero.Domain.FilteringEventArgs e)
    {
      if (_filter == null)
        return query;
      
      // Фильтр по виду документа.
      if (_filter.DocumentKind != null)
        query = query.Where(d => Equals(d.DocumentKind, _filter.DocumentKind));

      // Фильтр по кандидату.
      if (_filter.Aspirant != null)
        query = query.Where(d => Equals(d.Aspirant, _filter.Aspirant));
      
      // Фильтр по интервалу времени
      var periodBegin = Calendar.UserToday.AddDays(-7);
      var periodEnd = Calendar.UserToday.EndOfDay();
      
      if (_filter.LastWeek)
        periodBegin = Calendar.UserToday.AddDays(-7);
      
      if (_filter.LastMonth)
        periodBegin = Calendar.UserToday.AddDays(-30);
      
      if (_filter.Last90Days)
        periodBegin = Calendar.UserToday.AddDays(-90);
      
      if (_filter.ManualPeriod)
      {
        periodBegin = _filter.DateRangeFrom ?? Calendar.SqlMinValue;
        periodEnd = _filter.DateRangeTo ?? Calendar.SqlMaxValue;
      }
      
      query = query.Where(j => j.Created.Between(periodBegin, periodEnd));
                            
      return query;
    }
  }

  partial class AspirantDocumentServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      base.Created(e);
      if (!_obj.State.IsCopied)
        _obj.Aspirant = Functions.Aspirant.GetContextAspirant();
    }
  }
}