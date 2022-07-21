using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Company;
using DirRX.HRManagement.Vacation;
using HRRoles = DirRX.HRManagement.Constants.Module.HRRoles;

namespace DirRX.HRManagement
{
  partial class VacationUiFilteringServerHandler<T>
  {

    public override IQueryable<T> Filtering(IQueryable<T> query, Sungero.Domain.UiFilteringEventArgs e)
    {
      query = base.Filtering(query, e);
      var vacationsList = query.ToList();
      
      // Фильтр видимости записей справочника.
      var visibleDepartments = Functions.Vacation.GetVisibleDepartments();
      var vacationsQuery = vacationsList.Where(v => visibleDepartments.Any(vd => vd.Equals(v.Department))).AsQueryable();
      return vacationsQuery;
    }
  }

  partial class VacationVacationKindPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> VacationKindFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      if (_obj.Employee != null & Functions.Vacation.GetVacationAccess(_obj) != DirRX.HRManagement.Constants.Vacation.VacationAccess.FullAccess)
      {
        var employeeVacationKinds = Functions.Module.GetAvailableVacationKinds(_obj.Employee, _obj.Year.Value);
        return query.Where(l => employeeVacationKinds.Contains(l));
      }
      return query;
    }
  }

  partial class VacationFilteringServerHandler<T>
  {

    public virtual IQueryable<Sungero.Company.IBusinessUnit> BusinessUnitFiltering(IQueryable<Sungero.Company.IBusinessUnit> query, Sungero.Domain.FilteringEventArgs e)
    {
      var visibleDepartments = Functions.Vacation.GetVisibleDepartments();
      var visibleBusinessUnits = visibleDepartments.Select(d => d.BusinessUnit);
      var businessUnitsList = query.ToList();
      var businessUnitsQuery = businessUnitsList.Where(bu => visibleBusinessUnits.Any(vb => vb.Equals(bu))).AsQueryable();
      return businessUnitsQuery;     
    }

    public virtual IQueryable<Sungero.Company.IEmployee> EmployeeFiltering(IQueryable<Sungero.Company.IEmployee> query, Sungero.Domain.FilteringEventArgs e)
    {
      var visibleDepartments = Functions.Vacation.GetVisibleDepartments();
      if (_filter.Department != null)
        query = query.Where(l => l.Department.Equals(_filter.Department));
      else if (_filter.BusinessUnit != null)
        query = query.Where(l => Equals(l.Department.BusinessUnit, _filter.BusinessUnit));
      var employeesList = query.ToList();
      var employeesQuery = employeesList.Where(em => visibleDepartments.Any(vd => vd.Equals(em.Department))).AsQueryable();
      return employeesQuery;
    }

    public virtual IQueryable<Sungero.Company.IDepartment> DepartmentFiltering(IQueryable<Sungero.Company.IDepartment> query, Sungero.Domain.FilteringEventArgs e)
    {
      var visibleDepartments = Functions.Vacation.GetVisibleDepartments();
      
      if (_filter.BusinessUnit != null)
        query = query.Where(d => d.BusinessUnit.Equals(_filter.BusinessUnit));
      var departmentsList = query.ToList();
      var departmentsQuery = departmentsList.Where(d => visibleDepartments.Any(vd => vd.Equals(d))).AsQueryable();
      return departmentsQuery;
    }

    public override IQueryable<T> Filtering(IQueryable<T> query, Sungero.Domain.FilteringEventArgs e)
    {
      if (_filter == null)
        return query;
      
      // Фильтр по состоянию.
      if (_filter.Active || _filter.Closed || _filter.Shifted || _filter.OnApproval || _filter.Approved || _filter.Canceled || _filter.Performing)
        query = query.Where(x => (_filter.Active && x.Status == Status.Active) ||
                            (_filter.Closed && x.Status == Status.Closed) ||
                            (_filter.Shifted && x.Status == Status.Shifted) ||
                            (_filter.OnApproval && x.Status == Status.OnApproval) ||
                            (_filter.Approved && x.Status == Status.Approved) ||
                            (_filter.Canceled && x.Status == Status.Canceled) ||
                            (_filter.Performing && x.Status == Status.Performing));
      
      // Фильтр по виду отпуска.
      if (_filter.VacationKind != null)
        query = query.Where(x => Equals(x.VacationKind, _filter.VacationKind));
      
      // Фильтр по нашей организации.
      if (_filter.BusinessUnit != null)
        query = query.Where(x => Equals(x.BusinessUnit, _filter.BusinessUnit));
      
      // Фильтр по подразделению.
      if (_filter.Department != null)
        query = query.Where(x => Equals(x.Department, _filter.Department));
      
      // Фильтр по сотруднику.
      if (_filter.Employee != null)
        query = query.Where(x => Equals(x.Employee, _filter.Employee));

      // Фильтр по дате начала отпуска.
      var periodDate = Calendar.Today;
      var startDateBeginPeriod = _filter.StartDateRangeFrom ?? Calendar.SqlMinValue;
      var startDateEndPeriod = _filter.StartDateRangeTo ?? Calendar.SqlMaxValue;
      
      if (_filter.StartDateThisYear)
      {
        startDateBeginPeriod = periodDate.BeginningOfYear();
        startDateEndPeriod = periodDate.EndOfYear();
      }
      
      if (_filter.StartDateNextYear)
      {
        periodDate = periodDate.NextYear();
        startDateBeginPeriod = periodDate.BeginningOfYear();
        startDateEndPeriod = periodDate.EndOfYear();
      }
      
      if (_filter.StartDateThisYear || _filter.StartDateNextYear || (_filter.StartDateRangeFrom != null || _filter.StartDateRangeTo != null))
        query = query.Where(x => x.StartDate.Between(startDateBeginPeriod, startDateEndPeriod));

      // Фильтр по дате окончания отпуска.
      periodDate = Calendar.Today;
      var finishDateBeginPeriod = _filter.FinDateRangeFrom ?? Calendar.SqlMinValue;
      var finishDateEndPeriod = _filter.FinDateRangeTo ?? Calendar.SqlMaxValue;
      
      if (_filter.FinDateThisYear)
      {
        finishDateBeginPeriod = periodDate.BeginningOfYear();
        finishDateEndPeriod = periodDate.EndOfYear();
      }
      
      if (_filter.FinDateNextYear)
      {
        periodDate = periodDate.NextYear();
        finishDateBeginPeriod = periodDate.BeginningOfYear();
        finishDateEndPeriod = periodDate.EndOfYear();
      }

      if (_filter.FinDateThisYear || _filter.FinDateNextYear || (_filter.FinDateRangeFrom != null || _filter.FinDateRangeTo != null))
        query = query.Where(x => x.FinDate.Between(finishDateBeginPeriod, finishDateEndPeriod));

      return query;
    }
  }

  partial class VacationEmployeePropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> EmployeeFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      if (_obj.Department != null)
        query = query.Where(l => l.Department.Equals(_obj.Department));
      
      var availableEmployees = Functions.Vacation.GetEmployeesForVacationFill();
      return query.Where(v => availableEmployees.Any(id => id == v.Id));
    }
  }

  partial class VacationServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      var vacationAccess = Functions.Vacation.GetVacationAccess(_obj);
      var vacationDate = Functions.Vacation.CreateVacationDateStructure(_obj);
      var warningList = Functions.Vacation.CheckNotCriticalVacationsDates(_obj, vacationDate);
      var errorList = Functions.Vacation.CheckCriticalVacationsDates(_obj, vacationDate);
      var employeeVacationKinds = Functions.Module.GetAvailableVacationKinds(_obj.Employee, _obj.Year.Value);
      if (!employeeVacationKinds.Contains(_obj.VacationKind))
        errorList.Add(DirRX.HRManagement.Vacations.Resources.NotAvailableVacationKindFormat(_obj.VacationKind.Name, _obj.Year.Value.Year));
      if (vacationAccess == DirRX.HRManagement.Constants.Vacation.VacationAccess.FullAccess)
      {
        var warnings = warningList.Concat(errorList);
        if (warnings.Any())
          e.AddWarning(string.Join(Environment.NewLine, warnings));
      }
      else if (vacationAccess == DirRX.HRManagement.Constants.Vacation.VacationAccess.Change ||
               vacationAccess == DirRX.HRManagement.Constants.Vacation.VacationAccess.LimitedChange)
      {
        if (warningList.Any())
          e.AddWarning(string.Join(Environment.NewLine, warningList));
        if (errorList.Any())
          e.AddError(string.Join(Environment.NewLine, errorList));
      }
    }

    public override void BeforeDelete(Sungero.Domain.BeforeDeleteEventArgs e)
    {
      var vacationAccess = Functions.Vacation.GetVacationAccess(_obj);
      if (vacationAccess == DirRX.HRManagement.Constants.Vacation.VacationAccess.LimitedChange ||
          vacationAccess == DirRX.HRManagement.Constants.Vacation.VacationAccess.Read)
        throw AppliedCodeException.Create(Vacations.Resources.DeleteError);
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      // Проверить сначала, может ли пользователь создать какую-либо запись
      var vacationAccess = Functions.Vacation.GetVacationAccess(_obj);
      if (vacationAccess == DirRX.HRManagement.Constants.Vacation.VacationAccess.LimitedChange ||
          vacationAccess == DirRX.HRManagement.Constants.Vacation.VacationAccess.Read)
        throw AppliedCodeException.Create(Vacations.Resources.CreateError);
      _obj.Status = Status.OnApproval;
      _obj.Year = Calendar.Today.NextYear();
      if (!_obj.State.IsCopied)
      {
        // Заполнить сотрудника только если он может создать запись именно для себя
        if (Employees.Current != null)
        {
          var user = new List<IUser>();
          user.Add(Users.Current);
          vacationAccess = Functions.Vacation.GetVacationAccess(_obj, user);
          if (vacationAccess == DirRX.HRManagement.Constants.Vacation.VacationAccess.FullAccess ||
              vacationAccess == DirRX.HRManagement.Constants.Vacation.VacationAccess.Change)
            _obj.Employee = Employees.Current;
        }
        _obj.VacationKind  = Functions.Module.GetAvailableVacationKinds(_obj.Employee, _obj.Year.Value).FirstOrDefault();
      }
    }
  }

  partial class VacationDepartmentPropertyFilteringServerHandler<T>
  {
    public virtual IQueryable<T> DepartmentFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      var availableDepartments = Functions.Vacation.GetDepartmentsForVacationFill();
      return query.Where(d => availableDepartments.Any(id => id == d.Id));
    }
  }

  partial class VacationCreatingFromServerHandler
  {

    public override void CreatingFrom(Sungero.Domain.CreatingFromEventArgs e)
    {
      base.CreatingFrom(e);
      e.Without(_info.Properties.NoteDate);
      e.Without(_info.Properties.Status);

      // Проверить, может ли текущий пользователь создавать отпуск для указанных в оригинальной записи подразделения и сотрудника, если нет - исключить из копируемых полей
      var availableEmployees = Functions.Vacation.GetEmployeesForVacationFill();
      if (!availableEmployees.Any(id => id == _source.Employee.Id))
        e.Without(_info.Properties.Employee);
      var availableDepartments = Functions.Vacation.GetDepartmentsForVacationFill();
      if (!availableDepartments.Any(id => id == _source.Department.Id))
        e.Without(_info.Properties.Department);
      // Аналогично исключать Вид отпуска
      var availableVacationKinds = Functions.Module.GetAvailableVacationKinds(_source.Employee, _source.Year.Value);
      if (!availableVacationKinds.Contains(_source.VacationKind))
        e.Without(_info.Properties.VacationKind);
    }
  }
}