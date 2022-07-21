using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.VacationDuration;

namespace DirRX.HRManagement
{
  partial class VacationDurationServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      Functions.VacationDuration.FillTotalDaysInfo(_obj);
    }

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      var employeeShortName = Sungero.Company.PublicFunctions.Employee.GetShortName(_obj.Employee, DeclensionCase.Genitive, false);
      
      var doubleRecords = VacationDurations.GetAll(l => Equals(l.Employee, _obj.Employee) && l.Year.Value == _obj.Year.Value && !Equals(l, _obj));
      if (doubleRecords.Any())
        e.AddError(VacationDurations.Resources.RecordExistErrorFormat(employeeShortName, _obj.Year.Value.Year));
      _obj.Name = VacationDurations.Resources.RecordNameFormat(employeeShortName, _obj.Year.Value.Year);
      
      // Нормализовать доступные виды отпуска: схлопнуть повторяющиеся, убрать неположительные
      var currentVacationDuration = new List<Structures.VacationDuration.VacationKindDuration>();
      foreach (var vd in _obj.AvailableDuration.GroupBy(v => v.VacationKind))
        currentVacationDuration.Add(Structures.VacationDuration.VacationKindDuration.Create(vd.Key, vd.Sum(x => x.DaysCount.Value)));
      _obj.AvailableDuration.Clear();
      foreach (var vd in currentVacationDuration.FindAll(l => l.DaysCount > 0))
      {
        var newRow = _obj.AvailableDuration.AddNew();
        newRow.VacationKind = vd.VacationKind;
        newRow.DaysCount = vd.DaysCount;
      }
    }
  }

  partial class VacationDurationFilteringServerHandler<T>
  {

    public virtual IQueryable<Sungero.Company.IDepartment> DepartmentFiltering(IQueryable<Sungero.Company.IDepartment> query, Sungero.Domain.FilteringEventArgs e)
    {
      if (_filter.BusinessUnit != null)
        query = query.Where(l => l.BusinessUnit.Equals(_filter.BusinessUnit));
      return query;
    }

    public override IQueryable<T> Filtering(IQueryable<T> query, Sungero.Domain.FilteringEventArgs e)
    {
      if (_filter == null)
        return query;
      
      // Фильтр по НОР.
      if (_filter.BusinessUnit != null)
        query = query.Where(x => Equals(x.Employee.Department.BusinessUnit, _filter.BusinessUnit));
      
      // Фильтр по подразделению.
      if (_filter.Department != null)
        query = query.Where(x => Equals(x.Employee.Department, _filter.Department));
      
      // Фильтр по году.
      if (_filter.ThisYear)
        query = query.Where(x => x.Year.Value.Year == Calendar.Today.Year);
      
      if (_filter.NextYear)
        query = query.Where(x => x.Year.Value.Year == Calendar.Today.NextYear().Year);

      if (_filter.Manual)
      {
        if (_filter.DateRangeFrom != null)
          query = query.Where(x => x.Year.Value.Year >= _filter.DateRangeFrom.Value.Year);
        
        if (_filter.DateRangeTo != null)
          query = query.Where(x => x.Year.Value.Year <= _filter.DateRangeTo.Value.Year);
      }
      return query;
    }
  }

}